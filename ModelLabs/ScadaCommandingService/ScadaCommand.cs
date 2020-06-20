﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
    public class ScadaCommand : IScadaCommandingContract, ITransactionContract
    {
        private MdbClient modbusClient;
        private static List<AnalogLocation> listOfAnalog;

        private UpdateResult updateResult;
        private ConvertorHelper convertorHelper;

        private ModelResourcesDesc modelResourcesDesc;

        private string message = string.Empty;
        private readonly int START_ADDRESS_GENERATOR = 50;
        private ITransactionCallback transactionCallback;

        public ScadaCommand()
        {
            convertorHelper = new ConvertorHelper();
            ConnectToSimulator();

            listOfAnalog = new List<AnalogLocation>();
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
            ModelCode modelCode = ModelCode.ANALOG;
            int iteratorId = 0;
            int resourcesLeft = 0;
            int numberOfResources = 2;

            List<ResourceDescription> retList = new List<ResourceDescription>(5);
            try
            {
                properties = modelResourcesDesc.GetAllPropertyIds(modelCode);

                iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCode, properties);
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
                            StartAddress = iBateryStorage++ * 2,
                            Length = 2,
                            LengthInBytes = 4
                        });
                    }
                    else
                    {
                        listOfAnalog.Add(new AnalogLocation()
                        {
                            Analog = analog,
                            StartAddress = START_ADDRESS_GENERATOR + iGen++ * 2,
                            Length = 2,
                            LengthInBytes = 4
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
            //foreach(var item in measurements)
            //{
                float rawValue = convertorHelper.ConvertFromEGUToRawValue(measurements[0].CurrentValue, 1, 0);
                modbusClient.WriteSingleRegister((ushort)2, rawValue);
            //}

            Console.WriteLine("SendDataToSimulator executed...\n");
           // modbusClient.WriteSingleRegister(4, 15);

            return true;
        }
        
    }
}
