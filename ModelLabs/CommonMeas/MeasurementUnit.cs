using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMeas
{
    public class MeasurementUnit
    {
        /// <summary>
		/// gid for MeasurementUnit
		/// </summary>
		private long gid;

        /// <summary>
        /// currentValue for MeasurementUnit
        /// </summary>
        private float currentValue;


        /// <summary>
        /// minValue for MeasurementUnit
        /// </summary>
        private float minValue;

        /// <summary>
        /// maxValue for MeasurementUnit
        /// </summary>
        private float maxValue;

        /// <summary>
        /// measurement time
        /// </summary>
        private DateTime timestamp;

        private float currentPrice;
        private int scadaAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnit" /> class
        /// </summary>
        public MeasurementUnit()
        {
            gid = 0;
            currentValue = 0;
            minValue = 0;
            maxValue = 0;
            timestamp = DateTime.Now;
            currentPrice = 0;
            scadaAddress = 0;
        }

        //public OptimizationType OptimizationType { get; set; }

        /// <summary>
        /// Gets or sets Gid of the entity
        /// </summary>
        public long Gid
        {
            get
            {
                return this.gid;
            }

            set
            {
                this.gid = value;
            }
        }

        /// <summary>
        /// Gets or sets CurrentValue of the entity
        /// </summary>
        public float CurrentValue
        {
            get
            {
                return this.currentValue;
            }

            set
            {
                this.currentValue = value;
            }
        }

        /// <summary>
        /// Gets or sets MinValue of the entity
        /// </summary>
        public float MinValue
        {
            get
            {
                return this.minValue;
            }

            set
            {
                this.minValue = value;
            }
        }

        /// <summary>
        /// Gets or sets MaxValue of the entity
        /// </summary>
        public float MaxValue
        {
            get
            {
                return this.maxValue;
            }

            set
            {
                this.maxValue = value;
            }
        }


        /// <summary>
        /// Gets or sets TimeStamp of the entity
        /// </summary>
        public DateTime TimeStamp
        {
            get
            {
                return timestamp;
            }
            set
            {
                timestamp = value;
            }
        }

        public float CurrentPrice
        {
            get
            {
                return currentPrice;
            }
            set
            {
                currentPrice = value;
            }
        }

        public int ScadaAddress
        {
            get
            {
                return scadaAddress;
            }
            set
            {
                scadaAddress = value;
            }
        }
    }
}
