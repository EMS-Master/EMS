using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.Services.AlarmsEventsService.PubSub;

namespace FTN.Services.AlarmsEventsService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AlarmsEvents : IAlarmsEventsContract, IAesIntegirtyContract
    {
       

        // list for storing AlarmHelper entities
        private List<AlarmHelper> alarms;


       
        public object alarmLock = new object();
        private Dictionary<long, bool> isNormalCreated = new Dictionary<long, bool>(10);

        public AlarmsEvents()
        {
            
            this.Alarms = new List<AlarmHelper>();
            
        }

        public List<AlarmHelper> Alarms
        {
            get
            {
                return this.alarms;
            }

            set
            {
                this.alarms = value;
            }
        }
       
        //add new alarm
        public void AddAlarm(AlarmHelper alarm)
        {
            bool normalAlarm = false;
            if(Alarms.Count == 0 && alarm.Type.Equals(AlarmType.NORMAL))
            {
                return;
            }

            PublishingStatus publishingStatus = PublishingStatus.INSERT;

            bool updated = false;

            try
            {
                alarm.AckState = AckState.Unacknowledged;
                if (string.IsNullOrEmpty(alarm.CurrentState))
                {
                    alarm.CurrentState = string.Format("{0} | {1}", State.Active, alarm.AckState);
                }

                // cleared status check
                foreach (AlarmHelper item in Alarms)
                {
                    if (item.Gid.Equals(alarm.Gid) && item.CurrentState.Contains(State.Active.ToString()))
                    {
                        item.Severity = alarm.Severity;
                        item.Value = alarm.Value;
                        item.Message = alarm.Message;                   
                        publishingStatus = PublishingStatus.UPDATE;
                        updated = true;
                        break;
                    }
                    else if (item.Gid.Equals(alarm.Gid) && item.CurrentState.Contains(State.Cleared.ToString()))
                    {
                        if (alarm.Type.Equals(AlarmType.NORMAL) && !item.Type.Equals(AlarmType.NORMAL.ToString()))
                        {
                            bool normalCreated = false;
                            if (this.isNormalCreated.TryGetValue(alarm.Gid, out normalCreated))
                            {
                                if (!normalCreated)
                                {
                                    normalAlarm = true;
                                }
                            }

                            break;
                        }
                    }
                }

                // ako je insert dodaj u listu - inace je updateovan
                if (publishingStatus.Equals(PublishingStatus.INSERT) && !updated && !alarm.Type.Equals(AlarmType.NORMAL))
                {
                    RemoveFromAlarms(alarm.Gid);
                    this.Alarms.Add(alarm);
                    
                    this.isNormalCreated[alarm.Gid] = false;
                }
                if (alarm.Type.Equals(AlarmType.NORMAL) && normalAlarm)
                {
                    RemoveFromAlarms(alarm.Gid);
                    this.Alarms.Add(alarm);

                    this.isNormalCreated[alarm.Gid] = true;

                }
                else if (!alarm.Type.Equals(AlarmType.NORMAL))
                {
         
                }

                //Console.WriteLine("AlarmsEvents: AddAlarm method");
                string message = string.Format("Alarm on Analog Gid: {0} - Value: {1}", alarm.Gid, alarm.Value);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            }
            catch (Exception ex)
            {
                string message = string.Format("Greska ", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                //throw new Exception(message);
            }
        }

        private void RemoveFromAlarms(long gid)
        {

            lock (alarmLock)
            {
                List<AlarmHelper> alarmsToRemove = new List<AlarmHelper>(1);
                foreach (AlarmHelper ah in Alarms)
                {
                    if (ah.Gid == gid)
                    {
                        alarmsToRemove.Add(ah);
                    }
                }

                foreach (AlarmHelper ah in alarmsToRemove)
                {
                    Alarms.Remove(ah);
                }
            }
        }


        public List<AlarmHelper> InitiateIntegrityUpdate()
        {
            string message = string.Format("UI client requested integirty update for existing alarms.");
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

            return Alarms;
        }

        public void UpdateStatus(AnalogLocation analogLoc, State state)
        {
            long powerSystemResGid = analogLoc.Analog.PowerSystemResource;
            List<AlarmHelper> alarmsToAdd = new List<AlarmHelper>(2);
            foreach (AlarmHelper alarm in this.Alarms)
            {
                if (alarm.Gid.Equals(powerSystemResGid) && alarm.CurrentState.Contains(State.Active.ToString()))
                {
                    alarm.CurrentState = string.Format("{0} | {1}", state, alarm.AckState);
                    alarm.PubStatus = PublishingStatus.UPDATE;
                    

                    try
                    {
   
                        string message = string.Format("Alarm on Gid: {0} - Changed status: {1}", alarm.Gid, alarm.CurrentState);
                        CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("Greska ", ex.Message);
                        CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                        // throw new Exception(message);
                    }
                }
            }
        }
    }
}
