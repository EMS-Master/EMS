using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.DataBaseModels
{
    public class DiscreteCounter
    {
        [Key]
        public int Id { get; set; }
        public long Gid { get; set; }
        public bool CurrentValue { get; set; }
        public int Counter { get; set; }
        public string Name { get; set; }

    }
}
