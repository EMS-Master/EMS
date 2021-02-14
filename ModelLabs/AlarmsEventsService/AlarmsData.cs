using CommonMeas;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Services.AlarmsEventsService
{
    [DataContract]
    public sealed class AlarmsData
    {
        private static readonly IEnumerable<AlarmHelper> NoAlarms = ImmutableList<AlarmHelper>.Empty;

        public AlarmsData()
        {
            this.Alarms = NoAlarms;
        }

        [DataMember]
        public IEnumerable<AlarmHelper> Alarms { get; private set; }

        [OnDeserialized]
        private void OnDeserialize(StreamingContext context)
        {
            Alarms = Alarms.ToImmutableList();
        }

        public void AddAlarm(AlarmHelper alarm)
        {
            List<AlarmHelper> helperData = new List<AlarmHelper>();
            helperData.Add(alarm);
            Alarms = helperData.ToImmutableList();
        }

        public void AddAlarms(List<AlarmHelper> alarms)
        {
            Alarms = alarms.ToImmutableList();
        }
    }
}
