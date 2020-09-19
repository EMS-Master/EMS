using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.DataBaseModels
{
    public class TotalProduction
    {
        [Key]
        public int Id { get; set; }
        public float TotalGeneration { get; set; }
		public float CO2Reduction { get; set; }
		public float CO2Emission { get; set; }
        public float TotalCost { get; set; }
		public float Profit { get; set; }
        public DateTime TimeOfCalculation { get; set; }
    }
}
