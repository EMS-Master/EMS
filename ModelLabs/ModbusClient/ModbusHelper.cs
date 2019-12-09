using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusClient
{
    public static class ModbusHelper
    {
        public static ushort[] GetUShortValuesFromByteArray(byte[] byteArray, int arrayLength, int startIndex = 0)
        {
            int numberOfValues = arrayLength / 2;
            ushort[] retArray = new ushort[numberOfValues];
            for (int i = 0; i < numberOfValues; i++)
            {
                byte[] valueInBytes = new byte[2];
                Array.Copy(byteArray, startIndex + i * 2, valueInBytes, 0, 2);
                byte temp = valueInBytes[0];
                valueInBytes[0] = valueInBytes[1];
                valueInBytes[1] = temp;
                retArray[i] = BitConverter.ToUInt16(valueInBytes, 0);
            }

            return retArray;

        }
      
    }
}
