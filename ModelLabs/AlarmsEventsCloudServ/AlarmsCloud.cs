﻿using AESPubSbuContract;
using CalculationEngineServ;
using CommonCloud.AzureStorage.Entities;
using CommonMeas;
using EMS.Services.AlarmsEventsService;
using FTN.ServiceContracts;
using FTN.Services.AlarmsEventsService.PubSub;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmsEventsCloudServ
{
    public class AlarmsCloud : IAlarmsEventsContract, IAesIntegirtyContract
    {
        private List<AlarmHelper> alarms = new List<AlarmHelper>();
        private AesPubSub publisher;
        private IReliableStateManager StateManager;
        private IReliableDictionary<string, AlarmsData> alarmsEventsCache;
        private List<AlarmHelper> alarmsFromDatabase;

        private Dictionary<long, bool> isNormalCreated = new Dictionary<long, bool>(10);
        public object alarmLock = new object();
        private AesPublishSfProxy aesPublishSfProxy = new AesPublishSfProxy();
        public AlarmsCloud()
        {
            this.Publisher = new AesPubSub();
            this.Alarms = new List<AlarmHelper>();
            //alarmsFromDatabase = SelectAlarmsFromDatabase();
        }
        public async void Instantiate(IReliableStateManager stateManager)
        {
            this.StateManager = stateManager;
            alarmsEventsCache = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, AlarmsData>>("AlarmsEventsCache");

            using (var tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<AlarmsData> data = await alarmsEventsCache.TryGetValueAsync(tx, "AlarmsData", LockMode.Update);

                AlarmsData alarmsData = data.HasValue ? data.Value : new AlarmsData();
                try
                {
                    Alarms = alarmsData.Alarms as List<AlarmHelper> ?? new List<AlarmHelper>();
                }
                catch (Exception e)
                {
                    //CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Failed to read alarms from reliable collection. Message: {0}", e.Message);
                    Alarms = new List<AlarmHelper>();
                }

                await tx.CommitAsync();
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

        public AesPubSub Publisher
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


        public void AddAlarm(AlarmHelper alarm)
        {
            lock (alarmLock)
            {
                bool normalAlarm = false;
                this.GetAlarmsFromAlarmsEventsCache();
                if (Alarms.Count == 0 && alarm.Type.Equals(AlarmType.NORMAL))
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
                            item.TimeStamp = alarm.TimeStamp;
                            publishingStatus = PublishingStatus.UPDATE;
                            updated = true;

                            this.UpdateAlarmsEventsCache(item);
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
                        if (alarm.Type != AlarmType.DOM)
                        {
                            //RemoveFromAlarms(alarm.Gid);
                            this.RemoveAlarmFromAlarmsEventsCache(alarm.Gid);
                            //this.Alarms.Add(alarm);
                            this.AddAlarmToAlarmsEventsCache(alarm);
                            if (InsertAlarmIntoDb(alarm))
                            {
                                //ServiceEventSource.Current.Message("Alarm with GID: {0} recorded into alarms database.", alarm.Gid);

                                //Console.WriteLine("Alarm with GID:{0} recorded into alarms database.", alarm.Gid);
                            }
                            this.isNormalCreated[alarm.Gid] = false;
                        }
                        else
                        {
                            //this.Alarms.Add(alarm);
                            this.AddAlarmToAlarmsEventsCache(alarm);
                            if (InsertAlarmIntoDb(alarm))
                            {
                                Console.WriteLine("Alarm with GID:{0} recorded into alarms database.", alarm.Gid);
                            }
                            this.isNormalCreated[alarm.Gid] = false;
                        }
                    }
                    if (alarm.Type.Equals(AlarmType.NORMAL) && normalAlarm)
                    {
                        //RemoveFromAlarms(alarm.Gid);
                        this.RemoveAlarmFromAlarmsEventsCache(alarm.Gid);
                        //this.Alarms.Add(alarm);
                        this.AddAlarmToAlarmsEventsCache(alarm);
                        aesPublishSfProxy.PublishAlarmsEvents(alarm, publishingStatus);
                        this.isNormalCreated[alarm.Gid] = true;

                    }
                    else if (!alarm.Type.Equals(AlarmType.NORMAL))
                    {
                        aesPublishSfProxy.PublishAlarmsEvents(alarm, publishingStatus);
                    }

                    //Console.WriteLine("AlarmsEvents: AddAlarm method");
                    string message = string.Format("Alarm on Analog Gid: {0} - Value: {1}", alarm.Gid, alarm.Value);
                    //CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    //ServiceEventSource.Current.Message(message);

                }
                catch (Exception ex)
                {
                    string message = string.Format("Greska ", ex.Message);
                    //CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    //throw new Exception(message);
                }
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

        private bool InsertAlarmIntoDb(AlarmHelper alarm)
        {
            bool success = true;

            //EmsContext ems = new EmsContext();
            try
            {
                Alarm a = new Alarm(alarm.Gid, alarm.Value, alarm.MinValue, alarm.MaxValue, alarm.TimeStamp)
                {
                    AckState = (int)alarm.AckState,
                    AlarmType = (int)alarm.Type,
                    AlarmMessage = alarm.Message,
                    Severity = (int)alarm.Severity,
                    CurrentState = alarm.CurrentState
                };

                DbManager.Instance.AddAlarm(a);
                //ems.SaveChanges();
                //ServiceEventSource.Current.Message("AlarmsEvents: InsertAlarmIntoDb method.");


            }

            catch (Exception e)
            {
                success = false;
                string message = string.Format("Failed to insert alarm into database. {0}", e.Message);
                //CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }

            return success;
        }



        public List<AlarmHelper> InitiateIntegrityUpdate()
        {
            string message = string.Format("UI client requested integirty update for existing alarms.");
            //CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

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
                    if (UpdateAlarmStatusIntoDb(alarm))
                    {
                        Console.WriteLine("Alarm status with GID:{0} updated into database.", alarm.Gid);
                    }

                    try
                    {
                        aesPublishSfProxy.PublishStateChange(alarm);
                        string message = string.Format("Alarm on Gid: {0} - Changed status: {1}", alarm.Gid, alarm.CurrentState);
                        //ServiceEventSource.Current.Message(message);

                        //CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("Greska ", ex.Message);
                        //CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                        // throw new Exception(message);
                    }
                }
            }
        }

        public void UpdateAckStatus(AlarmHelper alarmtoupdate)
        {
            long powerSystemResGid = alarmtoupdate.Gid;
            foreach (AlarmHelper alarm in this.Alarms)
            {
                if (alarm.Gid.Equals(powerSystemResGid) && alarm.AckState == AckState.Unacknowledged)
                {
                    alarm.AckState = alarmtoupdate.AckState;
                    alarm.CurrentState = alarmtoupdate.CurrentState;
                    alarm.PubStatus = PublishingStatus.UPDATE;
                    if (UpdateAlarmAckStatusIntoDb(alarm))
                    {
                        Console.WriteLine("Alarm status with GID:{0} updated into database.", alarm.Gid);
                    }

                    try
                    {
                        aesPublishSfProxy.PublishStateChange(alarm);
                        string message = string.Format("Alarm on GID: {0} - Changed status: {1}", alarm.Gid, alarm.CurrentState);
                        ServiceEventSource.Current.Message(message);

                        //CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("Greska ", ex.Message);
                        //CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                        // throw new Exception(message);
                    }
                }
            }
        }

        private bool UpdateAlarmStatus(Alarm alarm)
        {
            bool success = true;
            try
            {

                var tmpAlarm = DbManager.Instance.GetAlarm(alarm.Gid, alarm.AlarmType, State.Active.ToString());//.FirstOrDefault(a => a.Gid == alarm.Gid && a.AlarmType == alarm.AlarmType && a.CurrentState.Contains(State.Active.ToString()));
                tmpAlarm.CurrentState = string.Format("{0}", State.Cleared);
                DbManager.Instance.AddAlarm(tmpAlarm);
                //ServiceEventSource.Current.Message("AlarmsEvents: UpdateAlarmStatus method.");

                // DbManager.Instance.SaveChanges();

            }
            catch (Exception e)
            {
                success = false;
                string message = string.Format("Failed to update alarm into database. {0}", e.Message);
                //CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }
            return success;
        }
        private bool UpdateAlarmStatusIntoDb(AlarmHelper alarm)
        {
            bool success = true;

            try
            {
                var tmpAlarm = DbManager.Instance.GetAlarm(alarm.Gid, (int)alarm.Type, State.Active.ToString());
                //var tmpAlarm = alarmList;//?.FirstOrDefault(a => a.Gid == alarm.Gid && a.AlarmType == (int)alarm.Type && a.CurrentState.Contains(State.Active.ToString()));
                tmpAlarm.AlarmValue = alarm.Value;
                tmpAlarm.Severity = (int)alarm.Severity;
                DbManager.Instance.AddAlarm(tmpAlarm);
                // DbManager.Instance.SaveChanges();                       

            }
            catch (Exception e)
            {
                success = false;
                string message = string.Format("Failed to update alarm into database. {0}", e.Message);
                //CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                //Console.WriteLine(message);
            }


            return success;
        }
        private bool UpdateAlarmAckStatusIntoDb(AlarmHelper alarm)
        {
            bool success = true;

            try
            {
                var tmpAlarm = DbManager.Instance.GetAlarm(alarm.Gid, (int)alarm.Type, State.Active.ToString());//.FirstOrDefault(a => a.Gid == alarm.Gid && a.AlarmType == (int)alarm.Type && a.CurrentState.Contains(State.Active.ToString()));
                tmpAlarm.AckState = (int)alarm.AckState;
                DbManager.Instance.AddAlarm(tmpAlarm);
                // DbManager.Instance.SaveChanges();

            }
            catch (Exception e)
            {
                success = false;
                string message = string.Format("Failed to update alarm into database. {0}", e.Message);
                //CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                Console.WriteLine(message);
            }


            return success;
        }


        //private List<AlarmHelper> SelectAlarmsFromDatabase()
        //{
        //    List<AlarmHelper> alarms = new List<AlarmHelper>();

        //   // EmsContext ems = new EmsContext();

        //        try
        //        {
        //            var alarmsdb = DbManager.Instance.GetAlarms(); 

        //            foreach (var item in alarmsdb)
        //            {
        //                AlarmHelper alarm = new AlarmHelper
        //                {
        //                    Gid = item.Gid,
        //                    Severity = (SeverityLevel)item.Severity,
        //                    Value = (float)item.AlarmValue,
        //                    MinValue = (float)item.MinValue,
        //                    MaxValue = (float)item.MaxValue,
        //                    TimeStamp = item.AlarmTimeStamp,
        //                    CurrentState = item.CurrentState,
        //                    AckState = (AckState)item.AckState,
        //                    PubStatus = (PublishingStatus)item.PubStatus,
        //                    Type = (AlarmType)item.AlarmType,
        //                    Message = item.AlarmMessage,

        //                };

        //                alarms.Add(alarm);
        //            }

        //        }
        //        catch (Exception e)
        //        {
        //            string message = string.Format("Failed read alarms from database. {0}", e.Message);
        //            CommonTrace.WriteTrace(CommonTrace.TraceError, message);
        //            Console.WriteLine(message);
        //        }

        //    return alarms;
        //}
        //private List<Alarm> SelectDiscretAlarmsFromDatabase()
        //{
        //    List<Alarm> discret = new List<Alarm>();
        //    List<Alarm> alarms = DbManager.Instance.GetAlarms().ToList();
        //    foreach(var a in alarms)
        //    {
        //        if (a.AlarmMessage.Contains("discret"))
        //        {
        //            discret.Add(a);
        //        }
        //    }

        //    return discret;
        //}

        public async void GetAlarmsFromAlarmsEventsCache()
        {
            try
            {
                alarmsEventsCache = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, AlarmsData>>("AlarmsEventsCache");
                using (ITransaction tx = this.StateManager.CreateTransaction())
                {
                    ConditionalValue<AlarmsData> data = await alarmsEventsCache.TryGetValueAsync(tx, "AlarmsData", LockMode.Update);

                    if (data.HasValue)
                    {
                        this.Alarms = data.Value.Alarms.ToList();
                    }
                    else
                    {
                        this.Alarms = new List<AlarmHelper>();
                    }

                    await tx.CommitAsync();
                }
            }
            catch (Exception)
            {
            }
        }
        public async void UpdateAlarmsEventsCache(AlarmHelper alarmHelper)
        {
            try
            {
                alarmsEventsCache = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, AlarmsData>>("AlarmsEventsCache");
                using (ITransaction tx = this.StateManager.CreateTransaction())
                {
                    ConditionalValue<AlarmsData> data = await alarmsEventsCache.TryGetValueAsync(tx, "AlarmsData", LockMode.Update);

                    if (data.HasValue)
                    {
                        List<AlarmHelper> alarms = data.Value.Alarms.ToList();

                        foreach (AlarmHelper item in alarms)
                        {
                            if (item.Gid == alarmHelper.Gid)
                            {
                                item.Severity = alarmHelper.Severity;
                                item.Value = alarmHelper.Value;
                                item.Message = alarmHelper.Message;
                                item.TimeStamp = alarmHelper.TimeStamp;
                            }
                        }

                        AlarmsData alarmsData = new AlarmsData();
                        alarmsData.AddAlarms(alarms);

                        await alarmsEventsCache.SetAsync(tx, "AlarmsData", alarmsData);

                        await tx.CommitAsync();
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
        public async void RemoveAlarmFromAlarmsEventsCache(long gid)
        {
            try
            {
                AlarmHelper alarmToRemove = null;
                alarmsEventsCache = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, AlarmsData>>("AlarmsEventsCache");
                using (ITransaction tx = this.StateManager.CreateTransaction())
                {
                    ConditionalValue<AlarmsData> data = await alarmsEventsCache.TryGetValueAsync(tx, "AlarmsData", LockMode.Update);

                    if (data.HasValue)
                    {
                        List<AlarmHelper> alarms = data.Value.Alarms.ToList();
                        foreach (AlarmHelper alarm in alarms)
                        {
                            if (alarm.Gid == gid)
                            {
                                alarmToRemove = alarm;
                                break;
                            }
                        }

                        if (alarmToRemove != null)
                        {
                            alarms.Remove(alarmToRemove);
                        }

                        AlarmsData alarmsData = new AlarmsData();
                        alarmsData.AddAlarms(alarms);

                        await alarmsEventsCache.SetAsync(tx, "AlarmsData", alarmsData);
                        await tx.CommitAsync();
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
        public async void AddAlarmToAlarmsEventsCache(AlarmHelper alarmHelper)
        {
            try
            {
                alarmsEventsCache = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, AlarmsData>>("AlarmsEventsCache");
                using (ITransaction tx = this.StateManager.CreateTransaction())
                {
                    ConditionalValue<AlarmsData> data = await alarmsEventsCache.TryGetValueAsync(tx, "AlarmsData", LockMode.Update);
                    AlarmsData alarmsData = new AlarmsData();
                    if (data.HasValue)
                    {
                        List<AlarmHelper> alarms = data.Value.Alarms.ToList();
                        alarms.Add(alarmHelper);

                        alarmsData.AddAlarms(alarms);
                    }
                    else
                    {
                        alarmsData = new AlarmsData();
                        alarmsData.AddAlarm(alarmHelper);
                    }

                    await alarmsEventsCache.SetAsync(tx, "AlarmsData", alarmsData);
                    await tx.CommitAsync();
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
