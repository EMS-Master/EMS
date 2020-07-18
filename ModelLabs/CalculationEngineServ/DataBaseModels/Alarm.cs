using CommonMeas;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.DataBaseModels
{
    public class Alarm
    {
        [Key]
        public int Id { get; set; }
        public long Gid { get; set; }
        public float AlarmValue { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public DateTime AlarmTimeStamp { get; set; }
        public AckState AckState { get; set; }
        public AlarmType AlarmType { get; set; }
        public string AlarmMessage { get; set; }
        public SeverityLevel Severity { get; set; }
        public string CurrentState { get; set; }
        public PublishingStatus PubStatus { get; set; }

        public Alarm(long gid, float value, float minValue, float maxValue, DateTime timeStamp)
        {
            this.Gid= gid;
            this.AlarmValue = value;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.AlarmTimeStamp = timeStamp;
            this.AlarmMessage = "";
        }

        public Alarm() { }
    }


}
