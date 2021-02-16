using CommonMeas;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class UIAlarmUpdateClient : ClientBase<IAlarmsEventsContract>, IAlarmsEventsContract
    {
        public UIAlarmUpdateClient() { }
        public UIAlarmUpdateClient(string endpoint) : base(endpoint) { }
        public void AddAlarm(AlarmHelper alarm)
        {
            Channel.AddAlarm(alarm);
        }

        public void UpdateAckStatus(AlarmHelper alarmtoupdate)
        {
            Channel.UpdateAckStatus(alarmtoupdate);
        }

        public void UpdateStatus(AnalogLocation analogLoc, State state)
        {
            Channel.UpdateStatus(analogLoc, state);
        }
    }
}
