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
                int iBateryStorage = 0;
                int iGen = 0;
                foreach (ResourceDescription rd in retList)
                {
                    Analog analog = ResourcesDescriptionConverter.ConvertTo<Analog>(rd);

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(analog.PowerSystemResource) == DMSType.BATTERY_STORAGE)
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

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(discrete.PowerSystemResource) == DMSType.BATTERY_STORAGE)
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

            foreach (var item in listOfAnalog)
            {
                var mes = measurements.Find(x => x.Gid == item.Analog.PowerSystemResource);
                if (mes != null)
                {
                    float rawValue = convertorHelper.ConvertFromEGUToRawValue(mes.CurrentValue, 1, 0);
                    modbusClient.WriteSingleRegister((ushort)((mes.ScadaAddress-1) * 2), rawValue);

                }
                else
                {
                    if (CheckIfGenerator(item.StartAddress))
                    {
                        modbusClient.WriteSingleRegister((ushort)((item.StartAddress-1) * 2), (float)0);
                    }
                }
            }

            //modbusClient.WriteSingleRegister((ushort)12, (float)94.8);

            Console.WriteLine("SendDataToSimulator executed...\n");

            return true;
        }

        public bool CommandDiscreteValues(List<MeasurementUnitDiscrete> measurements)
        {
            foreach (var item in listOfDiscretes)
            {
                var mes = measurements.Find(x => x.Gid == item.Discrete.PowerSystemResource);
                if (mes != null)
                {
                    // float rawValue = convertorHelper.ConvertFromEGUToRawValue(mes.CurrentValue, 1, 0);
                    modbusClient.WriteSingleCoil((ushort)((mes.ScadaAddress - 1)), mes.CurrentValue);

                }
                else
                {
                   // if (CheckIfGenerator(item.StartAddress))
                   // {
                   //     modbusClient.WriteSingleRegister((ushort)((item.StartAddress - 1) * 2), (float)0);
                   // }
                }
            }

            //modbusClient.WriteSingleRegister((ushort)12, (float)94.8);

            Console.WriteLine("SendDataToSimulator executed...\n");

            return true;
        }
        private bool CheckIfGenerator(int number)
        {
            return number > 10 ? true : false;
        }


    }
}
