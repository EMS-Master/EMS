using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
   public class MeasurementUI 
    {
        public long Gid { get; set; }
        public long Mrid { get; set; }
        public float CurrentValue { get; set; }
        public string AlarmType { get; set; }
        public DateTime TimeStamp { get; set; }
		public bool IsActive { get; set; }
        public string Name { get; set; }
        public GeneratorType GeneratorType { get; set; }
    }
}
