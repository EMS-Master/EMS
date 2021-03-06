using AlarmsEventsContract.ServiceFabricProxy;
using CalculationEngineContracts.ServiceFabricProxy;
using CalculationEngineServ.DataBaseModels;
using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts.ServiceFabricProxy;
using FTN.Services.NetworkModelService.DataModel.Meas;
using ModbusClient;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionContract;

namespace ScadaProcessingCloudServ
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ScadaProccessingCloud : IScadaProcessingContract, ITransactionContract
    {

        private ModelResourcesDesc modelResourcesDesc;
        private static List<AnalogLocation> energyConsumerAnalogs;
        private static List<AnalogLocation> energyConsumerAnalogsCopy;
        private static List<AnalogLocation> generatorAnalogs;
        private static List<AnalogLocation> generatorAnalogsCopy;
        private static List<DiscreteLocation> energyConsumerDiscretes;
        private static List<DiscreteLocation> energyConsumerDiscretesCopy;
        private static List<DiscreteLocation> generatorDscretes;
        private static List<DiscreteLocation> generatorDscretesCopy;
        private ConvertorHelper convertorHelper;
        //private static Dictionary<long, float> previousGeneratorDiscretes;
        private static Dictionary<Tuple<long, string>, int> DiscretMaxVal;
        private UpdateResult updateResult;
        private ITransactionCallback transactionCallback;
        private AlarmsEventsSfProxy alarmsEventsSfProxy;

        public ScadaProccessingCloud()
        {
            convertorHelper = new ConvertorHelper();

            generatorAnalogs = new List<AnalogLocation>();
            generatorAnalogsCopy = new List<AnalogLocation>();
            energyConsumerAnalogs = new List<AnalogLocation>();
            energyConsumerAnalogsCopy = new List<AnalogLocation>();
            modelResourcesDesc = new ModelResourcesDesc();
            //previousGeneratorDiscretes = new Dictionary<long, float>(10);
            energyConsumerDiscretes = new List<DiscreteLocation>();
            energyConsumerDiscretesCopy = new List<DiscreteLocation>();
            generatorDscretes = new List<DiscreteLocation>();
            generatorDscretesCopy = new List<DiscreteLocation>();
            DiscretMaxVal = new Dictionary<Tuple<long, string>, int>();
            alarmsEventsSfProxy = new AlarmsEventsSfProxy();
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
            List<float> sumOfConsumers = energyConsumerMeasUnits.Select(x => x.CurrentValue).ToList();
            float sss = sumOfConsumers.Sum();

            List<MeasurementUnit> generatorsActive = SelectActive(generatorMeasUnits, generatorMeasUnitsDiscrete);
            List<float> sumOfGen = generatorMeasUnits.Select(x => x.CurrentValue).ToList();
            float sssGen = sumOfGen.Sum();

            float windSpeed = GetWindSpeed(windData, 4);
            float sunlight = GetSunlight(sunData, 4);

            //LoadXMLFile();

            CheckWhichAreTurnedOff(energyConsumerMeasUnits, energyConsumerMeasUnitsDiscrete);
            CheckWhichAreTurnedOff(generatorMeasUnits, generatorMeasUnitsDiscrete);

            bool isSuccess = false;
            try
            {
                CalculationEngineSfProxy calculationEngineSfProxy = new CalculationEngineSfProxy();
                isSuccess = calculationEngineSfProxy.OptimisationAlgorithm(energyConsumerActive, generatorsActive, windSpeed, sunlight);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                //ServiceEventSource.Current.Message(ex.Message);

            }

            if (isSuccess)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Successfuly sent items to CE.");
                Console.WriteLine("Successfuly sent items to CE.");
                ServiceEventSource.Current.Message("Successfuly sent items to CE.");

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
            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();

            try
            {
                properties = modelResourcesDesc.GetAllPropertyIds(modelCode);
                propertiesDiscrete = modelResourcesDesc.GetAllPropertyIds(modelCodeDiscrete);

                var iteratorId = networkModelGDASfProxy.GetExtentValues(modelCode, properties);
                resourcesLeft = networkModelGDASfProxy.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = networkModelGDASfProxy.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = networkModelGDASfProxy.IteratorResourcesLeft(iteratorId);
                }
                networkModelGDASfProxy.IteratorClose(iteratorId);

                var iteratorIdDiscrete = networkModelGDASfProxy.GetExtentValues(modelCodeDiscrete, propertiesDiscrete);
                resourcesLeft = networkModelGDASfProxy.IteratorResourcesLeft(iteratorIdDiscrete);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = networkModelGDASfProxy.IteratorNext(numberOfResources, iteratorIdDiscrete);
                    retListDiscrete.AddRange(rds);
                    resourcesLeft = networkModelGDASfProxy.IteratorResourcesLeft(iteratorIdDiscrete);
                }
                networkModelGDASfProxy.IteratorClose(iteratorIdDiscrete);
            }
            catch (Exception e)
            {

                //NetworkModelGDAProxy.Instance = null;
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
                //ServiceEventSource.Current.Message(message1);
                return false;
            }

            var message = string.Format("Integrity update: Number of {0} values: {1}", modelCode.ToString(), retList.Count.ToString());
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            //ServiceEventSource.Current.Message(message);
            Console.WriteLine("Integrity update: Number of {0} values: {1}", modelCode.ToString(), retList.Count.ToString());

            Console.WriteLine("EnergyConsumer:");
            foreach (AnalogLocation al in energyConsumerAnalogs)
            {
                Console.WriteLine(al.Analog.Mrid + " " + al.Analog.NormalValue);
                var dic = energyConsumerDiscretes.Find(x => x.Discrete.PowerSystemResource == al.Analog.PowerSystemResource);
                Console.WriteLine(dic.Discrete.Mrid + " " + dic.Discrete.NormalValue);
            }

            Console.WriteLine("Generator:");
            //ServiceEventSource.Current.Message("Generator:");
            foreach (AnalogLocation al in generatorAnalogs)
            {
                Console.WriteLine(al.Analog.Mrid + " " + al.Analog.NormalValue);
                var dic = generatorDscretes.Find(x => x.Discrete.PowerSystemResource == al.Analog.PowerSystemResource);
                Console.WriteLine(dic.Discrete.Mrid + " " + dic.Discrete.NormalValue);
                //ServiceEventSource.Current.Message(dic.Discrete.Mrid + " " + dic.Discrete.NormalValue);
            }

            return true;
        }

        public UpdateResult Prepare(ref Delta delta)
        {
            try
            {
                transactionCallback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
                updateResult = new UpdateResult();

                generatorAnalogsCopy.Clear();
                energyConsumerAnalogsCopy.Clear();
                generatorDscretesCopy.Clear();
                energyConsumerDiscretesCopy.Clear();

                // napravi kopiju od originala
                foreach (AnalogLocation alocation in generatorAnalogs)
                {
                    generatorAnalogsCopy.Add(alocation.Clone() as AnalogLocation);
                }

                foreach (AnalogLocation alocation in energyConsumerAnalogs)
                {
                    energyConsumerAnalogsCopy.Add(alocation.Clone() as AnalogLocation);
                }
                foreach (DiscreteLocation dlocation in generatorDscretes)
                {
                    generatorDscretesCopy.Add(dlocation.Clone() as DiscreteLocation);
                }

                foreach (DiscreteLocation dlocation in energyConsumerDiscretes)
                {
                    energyConsumerDiscretesCopy.Add(dlocation.Clone() as DiscreteLocation);
                }

                Analog analog = null;
                Discrete discrete = null;
                foreach (ResourceDescription rd in delta.InsertOperations)
                {

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id) == (DMSType.ANALOG))
                    {
                        foreach (Property prop in rd.Properties)
                        {
                            analog = ResourcesDescriptionConverter.ConvertTo<Analog>(rd);

                            if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analog.PowerSystemResource) == DMSType.ENERGY_CONSUMER)
                            {
                                energyConsumerAnalogsCopy.Add(new AnalogLocation()
                                {
                                    Analog = analog,
                                    StartAddress = Int32.Parse(analog.ScadaAddress.Split('_')[1]),
                                    Length = 2,
                                    LengthInBytes = 4
                                });
                            }
                            else
                            {
                                generatorAnalogsCopy.Add(new AnalogLocation()
                                {
                                    Analog = analog,
                                    StartAddress = Int32.Parse(analog.ScadaAddress.Split('_')[1]), // float value 4 bytes
                                    Length = 2,
                                    LengthInBytes = 4
                                });
                            }
                            break;
                        }
                    }
                    else if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id) == (DMSType.DISCRETE))
                    {
                        foreach (Property prop in rd.Properties)
                        {
                            discrete = ResourcesDescriptionConverter.ConvertTo<Discrete>(rd);

                            if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(discrete.PowerSystemResource) == DMSType.ENERGY_CONSUMER)
                            {
                                energyConsumerDiscretesCopy.Add(new DiscreteLocation()
                                {
                                    Discrete = discrete,
                                    StartAddress = Int32.Parse(discrete.ScadaAddress.Split('_')[1]),
                                    Length = 1,
                                    LengthInBytes = 2
                                });
                            }
                            else
                            {
                                generatorDscretesCopy.Add(new DiscreteLocation()
                                {
                                    Discrete = discrete,
                                    StartAddress = Int32.Parse(discrete.ScadaAddress.Split('_')[1]), // float value 4 bytes
                                    Length = 1,
                                    LengthInBytes = 2
                                });
                            }
                            break;
                        }
                    }

                }

                foreach (ResourceDescription rd in delta.UpdateOperations)
                {

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id) == (DMSType.ANALOG))
                    {
                        foreach (Property prop in rd.Properties)
                        {
                            analog = ResourcesDescriptionConverter.ConvertTo<Analog>(rd);
                            if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analog.PowerSystemResource) == DMSType.ENERGY_CONSUMER)
                            {
                                if (ContainsMrid(analog, energyConsumerAnalogsCopy))
                                {
                                    foreach (AnalogLocation al in energyConsumerAnalogsCopy)
                                    {
                                        if (al.Analog.Mrid.Equals(analog.Mrid))
                                        {
                                            if (analog.MaxValue != al.Analog.MaxValue && analog.MaxValue != 0)
                                            {
                                                al.Analog.MaxValue = analog.MaxValue;
                                            }
                                            else if (analog.MeasurmentType != al.Analog.MeasurmentType && analog.MeasurmentType.ToString() != "")
                                            {
                                                al.Analog.MeasurmentType = analog.MeasurmentType;
                                            }
                                            else if (analog.MinValue != al.Analog.MinValue && analog.MinValue != 0)
                                            {
                                                al.Analog.MinValue = analog.MinValue;
                                            }
                                            else if (analog.Name != al.Analog.Name && analog.Name.ToString() != "")
                                            {
                                                al.Analog.Name = analog.Name;
                                            }
                                            else if (analog.AliasName != al.Analog.AliasName && analog.AliasName.ToString() != "")
                                            {
                                                al.Analog.AliasName = analog.AliasName;
                                            }
                                            else if (analog.NormalValue != al.Analog.NormalValue && analog.NormalValue != 0)
                                            {
                                                al.Analog.NormalValue = analog.NormalValue;
                                            }
                                            else if (analog.PowerSystemResource != al.Analog.PowerSystemResource && analog.PowerSystemResource.ToString() != "")
                                            {
                                                al.Analog.PowerSystemResource = analog.PowerSystemResource;
                                            }
                                            else if (analog.Direction != al.Analog.Direction && analog.Direction.ToString() != "")
                                            {
                                                al.Analog.Direction = analog.Direction;
                                            }
                                            else if (analog.ScadaAddress != al.Analog.ScadaAddress && analog.ScadaAddress.ToString() != "")
                                            {
                                                al.Analog.ScadaAddress = analog.ScadaAddress;
                                            }
                                        }
                                    }
                                }
                            }
                            else if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analog.PowerSystemResource) == DMSType.GENERATOR)
                            {

                                if (ContainsMrid(analog, generatorAnalogsCopy))
                                {
                                    foreach (AnalogLocation al in generatorAnalogsCopy)
                                    {
                                        if (al.Analog.Mrid.Equals(analog.Mrid))
                                        {
                                            if (analog.MaxValue != al.Analog.MaxValue && analog.MaxValue != 0)
                                            {
                                                al.Analog.MaxValue = analog.MaxValue;
                                            }
                                            else if (analog.MeasurmentType != al.Analog.MeasurmentType && analog.MeasurmentType.ToString() != "")
                                            {
                                                al.Analog.MeasurmentType = analog.MeasurmentType;
                                            }
                                            else if (analog.MinValue != al.Analog.MinValue && analog.MinValue != 0)
                                            {
                                                al.Analog.MinValue = analog.MinValue;
                                            }
                                            else if (analog.Name != al.Analog.Name && analog.Name.ToString() != "")
                                            {
                                                al.Analog.Name = analog.Name;
                                            }
                                            else if (analog.AliasName != al.Analog.AliasName && analog.AliasName.ToString() != "")
                                            {
                                                al.Analog.AliasName = analog.AliasName;
                                            }
                                            else if (analog.NormalValue != al.Analog.NormalValue && analog.NormalValue != 0)
                                            {
                                                al.Analog.NormalValue = analog.NormalValue;
                                            }
                                            else if (analog.PowerSystemResource != al.Analog.PowerSystemResource && analog.PowerSystemResource.ToString() != "")
                                            {
                                                al.Analog.PowerSystemResource = analog.PowerSystemResource;
                                            }
                                            else if (analog.Direction != al.Analog.Direction && analog.Direction.ToString() != "")
                                            {
                                                al.Analog.Direction = analog.Direction;
                                            }
                                            else if (analog.ScadaAddress != al.Analog.ScadaAddress && analog.ScadaAddress.ToString() != "")
                                            {
                                                al.Analog.ScadaAddress = analog.ScadaAddress;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id) == (DMSType.DISCRETE))
                    {
                        foreach (Property prop in rd.Properties)
                        {
                            discrete = ResourcesDescriptionConverter.ConvertTo<Discrete>(rd);
                            if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(discrete.PowerSystemResource) == DMSType.ENERGY_CONSUMER)
                            {
                                if (ContainsMridDiscrete(discrete, energyConsumerDiscretesCopy))
                                {
                                    foreach (DiscreteLocation al in energyConsumerDiscretesCopy)
                                    {
                                        if (al.Discrete.Mrid.Equals(discrete.Mrid))
                                        {
                                            if (discrete.MaxValue != al.Discrete.MaxValue && discrete.MaxValue != 0)
                                            {
                                                al.Discrete.MaxValue = discrete.MaxValue;
                                            }
                                            else if (discrete.MeasurmentType != al.Discrete.MeasurmentType && discrete.MeasurmentType.ToString() != "")
                                            {
                                                al.Discrete.MeasurmentType = discrete.MeasurmentType;
                                            }
                                            else if (discrete.MinValue != al.Discrete.MinValue && discrete.MinValue != 0)
                                            {
                                                al.Discrete.MinValue = discrete.MinValue;
                                            }
                                            else if (discrete.Name != al.Discrete.Name && discrete.Name.ToString() != "")
                                            {
                                                al.Discrete.Name = discrete.Name;
                                            }
                                            else if (discrete.AliasName != al.Discrete.AliasName && discrete.AliasName.ToString() != "")
                                            {
                                                al.Discrete.AliasName = discrete.AliasName;
                                            }
                                            else if (discrete.NormalValue != al.Discrete.NormalValue && discrete.NormalValue != 0)
                                            {
                                                al.Discrete.NormalValue = discrete.NormalValue;
                                            }
                                            else if (discrete.PowerSystemResource != al.Discrete.PowerSystemResource && discrete.PowerSystemResource.ToString() != "")
                                            {
                                                al.Discrete.PowerSystemResource = discrete.PowerSystemResource;
                                            }
                                            else if (discrete.Direction != al.Discrete.Direction && discrete.Direction.ToString() != "")
                                            {
                                                al.Discrete.Direction = discrete.Direction;
                                            }
                                            else if (discrete.ScadaAddress != al.Discrete.ScadaAddress && discrete.ScadaAddress.ToString() != "")
                                            {
                                                al.Discrete.ScadaAddress = discrete.ScadaAddress;
                                            }
                                        }
                                    }
                                }
                            }
                            else if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(discrete.PowerSystemResource) == DMSType.GENERATOR)
                            {
                                if (ContainsMridDiscrete(discrete, generatorDscretesCopy))
                                {
                                    foreach (DiscreteLocation al in generatorDscretesCopy)
                                    {
                                        if (al.Discrete.Mrid.Equals(discrete.Mrid))
                                        {
                                            if (discrete.MaxValue != al.Discrete.MaxValue && discrete.MaxValue != 0)
                                            {
                                                al.Discrete.MaxValue = discrete.MaxValue;
                                            }
                                            else if (discrete.MeasurmentType != al.Discrete.MeasurmentType && discrete.MeasurmentType.ToString() != "")
                                            {
                                                al.Discrete.MeasurmentType = discrete.MeasurmentType;
                                            }
                                            else if (discrete.MinValue != al.Discrete.MinValue && discrete.MinValue != 0)
                                            {
                                                al.Discrete.MinValue = discrete.MinValue;
                                            }
                                            else if (discrete.Name != al.Discrete.Name && discrete.Name.ToString() != "")
                                            {
                                                al.Discrete.Name = discrete.Name;
                                            }
                                            else if (discrete.AliasName != al.Discrete.AliasName && discrete.AliasName.ToString() != "")
                                            {
                                                al.Discrete.AliasName = discrete.AliasName;
                                            }
                                            else if (discrete.NormalValue != al.Discrete.NormalValue && discrete.NormalValue != 0)
                                            {
                                                al.Discrete.NormalValue = discrete.NormalValue;
                                            }
                                            else if (discrete.PowerSystemResource != al.Discrete.PowerSystemResource && discrete.PowerSystemResource.ToString() != "")
                                            {
                                                al.Discrete.PowerSystemResource = discrete.PowerSystemResource;
                                            }
                                            else if (discrete.Direction != al.Discrete.Direction && discrete.Direction.ToString() != "")
                                            {
                                                al.Discrete.Direction = discrete.Direction;
                                            }
                                            else if (discrete.ScadaAddress != al.Discrete.ScadaAddress && discrete.ScadaAddress.ToString() != "")
                                            {
                                                al.Discrete.ScadaAddress = discrete.ScadaAddress;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                updateResult.Message = "SCADA PR Transaction Prepare finished.";
                updateResult.Result = ResultType.Succeeded;
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "SCADA PR Transaction Prepare finished successfully.");
                transactionCallback.Response("OK");
                ServiceEventSource.Current.Message("SCADA PROCCESSING Transaction Prepare finished successfully.");
            }
            catch (Exception e)
            {
                updateResult.Message = "SCADA PR Transaction Prepare finished.";
                updateResult.Result = ResultType.Failed;
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "SCADA PR Transaction Prepare failed. Message: {0}", e.Message);
                transactionCallback.Response("ERROR");
                ServiceEventSource.Current.Message("SCADA PROCCESSING Transaction Prepare failed. Message: {0}", e.Message);

            }

            return updateResult;
        }

        public bool Commit()
        {
            try
            {
                generatorAnalogs.Clear();
                energyConsumerAnalogs.Clear();
                generatorDscretes.Clear();
                energyConsumerDiscretes.Clear();

                foreach (AnalogLocation alocation in generatorAnalogsCopy)
                {
                    generatorAnalogs.Add(alocation.Clone() as AnalogLocation);
                }

                foreach (AnalogLocation alocation in energyConsumerAnalogsCopy)
                {
                    energyConsumerAnalogs.Add(alocation.Clone() as AnalogLocation);
                }
                foreach (DiscreteLocation dlocation in generatorDscretesCopy)
                {
                    generatorDscretes.Add(dlocation.Clone() as DiscreteLocation);
                }

                foreach (DiscreteLocation dlocation in energyConsumerDiscretesCopy)
                {
                    energyConsumerDiscretes.Add(dlocation.Clone() as DiscreteLocation);
                }

                generatorAnalogsCopy.Clear();
                energyConsumerAnalogsCopy.Clear();
                generatorDscretesCopy.Clear();
                energyConsumerDiscretesCopy.Clear();
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "SCADA PR Transaction: Commit phase successfully finished.");

                ServiceEventSource.Current.Message("SCADA PROCCESSING Transaction: Commit phase successfully finished.");
                return true;
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "SCADA PR Transaction: Failed to Commit changes. Message: {0}", e.Message);
                ServiceEventSource.Current.Message("SCADA PR Transaction: Failed to Commit changes. Message: {0}", e.Message);

                return false;
            }
        }

        public bool Rollback()
        {
            try
            {
                generatorAnalogsCopy.Clear();
                generatorDscretesCopy.Clear();
                energyConsumerAnalogsCopy.Clear();
                energyConsumerDiscretesCopy.Clear();
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Transaction rollback successfully finished!");
                ServiceEventSource.Current.Message("Transaction rollback successfully finished!");

                return true;
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "Transaction rollback error. Message: {0}", e.Message);
                ServiceEventSource.Current.Message("Transaction rollback error. Message: {0}", e.Message);

                return false;
            }
        }

        private List<MeasurementUnit> ParseDataToMeasurementUnit(List<AnalogLocation> analogList, byte[] value, int startAddress, ModelCode type)
        {
            List<MeasurementUnit> retList = new List<MeasurementUnit>();
            lock (analogList)
            {
                foreach (AnalogLocation analogLoc in analogList)
                {
                    float[] values = ModbusHelper.GetValueFromByteArray<float>(value, analogLoc.LengthInBytes, (analogLoc.StartAddress - 1) * 4);

                    if (values == null)
                    {
                        MeasurementUnit measUnit1 = new MeasurementUnit();
                        measUnit1.Gid = analogLoc.Analog.PowerSystemResource;
                        measUnit1.MinValue = analogLoc.Analog.MinValue;
                        measUnit1.MaxValue = analogLoc.Analog.MaxValue;
                        measUnit1.CurrentValue = 0;
                        measUnit1.TimeStamp = DateTime.Now;
                        measUnit1.ScadaAddress = analogLoc.StartAddress;
                        retList.Add(measUnit1);
                    }
                    else
                    {
                        float eguVal = convertorHelper.ConvertFromRawToEGUValue(values[0], analogLoc.Analog.MinValue, analogLoc.Analog.MaxValue);
                        float MAX = convertorHelper.ConvertFromRawToEGUValue(analogLoc.Analog.MaxValue, 1, 1);
                        float MIN = convertorHelper.ConvertFromRawToEGUValue(analogLoc.Analog.MinValue, 1, 1);
                        bool alarmEGU = false;

                        if (type.Equals(ModelCode.GENERATOR))
                        {
                            alarmEGU = this.CheckForEGUAlarms(eguVal, MIN, MAX, analogLoc.Analog.PowerSystemResource, analogLoc.Analog.Name);

                            if (!alarmEGU)
                            {
                                AlarmHelper al = new AlarmHelper();
                                al.Gid = analogLoc.Analog.PowerSystemResource;
                                al.Value = eguVal;
                                alarmsEventsSfProxy.UpdateStatus(analogLoc, State.Cleared);

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
                }
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
                measUnit.MaxValue = discreteLoc.Discrete.MaxValue; ;
                measUnit.CurrentValue = value.Length < discreteLoc.StartAddress ? 1 : (value[discreteLoc.StartAddress - 1] ? 1 : 0);
                measUnit.TimeStamp = DateTime.Now;
                measUnit.ScadaAddress = discreteLoc.StartAddress;
                retList.Add(measUnit);

                //previousGeneratorDiscretes[discreteLoc.Discrete.GlobalId] = measUnit.CurrentValue;
            }
            return retList;
        }

        private bool CheckDiscretAlarm(int value, float max, long gid, string name)
        {
            bool retVal = false;
            AlarmHelper ah = new AlarmHelper(gid, value, 0, max, DateTime.Now);
            if (value > max)
            {
                ah.Value = value;
                ah.MaxValue = max;
                ah.Type = AlarmType.DOM;
                ah.Severity = SeverityLevel.HIGH;
                ah.Message = string.Format("Value on input discret signal: {0} higher than maximum expected value", name);
                ah.Name = name;
                alarmsEventsSfProxy.AddAlarm(ah);
                retVal = true;
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Alarm on high raw limit on name: {0}", name);
                Console.WriteLine("Alarm on high raw limit on name: {0}", name);
                //ServiceEventSource.Current.Message("Alarm on high raw limit on name: {0}", name);
            }
            return retVal;
        }
        private bool CheckForEGUAlarms(float value, float minEgu, float maxEgu, long gid, string name)
        {
            bool retVal = false;
            AlarmHelper ah = new AlarmHelper(gid, value, minEgu, maxEgu, DateTime.Now);
            if (value < minEgu)
            {
                ah.Type = AlarmType.LOW;
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
                ah.TimeStamp = DateTime.Now;
                ah.Message = string.Format("Value on input analog signal: {0} lower than minimum expected value", name);
                ah.Name = name;
                alarmsEventsSfProxy.AddAlarm(ah);
                retVal = true;
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Alarm on low raw limit on gid: {0:X}", gid);
                Console.WriteLine("Alarm on low raw limit on gid: {0}", name);
                //ServiceEventSource.Current.Message("Alarm on low raw limit on gid: {0}", name);

            }

            if (value > maxEgu)
            {
                ah.Type = AlarmType.HIGH;
                if (value <= (maxEgu + (maxEgu * 10 / 100)))
                {
                    ah.Severity = SeverityLevel.MAJOR;
                }
                else if (value <= (maxEgu + (maxEgu * 20 / 100)))
                {
                    ah.Severity = SeverityLevel.HIGH;
                }
                else
                {
                    ah.Severity = SeverityLevel.CRITICAL;
                }
                ah.TimeStamp = DateTime.Now;
                ah.Message = string.Format("Value on input analog signal: {0} higher than maximum expected value", name);
                ah.Name = name;

                alarmsEventsSfProxy.AddAlarm(ah);
                retVal = true;
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Alarm on high raw limit on gid: {0}", name);
                Console.WriteLine("Alarm on high raw limit on gid: {0}", name);
                //ServiceEventSource.Current.Message("Alarm on high raw limit on gid: {0}", name);

            }

            return retVal;
        }

        private List<MeasurementUnit> SelectActive(List<MeasurementUnit> analogs, List<MeasurementUnit> descretes)
        {
            List<MeasurementUnit> returnList = new List<MeasurementUnit>();

            foreach (var item in analogs)
            {
                var des = descretes.Find(x => x.Gid == item.Gid);
                if (des.CurrentValue == 1)
                    returnList.Add(item);
            }
            return returnList;
        }

        private void CheckWhichAreTurnedOff(List<MeasurementUnit> analogs, List<MeasurementUnit> descretes)
        {
            CalculationEngineSfProxy calculationEngineSfProxy = new CalculationEngineSfProxy();
            List<DiscreteCounterModel> dcFromDB = calculationEngineSfProxy.GetAllDiscreteCounters();
            foreach (var item in analogs)
            {
                var des = descretes.Find(x => x.Gid == item.Gid);
                var itemFromDb = dcFromDB.Find(x => x.Gid == item.Gid);

                if (des.CurrentValue == 1)
                {
                    if (itemFromDb != null)
                    {
                        if (itemFromDb.CurrentValue == false)
                        {
                            itemFromDb.CurrentValue = true;
                            calculationEngineSfProxy.InsertOrUpdate(itemFromDb);
                        }
                    }
                }
                else if (des.CurrentValue == 0)
                {
                    var obj = generatorDscretes.FirstOrDefault(x => x.Discrete.PowerSystemResource == item.Gid);
                    string name = obj.Discrete.Name;
                    if (itemFromDb == null)
                    {
                        calculationEngineSfProxy.InsertOrUpdate(new DiscreteCounterModel() { Gid = item.Gid, Counter = 1, CurrentValue = false, Name = name });
                    }
                    else
                    {
                        if (itemFromDb.CurrentValue == true)
                        {
                            itemFromDb.CurrentValue = false;
                            itemFromDb.Counter++;
                            DiscretMaxVal = calculationEngineSfProxy.GetCounterForGeneratorType();
                            int maxVal = DiscretMaxVal.FirstOrDefault(x => x.Key.Item1 == itemFromDb.Gid).Value;
                            CheckDiscretAlarm(itemFromDb.Counter, maxVal, itemFromDb.Gid, name);
                            calculationEngineSfProxy.InsertOrUpdate(itemFromDb);
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

        public bool ContainsMrid(Analog analog, List<AnalogLocation> list)
        {
            foreach (AnalogLocation al in list)
            {
                if (al.Analog.Mrid.Equals(analog.Mrid))
                {
                    return true;
                }
            }

            return false;
        }
        public bool ContainsMridDiscrete(Discrete analog, List<DiscreteLocation> list)
        {
            foreach (DiscreteLocation al in list)
            {
                if (al.Discrete.Mrid.Equals(analog.Mrid))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
