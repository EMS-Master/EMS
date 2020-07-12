using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.DataBaseModels
{
    public class HistoryMeasurement
    {
        [Key]
        public int Id { get; set; }
        public long Gid { get; set; }
        public DateTime MeasurementTime { get; set; }
        public float MeasurementValue { get; set; }
    }
}
