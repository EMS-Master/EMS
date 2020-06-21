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
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionContract;

namespace ScadaProcessingSevice
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
	public class ScadaProcessing : IScadaProcessingContract, ITransactionContract
    {
       
        private ModelResourcesDesc modelResourcesDesc;
        private NetworkModelGDAProxy gdaQueryProxy = null;
        private static List<AnalogLocation> batteryStorageAnalogs;
        private static List<AnalogLocation> generatorAnalogs;
		private static List<DiscreteLocation> batteryStorageDiscretes;
		private static List<DiscreteLocation> generatorDscretes;
		private readonly int START_ADDRESS_GENERATOR = 20;
		private readonly int START_ADDRESS_GENERATOR_DISCRETE = 10;
        private ConvertorHelper convertorHelper;
        private static Dictionary<long, float> previousGeneratorAnalogs;
		private static Dictionary<long, float> previousGeneratorDiscretes;


		public ScadaProcessing()
        {
            convertorHelper = new ConvertorHelper();

            generatorAnalogs = new List<AnalogLocation>();
            batteryStorageAnalogs = new List<AnalogLocation>();
            modelResourcesDesc = new ModelResourcesDesc();
            previousGeneratorAnalogs = new Dictionary<long, float>(10);
			previousGeneratorDiscretes = new Dictionary<long, float>(10);
			batteryStorageDiscretes = new List<DiscreteLocation>();
			generatorDscretes = new List<DiscreteLocation>();
        }
        //data collected from simulator should be passed through 
        //scadaProcessing,from scada, to calculationEngine for optimization
        public bool SendValues(byte[] value, bool[] valuesDiscrete)
        {
            string function = Enum.GetName(typeof(FunctionCode), value[0]);
            Console.WriteLine("Function executed: {0}", function);

			
			int arrayLength = value[1];
            byte[] data = new byte[arrayLength];

            Console.WriteLine("Byte count: {0}", arrayLength);

			Array.Copy(value, 2, data, 0, arrayLength);
			
			List<MeasurementUnit> batteryStorageMeasUnits = ParseDataToMeasurementUnit(batteryStorageAnalogs, data, 0, ModelCode.BATTERY_STORAGE);
            
            List<MeasurementUnit> generatorMeasUnits = ParseDataToMeasurementUnit(generatorAnalogs, data, 0, ModelCode.GENERATOR);

			List<MeasurementUnit> batteryStorageMeasUnitsDiscrete = ParseDataToMeasurementUnitdiscrete(batteryStorageDiscretes, valuesDiscrete, 0, ModelCode.BATTERY_STORAGE);

			List<MeasurementUnit> generatorMeasUnitsDiscrete = ParseDataToMeasurementUnitdiscrete(generatorDscretes, valuesDiscrete, 0, ModelCode.GENERATOR);

            List<MeasurementUnit> batteryStorageActive = SelectActive(batteryStorageMeasUnits, batteryStorageMeasUnitsDiscrete);
            List<MeasurementUnit> generatorsActive = SelectActive(generatorMeasUnits, generatorMeasUnitsDiscrete);

            bool isSuccess = false;
            try
            {
                isSuccess = CalculationEngineProxy.Instance.OptimisationAlgorithm(batteryStorageActive, generatorsActive);
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
        

        public bool InitiateIntegrityUpdate()
        {
            List<ModelCode> properties = new List<ModelCode>(10); //analog has 10 properties
            List<ModelCode> propertiesDiscrete = new List<ModelCode>(10);
            ModelCode modelCode = ModelCode.ANALOG;
            ModelCode modelCodeDiscrete = ModelCode.DISCRETE;
            
            int resourcesLeft = 0;
            int numberOfResources = 2;

            List<ResourceDescription> retList = new List<ResourceDescription>(5);
            List<ResourceDescription> retListDiscrete = new List<ResourceDescription>(5);
            try
            {
                properties = modelResourcesDesc.GetAllPropertyIds(modelCode);
				propertiesDiscrete = modelResourcesDesc.GetAllPropertyIds(modelCodeDiscrete);

				var iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCode, properties);
                resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                }
                NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);

				var iteratorIdDiscrete = NetworkModelGDAProxy.Instance.GetExtentValues(modelCodeDiscrete, propertiesDiscrete);
				resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorIdDiscrete);

				while (resourcesLeft > 0)
				{
					List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorIdDiscrete);
					retListDiscrete.AddRange(rds);
					resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorIdDiscrete);
				}
				NetworkModelGDAProxy.Instance.IteratorClose(iteratorIdDiscrete);
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
                            StartAddress = Int32.Parse(analog.ScadaAddress.Split('_')[1]),
                            Length = 2,
                            LengthInBytes = 4
                        });
                    }
                    else
                    {
                        generatorAnalogs.Add(new AnalogLocation()
                        {
                            Analog = analog,
                            StartAddress = Int32.Parse(analog.ScadaAddress.Split('_')[1]),
                            Length = 2,
                            LengthInBytes = 4
                        });
                    }
                }

				foreach (ResourceDescription rd in retListDiscrete)
				{
					Discrete discrete = ResourcesDescriptionConverter.ConvertTo<Discrete>(rd);

					if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(discrete.PowerSystemResource) == DMSType.BATTERY_STORAGE)
					{
						batteryStorageDiscretes.Add(new DiscreteLocation()
						{
							Discrete = discrete,
							StartAddress = Int32.Parse(discrete.ScadaAddress.Split('_')[1]),
							Length = 1,
							LengthInBytes = 2
						});
					}
					else
					{
						generatorDscretes.Add(new DiscreteLocation()
						{
							Discrete = discrete,
							StartAddress = Int32.Parse(discrete.ScadaAddress.Split('_')[1]),
							Length = 1,
							LengthInBytes = 2
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

            Console.WriteLine("BatteryStorage:");
            foreach(AnalogLocation al in batteryStorageAnalogs)
            {
                Console.WriteLine(al.Analog.Mrid + " " + al.Analog.NormalValue);
				var dic = batteryStorageDiscretes.Find(x => x.Discrete.PowerSystemResource == al.Analog.PowerSystemResource);
				Console.WriteLine(dic.Discrete.Mrid + " " + dic.Discrete.NormalValue);
            }
            Console.WriteLine("Generator:");
            foreach (AnalogLocation al in generatorAnalogs)
            {
                Console.WriteLine(al.Analog.Mrid + " " + al.Analog.NormalValue);
				var dic = generatorDscretes.Find(x => x.Discrete.PowerSystemResource == al.Analog.PowerSystemResource);
				Console.WriteLine(dic.Discrete.Mrid + " " + dic.Discrete.NormalValue);
			}

            return true;
        }

        public UpdateResult Prepare(ref Delta delta)
        {
            throw new NotImplementedException();
        }

        public bool Commit()
        {
            throw new NotImplementedException();
        }

        public bool Rollback()
        {
            throw new NotImplementedException();
        }

		private List<MeasurementUnit> ParseDataToMeasurementUnit(List<AnalogLocation> analogList, byte[] value, int startAddress, ModelCode type)
		{
			List<MeasurementUnit> retList = new List<MeasurementUnit>();
			foreach (AnalogLocation analogLoc in analogList)
			{
				float[] values = ModbusHelper.GetValueFromByteArray<float>(value, analogLoc.LengthInBytes, (analogLoc.StartAddress - 1) * 4);
                Console.WriteLine("Broj: {0}", values[0]);
                float eguVal = convertorHelper.ConvertFromRawToEGUValue(values[0], analogLoc.Analog.MinValue, analogLoc.Analog.MaxValue);
				
				MeasurementUnit measUnit = new MeasurementUnit();
				measUnit.Gid = analogLoc.Analog.PowerSystemResource;
				measUnit.MinValue = analogLoc.Analog.MinValue;
				measUnit.MaxValue = analogLoc.Analog.MaxValue;
				measUnit.CurrentValue = eguVal;
				measUnit.TimeStamp = DateTime.Now;
                measUnit.ScadaAddress = analogLoc.StartAddress;
				retList.Add(measUnit);
				
				previousGeneratorAnalogs[analogLoc.Analog.GlobalId] = eguVal;
			}

			return retList;
		}


		private List<MeasurementUnit> ParseDataToMeasurementUnitdiscrete(List<DiscreteLocation> discreteList, bool[] value, int startAddress, ModelCode type)
		{
			List<MeasurementUnit> retList = new List<MeasurementUnit>();
			foreach (DiscreteLocation discreteLoc in discreteList)
			{
				MeasurementUnit measUnit = new MeasurementUnit();
				measUnit.Gid = discreteLoc.Discrete.PowerSystemResource;
				measUnit.MinValue = discreteLoc.Discrete.MinValue;
				measUnit.MaxValue = discreteLoc.Discrete.MaxValue;
				measUnit.CurrentValue = value[discreteLoc.StartAddress - 1] ? 1 : 0;
				measUnit.TimeStamp = DateTime.Now;
                measUnit.ScadaAddress = discreteLoc.StartAddress;
				retList.Add(measUnit);

				previousGeneratorDiscretes[discreteLoc.Discrete.GlobalId] = measUnit.CurrentValue;
			}
			return retList;
		}

        private List<MeasurementUnit> SelectActive(List<MeasurementUnit> analogs, List<MeasurementUnit> descretes)
        {
           List<MeasurementUnit> returnList = new List<MeasurementUnit>();

           foreach(var item in analogs)
           {
                var des = descretes.Find(x => x.Gid == item.Gid);
                if (des.CurrentValue == 1)
                    returnList.Add(item);
           }
            return returnList;
        }

    }
}
