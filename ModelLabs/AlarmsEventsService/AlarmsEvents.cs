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
using System.Data.SqlClient;
using System.Data;
using CalculationEngineService;
using CalculationEngineServ.DataBaseModels;
using CalculationEngineServ;

namespace FTN.Services.AlarmsEventsService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AlarmsEvents : IAlarmsEventsContract, IAesIntegirtyContract
    {
       

        // list for storing AlarmHelper entities
        private List<AlarmHelper> alarms;

        private PublisherService publisher;



        public object alarmLock = new object();
        private List<Alarm> alarmsFromDatabase;
        private Dictionary<long, bool> isNormalCreated = new Dictionary<long, bool>(10);

        public AlarmsEvents()
        {
            this.Publisher = new PublisherService();
            this.Alarms = new List<AlarmHelper>();
            alarmsFromDatabase = SelectAlarmsFromDatabase();
        }

        public PublisherService Publisher
        {
            get
            {
                return this.publisher;
            }

            set
            {
                this.publisher = value;
            }
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

        public void AddAlarm(AlarmHelper alarm)
        {
            if(Alarms.Count == 0 && alarm.Type.Equals(AlarmType.NORMAL))
            {
                return;
            }

            PublishingStatus publishingStatus = PublishingStatus.INSERT;
            bool updated = false;
            bool normalAlarm = false;
            try
            {
                alarm.AckState = AckState.Unacknowledged;
                if (string.IsNullOrEmpty(alarm.CurrentState))
                {
                    alarm.CurrentState = string.Format("{0} | {1}", State.Active, alarm.AckState);
                }
                foreach (AlarmHelper item in Alarms)
                {
                    if (item.Gid.Equals(alarm.Gid) && item.CurrentState.Contains(State.Active.ToString()))
                    {
                        item.Severity = alarm.Severity;
                        item.Value = alarm.Value;
                        item.Message = alarm.Message;
                        item.TimeStamp = alarm.TimeStamp;
                        publishingStatus = PublishingStatus.UPDATE;
                        updated = true;
                        break;
                    }
                    //return to normal
                    else if (item.Gid.Equals(alarm.Gid) && item.CurrentState.Contains(State.Cleared.ToString()))
                    {
                        if (alarm.Type.Equals(AlarmType.NORMAL) /*&& !item.Type.Equals(AlarmType.NORMAL.ToString())*/)
                        {
                            bool normalCreated = false;
                            if (this.isNormalCreated.TryGetValue(alarm.Gid, out normalCreated))
                            {//prodjen je alarm koji je prije kreiran sa alarmom(High/low)
                                if (!normalCreated)
                                {
                                    normalAlarm = true;
                                }
                            }

                            break;
                        }
                    }
                }
                //ako se prvi put pojavio i nije .NORMAL
                if (publishingStatus.Equals(PublishingStatus.INSERT) && !updated && !alarm.Type.Equals(AlarmType.NORMAL))
                {
                    RemoveFromAlarms(alarm.Gid);
                    this.Alarms.Add(alarm);
                    if (InsertAlarmIntoDb(alarm))
                    {
                        Console.WriteLine("Alarm with GID:{0} recorded into alarms database.", alarm.Gid);
                    }
                    this.isNormalCreated[alarm.Gid] = false;
                }
                if (alarm.Type.Equals(AlarmType.NORMAL) && normalAlarm)
                {
                    RemoveFromAlarms(alarm.Gid);
                    this.Alarms.Add(alarm);
                    this.Publisher.PublishAlarmsEvents(alarm, publishingStatus);
                    this.isNormalCreated[alarm.Gid] = true;

                }
                else if (!alarm.Type.Equals(AlarmType.NORMAL))
                {
                    this.Publisher.PublishAlarmsEvents(alarm, publishingStatus);
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
        public bool InsertAlarmIntoDb(AlarmHelper alarm)
        {
            bool success = true;
            try
            {
                using (var db = new EmsContext())
                {
                    db.Alarms.Add(new Alarm { Gid = alarm.Gid, AlarmValue = alarm.Value, MinValue = alarm.MinValue, MaxValue = alarm.MaxValue, AlarmTimeStamp = alarm.TimeStamp, AckState = alarm.AckState, AlarmType = alarm.Type, AlarmMessage = alarm.Message, Severity = alarm.Severity });
                    db.SaveChanges();
                };
            }
            catch (Exception e)
            {
                success = false;
                string message = string.Format("Failed to insert alarm into database. {0}", e.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }
            return success;
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

            foreach (AlarmHelper alarm in this.Alarms)
            {
                if (alarm.Gid.Equals(powerSystemResGid) && alarm.CurrentState.Contains(State.Active.ToString()))
                {
                    alarm.CurrentState = string.Format("{0} | {1}", state, alarm.AckState);
                    alarm.PubStatus = PublishingStatus.UPDATE;
                    if (UpdateAlarmStatusIntoDb(alarm))
                    {
                        Console.WriteLine("Alarm status with GID:{0} updated into database.", alarm.Gid);
                    }
                    try
                    {
                        this.Publisher.PublishStateChange(alarm);
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
        private bool UpdateAlarmStatusIntoDb(AlarmHelper alarm)
        {
            bool success = true;
            
                try
                {
                    using (var db = new EmsContext())
                    {
                        var tmpAlarm = db.Alarms.First(a => a.Gid == alarm.Gid && a.CurrentState.Contains(State.Active.ToString()));
                        tmpAlarm.PubStatus = alarm.PubStatus;
                        tmpAlarm.CurrentState = alarm.CurrentState;
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    success = false;
                    string message = string.Format("Failed to update alarm status into database. {0}", e.Message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    Console.WriteLine(message);
                }
            

            return success;
        }
        private List<Alarm> SelectAlarmsFromDatabase()
        {
            using (var db = new EmsContext())
            {
                return db.Alarms.ToList();
            }
        }
    }
}
