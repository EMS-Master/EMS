using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMeas
{
    public class ConvertorHelper
    {
        /// <summary>
        /// minimal raw value for simulator
        /// </summary>
        private float minRaw;

        /// <summary>
        /// maximal raw value for simulator
        /// </summary>
        private float maxRaw;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertorHelper" /> class
        /// </summary>
        public ConvertorHelper()
        {
            this.minRaw = 0;
            this.maxRaw = 4095;
        }

        /// <summary>
        /// Gets MinRaw of the entity
        /// </summary>
        public float MinRaw
        {
            get
            {
                return this.minRaw;
            }
        }

        /// <summary>
        /// Gets MaxRaw of the entity
        /// </summary>
        public float MaxRaw
        {
            get
            {
                return this.maxRaw;
            }
        }

        /// <summary>
        /// Converts raw value to egu value
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="minEGU">minimal egu value</param>
        /// <param name="maxEGU">maximal egu value</param>
        /// <returns>returns value in egu format</returns>
        public float ConvertFromRawToEGUValue(float value, float minEGU, float maxEGU)
        {
            if (value < this.minRaw)
            {
                value = this.minRaw;
            }

            if (value > this.maxRaw)
            {
                value = this.maxRaw;
            }

            //if (value != 0)
            //{
            //    minEGU = minEGU * (float)0.9;
            //    maxEGU = maxEGU * (float)1.1;
            //}

            float retVal = ((value - this.minRaw) / (this.maxRaw - this.minRaw)) * (maxEGU - minEGU) + minEGU;
            return retVal;
        }

        /// <summary>
        /// Converts egu value to raw value
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="minEGU">minimal egu value</param>
        /// <param name="maxEGU">maximal egu value</param>
        /// <returns>returns value in raw format</returns>
        public float ConvertFromEGUToRawValue(float value, float minEGU, float maxEGU)
        {
            if (value < minEGU)
            {
                value = minEGU;
            }

            if (value > maxEGU)
            {
                value = maxEGU;
            }

            float retVal = ((value - minEGU) / (maxEGU - minEGU)) * (this.maxRaw - this.minRaw) + this.minRaw;
            return retVal;
        }
    }

}
