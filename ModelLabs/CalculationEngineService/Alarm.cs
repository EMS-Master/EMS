using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class Alarm
    {
        [Key]
        public int Gid { get; set; }
        public float AlarmValue { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        //[Column(TypeName = "DateTime2")]
        //public DateTime AlarmTimeStamp { get; set; }
        public int AckState { get; set; }
        public int AlarmType { get; set; }
        public string AlarmMessage { get; set; }
    }
}
