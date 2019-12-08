using EasyModbus;
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
        private ModbusClient mdbClient;
        private TcpClient client;
        private byte[] receivedData = new byte[500];
        private byte[] header = new byte[7] { 9, 0, 0, 0, 0, 5, 0 };
        private byte[] sendData = new byte[5];
        public Scada()
        {
            ConnectToSimulator();
        }

        private void ConnectToSimulator()
        {
            try
            {
                mdbClient = new ModbusClient("localhost", 502);
              //  client = new TcpClient("localhost", 502);

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
            //byte[] data1 = PreparePackageForRead(0, 104, 0x03);
            //int numOfBytes = client.Client.Send(data1);
            //Console.WriteLine("Send {0} bytes", numOfBytes);

            //numOfBytes = client.Client.Receive(receivedData);

            //byte[] data = StripHeader(receivedData);

            //short[] val = GetValueFromByteArray<short>(data, data.Length, 0);

           int[] val = mdbClient.ReadHoldingRegisters(0, 10);
            for (int i = 0; i < val.Length; i++)
                Console.WriteLine("Value of HoldingRegister " + (i + 1) + " " + val[i].ToString());
           // modbusClient.Disconnect();

            //Console.WriteLine("val[0]:" + val[0]);
            //Console.WriteLine("val[1]: " + val[1]);
            Console.WriteLine();
            return true;
        }

        private static Dictionary<Type, int> typeToByteCountDictionary = new Dictionary<Type, int>()
        {
            {typeof(int),    sizeof(int)},
            {typeof(long),   sizeof(long)},
            {typeof(float),  sizeof(float)},
            {typeof(double), sizeof(double)},
            {typeof(ushort), sizeof(ushort)},
            {typeof(short),  sizeof(short)},
            {typeof(byte),   sizeof(byte) }
        };

        //Dictionary that converts type into ConversionFunction
        //ConversionFunction return converted byte[] to T 
        private static Dictionary<Type, Func<byte[], object>> typeToConversionFunction = new Dictionary<Type, Func<byte[], object>>()
        {
            {typeof(int),    (byte[] byteArray)=> { return BitConverter.ToInt32(byteArray,0); } },
            {typeof(long),   (byte[] byteArray)=> { return BitConverter.ToInt64(byteArray,0); } },
            {typeof(float),  (byte[] byteArray)=> { return BitConverter.ToSingle(byteArray,0); } },
            {typeof(double), (byte[] byteArray)=> { return BitConverter.ToDouble(byteArray,0); } },
            {typeof(ushort), (byte[] byteArray)=> { return BitConverter.ToUInt16(byteArray,0); } },
            {typeof(short),  (byte[] byteArray)=> { return BitConverter.ToInt16(byteArray,0); } },
            {typeof(byte),   (byte[] byteArray)=> { return byteArray[0]; } }
        };

        public static T[] GetValueFromByteArray<T>(byte[] byteArray, int arrayLength, int startIndex = 0)
        {
            int sizeofType = typeToByteCountDictionary[typeof(T)];

            int numberOfValues = arrayLength / sizeofType;
            T[] genericArray = new T[numberOfValues];
            for (int i = 0; i < numberOfValues; i++)
            {
                byte[] valueInBytes = new byte[sizeofType];
                Array.Copy(byteArray, startIndex + i * sizeofType, valueInBytes, 0, sizeofType);

                genericArray[i] = ((T)typeToConversionFunction[typeof(T)].Invoke(valueInBytes));
            }

            return genericArray;
        }

        private byte[] PreparePackageForRead(ushort startingAddress, ushort quantity, byte functionCode)
        {
            byte[] startAddressBytes = BitConverter.GetBytes(startingAddress);
            byte[] quanityBytes = BitConverter.GetBytes(quantity);

            sendData[0] = functionCode;
            sendData[1] = startAddressBytes[1];
            sendData[2] = startAddressBytes[0];
            sendData[3] = quanityBytes[1];
            sendData[4] = quanityBytes[0];

            byte[] data = new byte[header.Length + sendData.Length + 1];

            header.CopyTo(data, 0);
            sendData.CopyTo(data, header.Length);
            return data;
        }

        private byte[] StripHeader(byte[] data)
        {
            byte[] returnData = new byte[data.Length - header.Length];

            Array.Copy(data, header.Length, returnData, 0, returnData.Length);
            return returnData;
        }
    }
}
