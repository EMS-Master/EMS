using CommonMeas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.AlarmsEventsService.PubSub
{
   public class AlarmsEventsEventArgs : EventArgs
    {
        private AlarmHelper alarm;

        public AlarmHelper Alarm
        {
            get { return alarm; }
            set { alarm = value; }
        }
    }
}
