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
using CalculationEngineServ.DataBaseModels;
using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
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
        private static List<DiscreteLocation> listOfDiscretes;
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
            listOfDiscretes = new List<DiscreteLocation>();
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
            throw new NotImplementedException();
        }

        public UpdateResult Prepare(ref Delta delta)
        {
            throw new NotImplementedException();
        }

        public bool Rollback()
        {
            throw new NotImplementedException();
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
            try
            {
                properties = modelResourcesDesc.GetAllPropertyIds(modelCode);
                propertiesDiscrete = modelResourcesDesc.GetAllPropertyIds(modelCodeDiscrete);

                iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCode, properties);
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
                message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCode, e.Message);
                Console.WriteLine(message);

                Console.WriteLine("Trying again...");
                CommonTrace.WriteTrace(CommonTrace.TraceError, "Trying again...");
                NetworkModelGDAProxy.Instance = null;
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
           
            if (discLoc != null)
            {
                modbusClient.WriteSingleCoil((ushort)(discLoc.StartAddress - 1), value);
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

				DbManager.Instance.UpdateCommandedGenerator(commandedGeneratorFromDB);
				DbManager.Instance.SaveChanges();
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
