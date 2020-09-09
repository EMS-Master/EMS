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
        private PublisherService publisher;
        public object alarmLock = new object();

        public AlarmsEvents()
        {
            this.Publisher = new PublisherService();
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

        public void AddAlarm(Alarm alarm)
        {
            bool updated = false;
            try
            {
                alarm.AckState = AckState.Unacknowledged;
                if (string.IsNullOrEmpty(alarm.CurrentState))
                {
                    alarm.CurrentState = string.Format("{0}", State.Active);
                }
                if (alarm.AlarmType == AlarmType.DOM)
                {
                    Alarm al = new Alarm();
                    List<Alarm> alarms = DbManager.Instance.GetAlarms().ToList();
                    
                    foreach (var all in alarms)
                    {
                        if (all.AlarmType == AlarmType.DOM && all.Gid == alarm.Gid)
                        {
                            al = all;
                            break;
                        }
                    }
                    
                    if (al.Gid != 0)
                    {
                        UpdateAlarmStatusIntoDb(alarm);
                    }
                    
                }
                else
                {
                    List<Alarm> alarms = DbManager.Instance.GetAlarms().ToList();
                    
                    foreach (Alarm item in alarms)
                    {
                        if (item.Gid.Equals(alarm.Gid) && item.CurrentState.Contains(State.Active.ToString()) && item.AlarmType == alarm.AlarmType)
                        {
                            UpdateAlarmStatusIntoDb(alarm);
                            updated = true;
                            break;
                        }                            
                    }
                    
                    //ako se prvi put pojavio i nije .NORMAL
                    if (!updated /* && !alarm.AlarmType.Equals(AlarmType.NORMAL)*/)
                    {
                        if (InsertAlarmIntoDb(alarm))
                        {
                            Console.WriteLine("Alarm with GID:{0} recorded into alarms database.", alarm.Gid);
                        }
                    }
                    //Console.WriteLine("AlarmsEvents: AddAlarm method");
                    string message = string.Format("Alarm on Analog Gid: {0} - Value: {1}", alarm.Gid, alarm.AlarmValue);
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Greska ", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                //throw new Exception(message);
            }
        }

        //private void RemoveFromAlarms(long gid)
        //{

        //    lock (alarmLock)
        //    {
        //        List<Alarm> alarmsToRemove = new List<Alarm>(1);
        //        foreach (Alarm ah in Alarms)
        //        {
        //            if (ah.Gid == gid)
        //            {
        //                alarmsToRemove.Add(ah);
        //            }
        //        }

        //        foreach (Alarm ah in alarmsToRemove)
        //        {
        //            Alarms.Remove(ah);
        //        }
        //    }
        //}
        public bool InsertAlarmIntoDb(Alarm alarm)
        {
            bool success = true;
            try
            {
                DbManager.Instance.AddAlarm(new Alarm { Gid = alarm.Gid, AlarmValue = alarm.AlarmValue, MinValue = alarm.MinValue, MaxValue = alarm.MaxValue, AlarmTimeStamp = alarm.AlarmTimeStamp, AckState = alarm.AckState, AlarmType = alarm.AlarmType, AlarmMessage = alarm.AlarmMessage, Severity = alarm.Severity, CurrentState=alarm.CurrentState });
                DbManager.Instance.SaveChanges();
               
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

        public List<Alarm> InitiateIntegrityUpdate()
        {
            string message = string.Format("UI client requested integirty update for existing alarms.");
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

            return DbManager.Instance.GetAlarms().ToList();
        }

        public void UpdateStatus(Alarm analogLoc, State state)
        {
            List<Alarm> alarms = DbManager.Instance.GetAlarms().ToList();
            
                foreach (Alarm alarm in alarms)
                {
                    if (alarm.Gid.Equals(analogLoc.Gid) && alarm.CurrentState.Contains(State.Active.ToString()) && alarm.AlarmType != AlarmType.DOM && alarm.AlarmType != AlarmType.NORMAL)
                    {
                        if (UpdateAlarmStatus(alarm))
                        {
                            Console.WriteLine("Alarm status with GID:{0} updated into database.", alarm.Gid);
                        }
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
        private bool UpdateAlarmStatus(Alarm alarm)
        {
            bool success = true;
            try{
                
                var tmpAlarm = DbManager.Instance.GetAlarms().FirstOrDefault(a => a.Gid == alarm.Gid && a.AlarmType == alarm.AlarmType && a.CurrentState.Contains(State.Active.ToString()));
                tmpAlarm.CurrentState = string.Format("{0}", State.Cleared);
                DbManager.Instance.SaveChanges();
                
            }
            catch (Exception e)
            {
                success = false;
                string message = string.Format("Failed to update alarm into database. {0}", e.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }
            return success;
        }
        private bool UpdateAlarmStatusIntoDb(Alarm alarm)
        {
            bool success = true;
            
                try
                {
                    var tmpAlarm = DbManager.Instance.GetAlarms().FirstOrDefault(a => a.Gid == alarm.Gid && a.AlarmType == alarm.AlarmType && a.CurrentState.Contains(State.Active.ToString()));
                    tmpAlarm.AlarmValue = alarm.AlarmValue;
                    tmpAlarm.Severity = alarm.Severity;
                    DbManager.Instance.SaveChanges();                       
                    
                }
                catch (Exception e)
                {
                    success = false;
                    string message = string.Format("Failed to update alarm into database. {0}", e.Message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    Console.WriteLine(message);
                }
            

            return success;
        }
        private List<Alarm> SelectAlarmsFromDatabase()
        {
            return DbManager.Instance.GetAlarms().ToList();
        }
        private List<Alarm> SelectDiscretAlarmsFromDatabase()
        {
            List<Alarm> discret = new List<Alarm>();
            List<Alarm> alarms = DbManager.Instance.GetAlarms().ToList();
            foreach(var a in alarms)
            {
                if (a.AlarmMessage.Contains("discret"))
                {
                    discret.Add(a);
                }
            }
            
            return discret;
        }
        
    }
}
