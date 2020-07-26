﻿using FTN.Common;
using ModbusClient;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScadaService
{
    public class Scada : IScadaContract
    {

        private MdbClient mdbClient;
        private ushort numberOfHoldingRegisters = 42;    //in bytes
		private ushort numberOfCoils = 20;
        private ushort numberOfHRegistersWS = 6;
		public Scada()
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
            catch(SocketException e)
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
                    GetDataFromSimulator();
                    Thread.Sleep(3000);
                }
            });
            task.Start();
        }

        public bool GetDataFromSimulator()
        {
            //mdbClient.WriteSingleRegister(5, 567);
            //mdbClient.WriteSingleCoil(10, true);

            //byte[] val = mdbClient.ReadHoldingRegisters(0, 10);
            //bool[] val1 = mdbClient.ReadCoils(0, 13);
            
            //ushort[] retVal = ModbusHelper.GetUShortValuesFromByteArray(val, val.Length, 0);
            //for (int i = 0; i < retVal.Length; i++)
            //{

            //    Console.WriteLine("Value of HoldingRegister " + (i + 1) + ": " + Convert.ToString((int)retVal[i]));
                
            //}

            //for(int i = 0; i < val1.Length; i++)
            //{
            //    Console.WriteLine("Value of DiscreteInput " + (i + 1) + ": " + val1[i]);
            //}

            //Console.WriteLine();

            var values = mdbClient.ReadHoldingRegisters(0, numberOfHoldingRegisters);
			var valuesDiscrete = mdbClient.ReadCoils(0, numberOfCoils);
            var valuesWindSun = mdbClient.ReadHoldingRegisters(49, numberOfHRegistersWS);
            bool isSuccess = false;
            try
            {
                isSuccess = ScadaProcessingProxy.Instance.SendValues(values, valuesDiscrete, valuesWindSun);
				
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                Console.WriteLine("[Method = GetDataFromSimulator] Error: " + ex.Message);
            }

            return isSuccess;


            //return true;
        }
    }
}
