using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusClient
{
    public static class ModbusHelper
    {

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

    }
}
