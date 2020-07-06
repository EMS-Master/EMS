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
        private List<Alarm> SelectAlarmsFromDatabase()
        {
            using (var db = new AlarmContext())
            {
                return db.Alarms.ToList();
            }
        }

            //add new alarm
        public void AddAlarm(AlarmHelper alarm)
        {
            if(Alarms.Count == 0 && alarm.Type.Equals(AlarmType.NORMAL))
            {
                return;
            }

            try
            {
                alarm.AckState = AckState.Unacknowledged;
                if (string.IsNullOrEmpty(alarm.CurrentState))
                {
                    alarm.CurrentState = string.Format("{0} | {1}", State.Active, alarm.AckState);
                }

                this.Alarms.Add(alarm);
                if (InsertAlarmIntoDb(alarm))
                {
                    Console.WriteLine("Alarm with GID:{0} recorded into alarms database.", alarm.Gid);
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
                using (var db = new AlarmContext())
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
                    using (var db = new AlarmContext())
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
    }
}
