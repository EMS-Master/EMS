using CalculationEngineContracts;
using CalculationEngineServ.DataBaseModels;
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
using System.Xml;
using TransactionContract;

namespace ScadaProcessingSevice
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
	public class ScadaProcessing : IScadaProcessingContract, ITransactionContract
    {
       
        private ModelResourcesDesc modelResourcesDesc;
        private NetworkModelGDAProxy gdaQueryProxy = null;
        private static List<AnalogLocation> energyConsumerAnalogs;
        private static List<AnalogLocation> generatorAnalogs;
		private static List<DiscreteLocation> energyConsumerDiscretes;
		private static List<DiscreteLocation> generatorDscretes;
		private readonly int START_ADDRESS_GENERATOR = 20;
		private readonly int START_ADDRESS_GENERATOR_DISCRETE = 10;
        private ConvertorHelper convertorHelper;
		private static Dictionary<long, float> previousGeneratorDiscretes;
		private static Dictionary<long, int> DiscretMaxVal;



		public ScadaProcessing()
        {
            convertorHelper = new ConvertorHelper();

            generatorAnalogs = new List<AnalogLocation>();
            energyConsumerAnalogs = new List<AnalogLocation>();
            modelResourcesDesc = new ModelResourcesDesc();
			previousGeneratorDiscretes = new Dictionary<long, float>(10);
            energyConsumerDiscretes = new List<DiscreteLocation>();
			generatorDscretes = new List<DiscreteLocation>();
            DiscretMaxVal = new Dictionary<long, int>();
        }
        //data collected from simulator should be passed through 
        //scadaProcessing,from scada, to calculationEngine for optimization
        public bool SendValues(byte[] value, bool[] valuesDiscrete, byte[] valuesWindSun)
        {
            string function = Enum.GetName(typeof(FunctionCode), value[0]);
            Console.WriteLine("Function executed: {0}", function);

			
			int arrayLength = value[1];
            byte[] data = new byte[arrayLength];
            byte[] windData = new byte[4];
            byte[] sunData = new byte[4];

            Console.WriteLine("Byte count: {0}", arrayLength);

			Array.Copy(value, 2, data, 0, arrayLength);
            Array.Copy(valuesWindSun, 2, windData, 0, 4);
            Array.Copy(valuesWindSun, 6, sunData, 0, 4);

            List<MeasurementUnit> energyConsumerMeasUnits = ParseDataToMeasurementUnit(energyConsumerAnalogs, data, 0, ModelCode.ENERGY_CONSUMER);
            
            List<MeasurementUnit> generatorMeasUnits = ParseDataToMeasurementUnit(generatorAnalogs, data, 0, ModelCode.GENERATOR);

			List<MeasurementUnit> energyConsumerMeasUnitsDiscrete = ParseDataToMeasurementUnitdiscrete(energyConsumerDiscretes, valuesDiscrete, 0, ModelCode.ENERGY_CONSUMER);

			List<MeasurementUnit> generatorMeasUnitsDiscrete = ParseDataToMeasurementUnitdiscrete(generatorDscretes, valuesDiscrete, 0, ModelCode.GENERATOR);

            List<MeasurementUnit> energyConsumerActive = SelectActive(energyConsumerMeasUnits, energyConsumerMeasUnitsDiscrete);
            List<MeasurementUnit> generatorsActive = SelectActive(generatorMeasUnits, generatorMeasUnitsDiscrete);

            float windSpeed = GetWindSpeed(windData, 4);
            float sunlight = GetSunlight(sunData, 4);

            LoadXMLFile();
            
            CheckWhichAreTurnedOff(energyConsumerMeasUnits, energyConsumerMeasUnitsDiscrete);
            CheckWhichAreTurnedOff(generatorMeasUnits, generatorMeasUnitsDiscrete);

            bool isSuccess = false;
            try
            {
                isSuccess = CalculationEngineProxy.Instance.OptimisationAlgorithm(energyConsumerActive, generatorsActive, windSpeed, sunlight);
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

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analog.PowerSystemResource) == DMSType.ENERGY_CONSUMER)
                    {
                        energyConsumerAnalogs.Add(new AnalogLocation()
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

					if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(discrete.PowerSystemResource) == DMSType.ENERGY_CONSUMER)
					{
                        energyConsumerDiscretes.Add(new DiscreteLocation()
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

            Console.WriteLine("EnergyConsumer:");
            foreach(AnalogLocation al in energyConsumerAnalogs)
            {
                Console.WriteLine(al.Analog.Mrid + " " + al.Analog.NormalValue);
				var dic = energyConsumerDiscretes.Find(x => x.Discrete.PowerSystemResource == al.Analog.PowerSystemResource);
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
                float MAX = convertorHelper.ConvertFromRawToEGUValue(analogLoc.Analog.MaxValue, 1, 1);
                float MIN = convertorHelper.ConvertFromRawToEGUValue(analogLoc.Analog.MinValue, 1, 1);
                bool alarmEGU = false;
                if (type.Equals(ModelCode.GENERATOR))
                {
                    alarmEGU = this.CheckForEGUAlarms(eguVal, MIN, MAX, analogLoc.Analog.PowerSystemResource);

                    if (!alarmEGU)
                    {
                        Alarm al = new Alarm();
                        al.Gid = analogLoc.Analog.PowerSystemResource;
                        al.AlarmValue = eguVal;
                        AlarmsEventsProxy.Instance.UpdateStatus(al, State.Cleared);

                        Alarm normalAlarm = new Alarm();
                        normalAlarm.AckState = AckState.Unacknowledged;
                        normalAlarm.CurrentState = string.Format("{0}", State.Active);
                        normalAlarm.Gid = analogLoc.Analog.PowerSystemResource;
                        normalAlarm.AlarmMessage = string.Format("Value on gid {0} returned to normal state", normalAlarm.Gid);
                        normalAlarm.AlarmTimeStamp = DateTime.Now;
                        normalAlarm.Severity = SeverityLevel.NORMAL;
                        normalAlarm.AlarmValue = eguVal;
                        normalAlarm.AlarmType = AlarmType.NORMAL;
                        normalAlarm.MaxValue = analogLoc.Analog.MaxValue;
                        normalAlarm.MinValue = analogLoc.Analog.MinValue;
                        //AlarmsEventsProxy.Instance.AddAlarm(normalAlarm);
                    }
                }

                MeasurementUnit measUnit = new MeasurementUnit();
				measUnit.Gid = analogLoc.Analog.PowerSystemResource;
				measUnit.MinValue = analogLoc.Analog.MinValue;
				measUnit.MaxValue = analogLoc.Analog.MaxValue;
				measUnit.CurrentValue = eguVal;
				measUnit.TimeStamp = DateTime.Now;
                measUnit.ScadaAddress = analogLoc.StartAddress;
				retList.Add(measUnit);
				
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
                measUnit.MaxValue = discreteLoc.Discrete.MaxValue;;
                measUnit.CurrentValue = value[discreteLoc.StartAddress - 1] ? 1 : 0;
				measUnit.TimeStamp = DateTime.Now;
                measUnit.ScadaAddress = discreteLoc.StartAddress;
				retList.Add(measUnit);

            previousGeneratorDiscretes[discreteLoc.Discrete.GlobalId] = measUnit.CurrentValue;
            }
            return retList;
		}
        private bool CheckDiscretAlarm (int value, float max, long gid)
        {
            bool retVal = false;
            Alarm ah = new Alarm(gid, value, -1, max, DateTime.Now);
            if (value > max)
            {
                ah.AlarmValue = value;
                ah.MaxValue = max;
                ah.AlarmType = AlarmType.DOM;
                ah.Severity = SeverityLevel.HIGH;                
                ah.AlarmMessage = string.Format("Value on input discret signal: {0:X} higher than maximum expected value", gid);
                AlarmsEventsProxy.Instance.AddAlarm(ah);
                retVal = true;
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Alarm on high raw limit on gid: {0:X}", gid);
                Console.WriteLine("Alarm on high raw limit on gid: {0:X}", gid);
            }
            return retVal;
        }
        private bool CheckForEGUAlarms(float value, float minEgu, float maxEgu, long gid)
        {
            bool retVal = false;
            Alarm ah = new Alarm(gid, value, minEgu, maxEgu, DateTime.Now);
            if (value < minEgu)
            {
                ah.AlarmType = AlarmType.LOW;
                if (value >= (minEgu - (minEgu * 20 / 100)))
                {
                    ah.Severity = SeverityLevel.MEDIUM;
                }
                else if (value >= (minEgu - (minEgu * 40 / 100)))
                {
                    ah.Severity = SeverityLevel.MINOR;
                }
                else
                {
                    ah.Severity = SeverityLevel.LOW;
                }
                ah.AlarmTimeStamp = DateTime.Now;
                ah.AlarmMessage = string.Format("Value on input analog signal: {0:X} lower than minimum expected value", gid);
                AlarmsEventsProxy.Instance.AddAlarm(ah);
                retVal = true;
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Alarm on low raw limit on gid: {0:X}", gid);
                Console.WriteLine("Alarm on low raw limit on gid: {0:X}", gid);
            }

            if (value > maxEgu)
            {
                ah.AlarmType = AlarmType.HIGH;
                if (value <= (maxEgu + (maxEgu * 20 / 100)))
                {
                    ah.Severity = SeverityLevel.MAJOR;
                }
                else if (value <= (maxEgu + (maxEgu * 40 / 100)))
                {
                    ah.Severity = SeverityLevel.HIGH;
                }
                else
                {
                    ah.Severity = SeverityLevel.CRITICAL;
                }
                ah.AlarmTimeStamp = DateTime.Now;
                ah.AlarmMessage = string.Format("Value on input analog signal: {0:X} higher than maximum expected value", gid);
                AlarmsEventsProxy.Instance.AddAlarm(ah);
                retVal = true;
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Alarm on high raw limit on gid: {0:X}", gid);
                Console.WriteLine("Alarm on high raw limit on gid: {0:X}", gid);
            }

            return retVal;
        }
        private void LoadXMLFile()
        {
            try
            {
                string path = System.IO.Path.GetFullPath("..\\..\\..\\..\\");

                XmlDocument doc = new XmlDocument();
                doc.Load(path + "ScadaProcessingSevice/MaxValDiscret.xml");

                XmlNodeList EnergyConsumerNode = doc.GetElementsByTagName("EnergyConsumer");
                foreach (XmlNode item in EnergyConsumerNode)
                {
                    long gid = long.Parse(item["Gid"].InnerText);
                    int maxTurnOn = int.Parse(item["MaxVal"].InnerText);
                    DiscretMaxVal[gid] = maxTurnOn;
                }
                XmlNodeList GeneratorStorageNode = doc.GetElementsByTagName("Generator");
                foreach (XmlNode item in GeneratorStorageNode)
                {
                    long gid = long.Parse(item["Gid"].InnerText);
                    int maxTurnOn = int.Parse(item["MaxVal"].InnerText);
                    DiscretMaxVal[gid] = maxTurnOn;
                }
            }
            catch
            {
                var message1 = string.Format("Error reading xml file");
                Console.WriteLine(message1);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message1);
            }
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

        private void CheckWhichAreTurnedOff(List<MeasurementUnit> analogs, List<MeasurementUnit> descretes)
        {
            List<DiscreteCounterModel> dcFromDB = CalculationEngineProxy.InstanceRepository.GetAllDiscreteCounters();
            foreach (var item in analogs)
            {
                var des = descretes.Find(x => x.Gid == item.Gid);
                var itemFromDb = dcFromDB.Find(x => x.Gid == item.Gid);
                if (des.CurrentValue == 1)
                {
                    if(itemFromDb != null)
                    {
                        if(itemFromDb.CurrentValue == false)
                        {
                            itemFromDb.CurrentValue = true;
                            CalculationEngineProxy.InstanceRepository.InsertOrUpdate(itemFromDb);
                        }
                    }
                }
                else if(des.CurrentValue == 0)
                {
                    if(itemFromDb == null)
                    {
                        CalculationEngineProxy.InstanceRepository.InsertOrUpdate(new DiscreteCounterModel() { Gid = item.Gid, Counter = 0, CurrentValue = false });
                    }
                    else
                    {
                        if (itemFromDb.CurrentValue == true)
                        {
                            itemFromDb.CurrentValue = false;
                            itemFromDb.Counter++;
                            int maxVal = DiscretMaxVal.FirstOrDefault(x => x.Key == itemFromDb.Gid).Value;
                            CheckDiscretAlarm(itemFromDb.Counter, maxVal, itemFromDb.Gid);
                            CalculationEngineProxy.InstanceRepository.InsertOrUpdate(itemFromDb);
                        }
                    }
                }
            }
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
    }
}
