using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMeas
{
    public class MeasurementUnitDiscrete
    {
		private long gid;
        private bool currentValue;
        private bool minValue;
        private bool maxValue;
        private DateTime timestamp;

       // private float currentPrice;
        private int scadaAddress;
        
        public MeasurementUnitDiscrete()
        {
            gid = 0;
            currentValue = false;
            minValue = false;
            maxValue = true;
            timestamp = DateTime.Now;
            //currentPrice = 0;
            scadaAddress = 0;
        }

        //public OptimizationType OptimizationType { get; set; }
        
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
        
        public bool CurrentValue
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
        
        public bool MinValue
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
        
        public bool MaxValue
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

        //public float CurrentPrice
       // {
       //     get
       //     {
        //        return currentPrice;
        //    }
        //    set
        //    {
        //        currentPrice = value;
        //    }
        //}

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
