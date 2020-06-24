using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIContract
{
    public class MeasurementUI
    {
        public long Gid { get; set; }

        public long MRid { get; set; }

        public float CurrentValue { get; set; }

        public string AlarmType { get; set; }

        public DateTime TimeStamp { get; set; }

        public int OptimizationType { get; set; }

        public float Price { get; set; }
    }
}
