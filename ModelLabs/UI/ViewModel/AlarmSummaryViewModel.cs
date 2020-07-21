using CalculationEngineServ;
using CalculationEngineServ.DataBaseModels;
using CalculationEngineService;
using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using UI.PubSub;
using UI.View;

namespace UI.ViewModel
{
    public class AlarmSummaryViewModel : ViewModelBase
    {
        private AlarmsEventsSubscribeProxy aeSubscribeProxy;

        

        private ObservableCollection<Alarm> alarmSummaryQueue = new ObservableCollection<Alarm>();

        private ICommand acknowledgeCommand;

        public object alarmSummaryLock = new object();
        AlarmSummaryView aw = new AlarmSummaryView();


        public ObservableCollection<Alarm> AlarmSummaryQueue
        {
            get
            {
                return alarmSummaryQueue;
            }
            set
            {
                alarmSummaryQueue = value;
                OnPropertyChanged(nameof(AlarmSummaryQueue));
            }
        }

        public AlarmSummaryViewModel()
        {
            Title = "Alarm Summary";

            //using (var db = new EmsContext())
            //{
            //    List<Alarm> al = new List<Alarm>();
            //    foreach (var alarm in db.Alarms)
            //    {
            //        al.Add(alarm);
            //        aw.AlarmSummaryDataGrid.ItemsSource = al;


            //    }

            //}

            try
            {
                aeSubscribeProxy = new AlarmsEventsSubscribeProxy(CallbackAction);
                aeSubscribeProxy.Subscribe();
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Could not connect to Alarm Publisher Service! \n {0}", e.Message);
            }

            try
            {
                //IntegirtyUpdate();
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Successfully finished Integirty update operation for existing Alarms on AES! \n");
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Could not connect to Alarm Events Service for Integirty update operation! \n {0}", e.Message);
            }
        }

        public ICommand AcknowledgeCommand => acknowledgeCommand ?? (acknowledgeCommand = new RelayCommand<Alarm>(AcknowledgeCommandExecute));


        private void CallbackAction(object obj)
        {
            Alarm alarm = obj as Alarm;

            if (obj == null)
            {
                throw new Exception("CallbackAction receive wrong param");
            }

            if (alarm.PubStatus.Equals(PublishingStatus.UPDATE))
            {
                UpdateAlarm(alarm);
            }
            else
            {
                AddAlarm(alarm);
            }
        }

        private void AddAlarm(Alarm alarm)
        {
            lock (alarmSummaryLock)
            {
                List<Alarm> alarmsToRemove = new List<Alarm>(1);
                if (!alarm.AlarmType.Equals(AlarmType.NORMAL))
                {
                    foreach (Alarm aHelper in AlarmSummaryQueue)
                    {
                        if (aHelper.Gid.Equals(alarm.Gid) && aHelper.CurrentState.Contains(State.Active.ToString()))
                        {
                            
                            UpdateAlarm(alarm);
                            return;
                        }
                    }
                }
                else //ako je tip NORMAL
                {
                    foreach (Alarm aHelper in AlarmSummaryQueue)
                    {
                        if (aHelper.Gid.Equals(alarm.Gid) && aHelper.AlarmTimeStamp.Equals(alarm.AlarmTimeStamp))
                            return;
                    }


                }

                foreach (Alarm aHelper in AlarmSummaryQueue)
                {
                    if (aHelper.Gid == alarm.Gid)
                    {
                        alarmsToRemove.Add(aHelper);
                    }
                }
                foreach (Alarm ah in alarmsToRemove)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        AlarmSummaryQueue.Remove(ah);


                    });
                }

                try
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        AlarmSummaryQueue.Add(alarm);
                        

                    });
                }
                catch (Exception e)
                {
                    CommonTrace.WriteTrace(CommonTrace.TraceWarning, "AES can not update alarm values on UI becaus UI instance does not exist. Message: {0}", e.Message);
                }
                OnPropertyChanged(nameof(AlarmSummaryQueue));
            }
        }

        private void UpdateAlarm(Alarm alarm)
        {
            lock (alarmSummaryLock)
            {
                foreach (Alarm aHelper in AlarmSummaryQueue)
                {
                    if (aHelper.Gid.Equals(alarm.Gid) && aHelper.CurrentState.Contains(State.Active.ToString()))
                    {
                        if (!aHelper.CurrentState.Contains(AckState.Acknowledged.ToString()))
                        {
                            aHelper.CurrentState = alarm.CurrentState;
                        }
                        aHelper.Severity = alarm.Severity;
                        aHelper.AlarmValue = alarm.AlarmValue;
                        aHelper.AlarmMessage = alarm.AlarmMessage;
                        aHelper.AlarmTimeStamp = alarm.AlarmTimeStamp;
                    }

                }
                OnPropertyChanged(nameof(AlarmSummaryQueue));

            }
        }

        private void IntegirtyUpdate()
        {
            List<Alarm> integirtyResult = AesIntegrityProxy.Instance.InitiateIntegrityUpdate();
            
            lock (alarmSummaryLock)
            {
                foreach (Alarm alarm in integirtyResult)
                {                   
                    AlarmSummaryQueue.Add(alarm);
                   //aw.AlarmSummaryDataGrid.ItemsSource = AlarmSummaryQueue;

                    OnPropertyChanged(nameof(AlarmSummaryQueue));


                }
            }
        }


        private void AcknowledgeCommandExecute(Alarm alarmHelper)
        {
            using (var db = new EmsContext())
            {
                foreach (Alarm alarm in db.Alarms.ToList())
                {
                    if (alarmHelper.AckState == AckState.Unacknowledged && alarm.Gid==alarmHelper.Gid)
                    {
                        //Alarm al = db.Alarms.Where(a => a.Gid == alarm.Gid).FirstOrDefault();                        
                        //al.AckState = AckState.Acknowledged;
                        alarm.AckState = AckState.Acknowledged;
                        OnPropertyChanged(nameof(db.Alarms));
                      
                        // db.Alarms.Add(alarm);
                        db.SaveChanges();
                    }
                    if (alarmHelper.AckState == AckState.Unacknowledged && alarm.Gid == alarmHelper.Gid && alarmHelper.AlarmMessage.Contains("discret"))
                    {
                        //Alarm al = db.Alarms.Where(a => a.Gid == alarm.Gid && a.AlarmMessage.Contains("discret")).FirstOrDefault();
                        //al.AckState = AckState.Acknowledged;
                        alarm.AckState = AckState.Acknowledged;

                        OnPropertyChanged(nameof(db.Alarms));
                        
                        // db.Alarms.Add(alarm);
                        db.SaveChanges();
                    }
                }                
            }

           
            //using (var db = new EmsContext())
            //{
            //    if (alarmHelper == null)
            //    {
            //        return;
            //    }

            //if (alarmHelper.AckState == AckState.Unacknowledged)
            //{
            //    lock (alarmSummaryLock)
            //    {

            //        foreach (Alarm alarm in db.Alarms.ToList())
            //        {
            //            if (alarm.Gid.Equals(alarmHelper.Gid))
            //            {
            //                alarm.AckState = AckState.Acknowledged;
            //                db.SaveChanges();
            //                //alarm.CurrentState = string.Format("{0} | {1}", alarm.CurrentState.Contains(State.Cleared.ToString()) ? State.Cleared.ToString() : State.Active.ToString(), alarm.AckState.ToString());
            //                OnPropertyChanged(nameof(db.Alarms));
            //                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Alarm acknowledged");
            //            }
            //        }

            //    }
            //}
            //else
            //{
            //    alarmHelper.AckState = AckState.Unacknowledged;
            //    string state = alarmHelper.CurrentState.Contains(State.Cleared.ToString()) ? State.Cleared.ToString() : State.Active.ToString();
            //    alarmHelper.CurrentState = string.Format("{0} | {1}", state, alarmHelper.AckState.ToString());
            //    OnPropertyChanged(nameof(db.Alarms));
            //}
            //}
        }
        
        }

}
