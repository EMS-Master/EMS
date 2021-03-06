using FTN.Common;
using ModbusClient;
using ScadaContracts;
using ScadaContracts.ServiceFabricProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScadaCloudServ
{
    public class ScadaCloud : IScadaContract
    {

        private MdbClient mdbClient;
        private ushort numberOfHoldingRegisters = 82;    //in bytes
        private ushort numberOfCoils = 40;
        private ushort numberOfHRegistersWS = 6;
        private readonly object lockObj = new object();
        public ScadaCloud()
        {
            ConnectToSimulator();
        }

        private void ConnectToSimulator()
        {
            try
            {
                mdbClient = new MdbClient("localhost", 502);
                if (mdbClient.Connected)
                {
                    return;
                }
                mdbClient.Connect("localhost", 502);
            }
            catch (SocketException e)
            {
                Thread.Sleep(2000);
                ConnectToSimulator();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void StartCollectingData()
        {
            Thread.Sleep(5000);
            Task task = new Task(() =>
            {
                while (true)
                {
                    if (mdbClient == null || mdbClient.Connected == false)
                        ConnectToSimulator();

                    GetDataFromSimulator();
                    Thread.Sleep(3000);
                }
            });
            task.Start();
        }

        public bool GetDataFromSimulator()
        {
            byte[] values;
            bool[] valuesDiscrete;
            byte[] valuesWindSun;

            lock (lockObj)
            {
                values = mdbClient.ReadHoldingRegisters(0, numberOfHoldingRegisters);
                valuesDiscrete = mdbClient.ReadCoils(0, numberOfCoils);
                valuesWindSun = mdbClient.ReadHoldingRegisters(100, numberOfHRegistersWS);
            }

            bool isSuccess = false;
            try
            {
                ScadaProcessingSfProxy scadaSf = new ScadaProcessingSfProxy();
                isSuccess = scadaSf.SendValues(values, valuesDiscrete, valuesWindSun);
                //string message = string.Format("Method SendValues() successfully executed...);
                //ServiceEventSource.Current.Message(message);

            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                Console.WriteLine("[Method = GetDataFromSimulator] Error: " + ex.Message);
                //ServiceEventSource.Current.Message("[Method = GetDataFromSimulator] Error: " + ex.Message);

            }

            return isSuccess;
        }
    }
}
