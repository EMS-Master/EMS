using CalculationEngineContracts;
using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel.Meas;
using ModbusClient;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScadaProcessingSevice
{
    public class ScadaProcessing : IScadaProcessingContract
    {
       
        private ModelResourcesDesc modelResourcesDesc;
        private NetworkModelGDAProxy gdaQueryProxy = null;
        private static List<AnalogLocation> batteryStorageAnalogs;
        private static List<AnalogLocation> generatorAnalogs;
        private readonly int START_ADDRESS_GENERATOR = 50;
        private ConvertorHelper convertorHelper;
        private static Dictionary<long, float> previousGeneratorAnalogs;


        public ScadaProcessing()
        {
            convertorHelper = new ConvertorHelper();

            generatorAnalogs = new List<AnalogLocation>();
            batteryStorageAnalogs = new List<AnalogLocation>();
            modelResourcesDesc = new ModelResourcesDesc();
            previousGeneratorAnalogs = new Dictionary<long, float>(10);
        }
        //data collected from simulator should be passed through 
        //scadaProcessing,from scada, to calculationEngine for optimization
        public bool SendValues(byte[] value)
        {
            string function = Enum.GetName(typeof(FunctionCode), value[0]);
            Console.WriteLine("Function executed: {0}", function);

            int arrayLength = value[1];
            int windByteLength = 4;
            int sunByteLength = 4;
            byte[] windData = new byte[windByteLength];
            byte[] sunData = new byte[sunByteLength];
            byte[] data = new byte[arrayLength - windByteLength - sunByteLength];

            Console.WriteLine("Byte count: {0}", arrayLength);

            Array.Copy(value, 2, data, 0, arrayLength - windByteLength - sunByteLength);
            Array.Copy(value, 2 + arrayLength - windByteLength - sunByteLength, windData, 0, windByteLength);
            Array.Copy(value, 2 + arrayLength - sunByteLength, sunData, 0, sunByteLength);

            List<MeasurementUnit> enConsumMeasUnits = ParseDataToMeasurementUnit(batteryStorageAnalogs, data, 0, ModelCode.BATTERY_STORAGE);
            List<MeasurementUnit> generatorMeasUnits = ParseDataToMeasurementUnit(generatorAnalogs, data, 0, ModelCode.GENERATOR);

            float windSpeed = GetWindSpeed(windData, windByteLength);
            float sunlight = GetSunlight(sunData, sunByteLength);
            Console.WriteLine(value[0]);
            Console.WriteLine(value[1]);

            bool isSuccess = false;
            try
            {
                isSuccess = CalculationEngineProxy.Instance.OptimisationAlgorithm(enConsumMeasUnits, generatorMeasUnits, windSpeed, sunlight);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
            }

            if (isSuccess)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Successfuly sent items to CE.");
                Console.WriteLine("Successfuly sent items to CE.");
            }

            return isSuccess;
        }


        private float GetWindSpeed(byte[] windData, int byteLength)
        {
            float[] values = ModbusHelper.GetValueFromByteArray<float>(windData, byteLength);
            return values[0];
        }

        private float GetSunlight(byte[] sunData, int byteLength)
        {
            float[] values = ModbusHelper.GetValueFromByteArray<float>(sunData, byteLength);
            return values[0];
        }

        private List<MeasurementUnit> ParseDataToMeasurementUnit(List<AnalogLocation> analogList, byte[] value, int startAddress, ModelCode type)
        {
            List<MeasurementUnit> retList = new List<MeasurementUnit>();
            foreach (AnalogLocation analogLoc in analogList)
            {
                float[] values = ModbusHelper.GetValueFromByteArray<float>(value, analogLoc.LengthInBytes, startAddress + analogLoc.StartAddress * 2); // 2 jer su registri od 2 byte-a
                float eguVal = convertorHelper.ConvertFromRawToEGUValue(values[0], analogLoc.Analog.MinValue, analogLoc.Analog.MaxValue);
                Console.WriteLine("----------------------------");
                Console.WriteLine("Values: ");
                for (int i = 0; i < values.Length; i++)
                {
                    Console.WriteLine("[{0}] {1}\n", i, values[i]);

                }
                Console.WriteLine("EguValue: {0}", eguVal);
                Console.WriteLine("----------------------------");
                if (type.Equals(ModelCode.GENERATOR))
                {
                    //al
                }
                

                if (analogLoc.Analog.Mrid.Equals("Analog_sm_2"))
                {
                    using (var txtWriter = new StreamWriter("PointsReport.txt", true))
                    {
                        txtWriter.WriteLine(" [" + DateTime.Now + "] " + " The value for " + analogLoc.Analog.Mrid + " before the conversion was: " + values[0] + ", and after:" + eguVal);
                        txtWriter.Dispose();
                    }
                }

                MeasurementUnit measUnit = new MeasurementUnit();
                measUnit.Gid = analogLoc.Analog.PowerSystemResource;
                measUnit.MinValue = analogLoc.Analog.MinValue;
                measUnit.MaxValue = analogLoc.Analog.MaxValue;
                measUnit.CurrentValue = eguVal;
                measUnit.TimeStamp = DateTime.Now;
                retList.Add(measUnit);

                previousGeneratorAnalogs[analogLoc.Analog.GlobalId] = eguVal;
            }

            return retList;
        }





        public bool InitiateIntegrityUpdate()
        {
            List<ModelCode> properties = new List<ModelCode>(10);
            ModelCode modelCode = ModelCode.ANALOG;
            
            int resourcesLeft = 0;
            int numberOfResources = 2;

            List<ResourceDescription> retList = new List<ResourceDescription>(5);
            try
            {
                properties = modelResourcesDesc.GetAllPropertyIds(modelCode);

                var iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCode, properties);
                resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                }
                NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);
            }
            catch (Exception e)
            {

                NetworkModelGDAProxy.Instance = null;
                Thread.Sleep(1000);
                InitiateIntegrityUpdate();

                return false;
            }

            try
            {
                foreach (ResourceDescription rd in retList)
                {
                    Analog analog = ResourcesDescriptionConverter.ConvertTo<Analog>(rd);

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analog.PowerSystemResource) == DMSType.BATTERY_STORAGE)
                    {
                        batteryStorageAnalogs.Add(new AnalogLocation()
                        {
                            Analog = analog,
                            StartAddress = batteryStorageAnalogs.Count * 2,
                            Length = 2,
                            LengthInBytes = 4
                        });
                    }
                    else
                    {
                        generatorAnalogs.Add(new AnalogLocation()
                        {
                            Analog = analog,
                            StartAddress = START_ADDRESS_GENERATOR + generatorAnalogs.Count * 2,
                            Length = 2,
                            LengthInBytes = 4
                        });
                    }
                }

              
            }
            catch (Exception e)
            {
                var message1 = string.Format("Conversion to Analog object failed.\n\t{0}", e.Message);
                Console.WriteLine(message1);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message1);
                return false;
            }

           var message = string.Format("Integrity update: Number of {0} values: {1}", modelCode.ToString(), retList.Count.ToString());
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            Console.WriteLine("Integrity update: Number of {0} values: {1}", modelCode.ToString(), retList.Count.ToString());

            Console.WriteLine("BatteryStorafge:");
            foreach(AnalogLocation al in batteryStorageAnalogs)
            {
                Console.WriteLine(al.Analog.Mrid + " " + al.Analog.NormalValue);
            }
            Console.WriteLine("Generator:");
            foreach (AnalogLocation al in generatorAnalogs)
            {
                Console.WriteLine(al.Analog.Mrid + " " + al.Analog.NormalValue);
            }

            return true;
        }

    }
}
