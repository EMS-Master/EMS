using CalculationEngineServ.DataBaseModels;
using CommonMeas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.AlarmsEventsService.PubSub
{
   public class AlarmUpdateEventArgs : EventArgs
    {
        private Alarm alarm;

        public Alarm Alarm
        {
            get
            {
                return alarm;
            }
            set
            {
                alarm = value;
            }
        }
    }
}
