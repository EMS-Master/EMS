using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using FTN.ServiceContracts;
using CommonMeas;
using FTN.Common;
using CalculationEngineServ.DataBaseModels;

namespace UI.PubSub
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class AePubSubCallbackService : IAesPubSubCallbackContract
    {
        private Action<object> callbackAction;

        public Action<object> CallbackAction
        {
            get { return callbackAction; }
            set { callbackAction = value; }
        }

        public void AlarmsEvents(Alarm alarm)
        {
            Console.WriteLine("SessionID id: {0}", OperationContext.Current.SessionId);
            Console.WriteLine(string.Format("ALARM: {0} on Signal GID: {1} | SessionID id: {2}",
                                            alarm.AlarmValue.ToString(), alarm.Gid.ToString(), OperationContext.Current.SessionId));

            CommonTrace.WriteTrace(CommonTrace.TraceInfo, string.Format("ALARM: {0} on Signal GID: {1} | SessionID id: {2}",
                                                                        alarm.AlarmValue.ToString(), alarm.Gid.ToString(), OperationContext.Current.SessionId));
            CallbackAction(alarm);
        }

        public void UpdateAlarmsEvents(Alarm alarm)
        {
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, string.Format("UPDATE ALARM: {0} on Signal GID: {1} | SessionID id: {2}",
                                                                       alarm.AlarmValue.ToString(), alarm.Gid.ToString(), OperationContext.Current.SessionId));

            CallbackAction(alarm);
        
         }
    }
}
