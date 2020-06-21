using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMeas
{
    public class ConvertorHelper
    {
        public ConvertorHelper()
        {
        }

        public float ConvertFromRawToEGUValue(float value, float minEGU, float maxEGU)
        {
            float scalingFactor = 1;
            float deviation = 0;

            float retVal = value * scalingFactor + deviation;
            return retVal;
        }

        public float ConvertFromEGUToRawValue(float value, float minEGU, float maxEGU)
        {
            float scalingFactor = 1;
            float deviation = 0;

            float retVal = (value - deviation) / scalingFactor;
            return retVal;
        }
    }

}
