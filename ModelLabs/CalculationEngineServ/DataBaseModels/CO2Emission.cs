using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.DataBaseModels
{
    public class CO2Emission
    {
        [Key]
        public int Id { get; set; }
        public float PreviousEmission { get; set; }
        public float CurrentEmission { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
