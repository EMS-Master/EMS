using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CalculationEngineServ;
//using CalculationEngineServ;
using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.ServiceContracts.ServiceFabricProxy;
using FTN.Services.NetworkModelService.DataModel.Meas;
using ModbusClient;
using ScadaContracts;
using TransactionContract;

namespace ScadaCommandingService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ScadaCommand : IScadaCommandingContract, ITransactionContract
    {
        private MdbClient modbusClient;
        private static List<AnalogLocation> listOfAnalog;
        private static List<AnalogLocation> listOfAnalogCopy;
        private static List<DiscreteLocation> listOfDiscretes;
        private static List<DiscreteLocation> listOfDiscretesCopy;
        private UpdateResult updateResult;
        private ConvertorHelper convertorHelper;
		private readonly object lockObj = new object();

		private ModelResourcesDesc modelResourcesDesc;

        private string message = string.Empty;
        private readonly int START_ADDRESS_GENERATOR = 20;
        private ITransactionCallback transactionCallback;

        public ScadaCommand()
        {
            convertorHelper = new ConvertorHelper();
            ConnectToSimulator();

            listOfAnalog = new List<AnalogLocation>();
            listOfAnalogCopy = new List<AnalogLocation>();
            listOfDiscretes = new List<DiscreteLocation>();
            listOfDiscretesCopy = new List<DiscreteLocation>();
            modelResourcesDesc = new ModelResourcesDesc();
        }

        private void ConnectToSimulator()
        {
            try
            {
                modbusClient = new MdbClient("localhost", 502);
                modbusClient.Connect();
            }
            catch (SocketException)
            {
                //Start simulator EasyModbusServerSimulator.exe
                string appPath = Path.GetFullPath("..\\..\\..\\..\\..\\");
                Process.Start(appPath + "EasyModbusServerSimulator.exe");

                Thread.Sleep(2000);
                ConnectToSimulator();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Transaction
        public bool Commit()
        {
            try
            {
                listOfAnalog.Clear();
                listOfDiscretes.Clear();
                foreach (AnalogLocation alocation in listOfAnalogCopy)
                {
                    listOfAnalog.Add(alocation.Clone() as AnalogLocation);
                }

                foreach (DiscreteLocation dlocation in listOfDiscretesCopy)
                {
                    listOfDiscretes.Add(dlocation.Clone() as DiscreteLocation);
                }
                listOfAnalogCopy.Clear();
                listOfDiscretesCopy.Clear();
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "SCADA CMD Transaction: Commit phase successfully finished.");
                Console.WriteLine("Number of Analog values: {0}", listOfAnalog.Count);

                

                return true;
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "SCADA CMD Transaction: Failed to Commit changes. Message: {0}", e.Message);
                return false;
            }
        }

        public UpdateResult Prepare(ref Delta delta)
        {
            try
            {
                transactionCallback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
                updateResult = new UpdateResult();

                listOfAnalogCopy = new List<AnalogLocation>();
                listOfDiscretesCopy = new List<DiscreteLocation>();

                // napravi kopiju od originala
                foreach (AnalogLocation alocation in listOfAnalog)
                {
                    listOfAnalogCopy.Add(alocation.Clone() as AnalogLocation);
                   
                }
                foreach (DiscreteLocation dlocation in listOfDiscretes)
                {
                    listOfDiscretesCopy.Add(dlocation.Clone() as DiscreteLocation);

                }
                Analog analog = null;
                Discrete discrete = null;

                foreach (ResourceDescription analogRd in delta.InsertOperations)
                {

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analogRd.Id) == (DMSType.ANALOG))
                    {
                        foreach (Property prop in analogRd.Properties)
                        {
                            analog = ResourcesDescriptionConverter.ConvertTo<Analog>(analogRd);
                            listOfAnalogCopy.Add(new AnalogLocation()
                            {
                                Analog = analog,
                                StartAddress = Int32.Parse(analog.ScadaAddress.Split('_')[1]),
                                Length = 2,
                                LengthInBytes = 4
                            });

                        }
                    }
                    else if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analogRd.Id) == (DMSType.DISCRETE))
                    {
                        foreach (Property prop in analogRd.Properties)
                        {
                            discrete = ResourcesDescriptionConverter.ConvertTo<Discrete>(analogRd);
                            listOfDiscretes.Add(new DiscreteLocation()
                            {
                                Discrete = discrete,
                                StartAddress = Int32.Parse(discrete.ScadaAddress.Split('_')[1]),
                                Length = 1,
                                LengthInBytes = 2
                            });
                        }
                    }
                    
                    
                }

                foreach (ResourceDescription analogRd in delta.UpdateOperations)
                {

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analogRd.Id) == (DMSType.ANALOG))
                    {
                        foreach (Property prop in analogRd.Properties)
                        {
                            analog = ResourcesDescriptionConverter.ConvertTo<Analog>(analogRd);

                            foreach (AnalogLocation al in listOfAnalogCopy)
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
                    else if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analogRd.Id) == (DMSType.DISCRETE))
                    {
                        foreach (Property prop in analogRd.Properties)
                        {
                            discrete = ResourcesDescriptionConverter.ConvertTo<Discrete>(analogRd);
                            foreach (DiscreteLocation al in listOfDiscretesCopy)
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

                updateResult.Message = "SCADA CMD Transaction Prepare finished.";
                updateResult.Result = ResultType.Succeeded;
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "SCADA CMD Transaction Prepare finished successfully.");
                transactionCallback.Response("OK");
            }
            catch (Exception e)
            {
                updateResult.Message = "SCADA CMD Transaction Prepare finished.";
                updateResult.Result = ResultType.Failed;
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "SCADA CMD Transaction Prepare failed. Message: {0}", e.Message);
                transactionCallback.Response("ERROR");
            }

            return updateResult;
        }

        public bool Rollback()
        {
            try
            {
                listOfAnalogCopy.Clear();
                listOfDiscretesCopy.Clear();
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Transaction rollback successfully finished!");
                return true;
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "Transaction rollback error. Message: {0}", e.Message);
                return false;
            }
        }
        #endregion

      
        /// <summary>
        /// Method implements integrity update logic for scada cr component
        /// </summary>
        /// <returns></returns>
        public bool InitiateIntegrityUpdate()
        {
            List<ModelCode> properties = new List<ModelCode>(10);
            List<ModelCode> propertiesDiscrete = new List<ModelCode>(10);
            ModelCode modelCode = ModelCode.ANALOG;
            ModelCode modelCodeDiscrete = ModelCode.DISCRETE;
            int iteratorId = 0;
            int resourcesLeft = 0;
            int numberOfResources = 2;

            List<ResourceDescription> retList = new List<ResourceDescription>(5);
            List<ResourceDescription> retListDiscrete = new List<ResourceDescription>(5);

            NetworkModelGDASfProxy networkModelGDASfProxy = new NetworkModelGDASfProxy();
            try
            {
                properties = modelResourcesDesc.GetAllPropertyIds(modelCode);
                propertiesDiscrete = modelResourcesDesc.GetAllPropertyIds(modelCodeDiscrete);

                iteratorId = networkModelGDASfProxy.GetExtentValues(modelCode, properties);
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
                message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCode, e.Message);
                Console.WriteLine(message);

                Console.WriteLine("Trying again...");
                CommonTrace.WriteTrace(CommonTrace.TraceError, "Trying again...");
                //NetworkModelGDAProxy.Instance = null;
                Thread.Sleep(1000);
                InitiateIntegrityUpdate();
                return false;
            }

            listOfAnalog.Clear();

            try
            {
                foreach (ResourceDescription rd in retList)
                {
                    Analog analog = ResourcesDescriptionConverter.ConvertTo<Analog>(rd);

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analog.PowerSystemResource) == DMSType.ENERGY_CONSUMER)
                    {
                        listOfAnalog.Add(new AnalogLocation()
                        {
                            Analog = analog,
                            StartAddress = Int32.Parse(analog.ScadaAddress.Split('_')[1]),
                            Length = 2,
                            LengthInBytes = 4
                        });
                    }
                    else
                    {
                        listOfAnalog.Add(new AnalogLocation()
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
                        listOfDiscretes.Add(new DiscreteLocation()
                        {
                            Discrete = discrete,
                            StartAddress = Int32.Parse(discrete.ScadaAddress.Split('_')[1]),
                            Length = 1,
                            LengthInBytes = 2
                        });
                    }
                    else
                    {
                        listOfDiscretes.Add(new DiscreteLocation()
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
                message = string.Format("Conversion to Analog object failed.\n\t{0}", e.Message);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                return false;
            }

            message = string.Format("Integrity update: Number of {0} values: {1}", modelCode.ToString(), listOfAnalog.Count.ToString());
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            Console.WriteLine("Integrity update: Number of {0} values: {1}", modelCode.ToString(), listOfAnalog.Count.ToString());
            return true;
        }

        public bool SendDataToSimulator(List<MeasurementUnit> measurements)
        {
			lock (lockObj)
			{
				foreach (var item in listOfAnalog)
				{
					var mes = measurements.Find(x => x.Gid == item.Analog.PowerSystemResource);
					if (mes != null)
					{
						float rawValue = convertorHelper.ConvertFromEGUToRawValue(mes.CurrentValue, 1, 0);
						modbusClient.WriteSingleRegister((ushort)((mes.ScadaAddress - 1) * 2), rawValue);

					}
					else
					{
						if (CheckIfGenerator(item.StartAddress))
						{
							modbusClient.WriteSingleRegister((ushort)((item.StartAddress - 1) * 2), (float)0);
						}
					}
				}
			}

            //modbusClient.WriteSingleRegister((ushort)12, (float)94.8);

            Console.WriteLine("SendDataToSimulator executed...\n");

            return true;
        }

        public bool CommandDiscreteValues(long gid, bool value)
        {
            var discLoc = listOfDiscretes.Find(p => p.Discrete.PowerSystemResource == gid);
			var anLoc = listOfAnalog.Find(p => p.Analog.PowerSystemResource == gid);

			if (discLoc != null)
            {
                modbusClient.WriteSingleCoil((ushort)(discLoc.StartAddress - 1), value);
				modbusClient.WriteSingleRegister((ushort)((anLoc.StartAddress - 1) * 2), 0f);
			}

            Console.WriteLine("SendDataToSimulator executed...\n");

            return true;
        }

        public bool CommandAnalogValues(long gid, float value)
        {
            var anLoc = listOfAnalog.Find(p => p.Analog.PowerSystemResource == gid);

            if (anLoc != null)
            {
				value = value * 1000f;
                float rawValue = convertorHelper.ConvertFromEGUToRawValue(value, 1, 0);
                modbusClient.WriteSingleRegister((ushort)((anLoc.StartAddress - 1) * 2), rawValue);
				var commandedGeneratorFromDB = DbManager.Instance.GetCommandedGenerator(gid);
				commandedGeneratorFromDB.CommandingFlag = true;
				commandedGeneratorFromDB.CommandingValue = value;

				DbManager.Instance.AddCommandedGenerator(commandedGeneratorFromDB);
				//DbManager.Instance.SaveChanges();
			}

            Console.WriteLine("SendDataToSimulator executed...\n");

            return true;
        }

        private bool CheckIfGenerator(int number)
        {
            return number > 33 ? true : false;
        }

        //public void FillSimulatorFirstTime()
        //{
        //    var random = new Random();
        //    for(int i = 0; i < 20; i++)
        //    {
        //        float value = 0;
        //        if(i < 10)
        //        {
        //            var x = random.Next(10, 70);
        //            var y = (float)random.NextDouble();
        //            value = x + y;
        //        }
        //        else
        //        {
        //            var x = random.Next(70, 150);
        //            var y = (float)random.NextDouble();
        //            value = x + y;
        //        }
        //        modbusClient.WriteSingleRegister((ushort)(i * 2), value);
        //        modbusClient.WriteSingleCoil((ushort)i, true);
        //    }
        //    var a = random.Next(1, 8);
        //    var b = (float)random.NextDouble();
        //    float val = a + b; // m/s
        //    modbusClient.WriteSingleRegister(49, val);

        //    a = random.Next(1, 7);
        //    b = (float)random.NextDouble();
        //    val = a + b; // kWh/m2/per day
        //    modbusClient.WriteSingleRegister(51, val);

        //}
    }
}
