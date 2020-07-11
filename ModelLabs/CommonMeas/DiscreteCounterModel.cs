using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMeas
{
    public class DiscreteCounterModel
    {
        public int Id { get; set; }
        public long Gid { get; set; }
        public bool CurrentValue { get; set; }
        public int Counter { get; set; }
    }
}
