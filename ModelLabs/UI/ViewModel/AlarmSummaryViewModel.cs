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
        private string combo1;
        private string combo2;

        private AlarmsEventsSubscribeProxy aeSubscribeProxy;

        

        private ObservableCollection<AlarmHelper> alarmSummaryQueue = new ObservableCollection<AlarmHelper>();
        private ObservableCollection<string> sourceCombo2 = new ObservableCollection<string>();


        private ICommand acknowledgeCommand;
        private ICommand hideClick;
        public object alarmSummaryLock = new object();


        public ObservableCollection<AlarmHelper> AlarmSummaryQueue
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
                IntegirtyUpdate();
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Successfully finished Integirty update operation for existing Alarms on AES! \n");
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Could not connect to Alarm Events Service for Integirty update operation! \n {0}", e.Message);
            }
        }

        public ICommand HideClick => hideClick ?? (hideClick = new RelayCommand(HideCLickExecute));
        public ICommand AcknowledgeCommand => acknowledgeCommand ?? (acknowledgeCommand = new RelayCommand<AlarmHelper>(AcknowledgeCommandExecute));

        public string Combo1 { get => combo1;
            set
            {
                combo1 = value.Split(':')[1];
                OnPropertyChanged();

                if (combo1.Contains("Type Alarm"))
                {
                    SourceCombo2 = new ObservableCollection<string>() { "NORMAL", "HIGH", "LOW", "DOM" };
                }
                else if (combo1.Contains("Severity"))
                {
                    SourceCombo2 = new ObservableCollection<string>() { "HIGH", "LOW", "MEDIUM" };
                }
                else if (combo1.Contains("Name"))
                {

                }

                OnPropertyChanged("SourceCombo2");
            }
        }

        public string Combo2
        {
            get => combo2;
            set
            {
                combo2 = value;
                if (combo2 != null)
                {
                    OnPropertyChanged();

                    if (Combo1.Contains("Type Alarm"))
                    {
                        if (combo2.Equals("NORMAL") || combo2.Equals("HIGH") || combo2.Equals("LOW") || combo2.Equals("DOM"))
                        {
                            foreach (var item in AlarmSummaryQueue)
                            {
                                if (item.Type.ToString() != combo2)
                                    item.IsVisible = false;
                                else
                                    item.IsVisible = true;
                            }
                        }
                    }
                    else if (Combo1.Contains("Severity"))
                    {
                        if (combo2.Equals("HIGH") || combo2.Equals("LOW") || combo2.Equals("MEDIUM"))
                        {
                            foreach (var item in AlarmSummaryQueue)
                            {
                                if (item.Severity.ToString() != combo2)
                                    item.IsVisible = false;
                                else
                                    item.IsVisible = true;
                            }
                        }
                    }

                    OnPropertyChanged("AlarmSummaryQueue");
                }

            }
        }

        public ObservableCollection<string> SourceCombo2 { get => sourceCombo2; set => sourceCombo2 = value; }

        private void CallbackAction(object obj)
        {
            AlarmHelper alarm = obj as AlarmHelper;

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

        private void AddAlarm(AlarmHelper alarm)
        {
            lock (alarmSummaryLock)
            {
                List<AlarmHelper> alarmsToRemove = new List<AlarmHelper>(1);
                if (!alarm.Type.Equals(AlarmType.NORMAL))
                {
                    foreach (AlarmHelper aHelper in AlarmSummaryQueue)
                    {
                        if (aHelper.Gid.Equals(alarm.Gid) && aHelper.CurrentState.Contains(State.Active.ToString()))
                        {
                            //aHelper.CurrentState = string.Format("{0}, {1}", State.Active, aHelper.AckState);
                            //OnPropertyChanged(nameof(AlarmSummaryQueue));
                            UpdateAlarm(alarm);
                            return;
                        }
                    }
                }
                else //ako je tip NORMAL
                {
                    foreach (AlarmHelper aHelper in AlarmSummaryQueue)
                    {
                        if (aHelper.Gid.Equals(alarm.Gid) && aHelper.TimeStamp.Equals(alarm.TimeStamp))
                            return;
                    }


                }

                foreach (AlarmHelper aHelper in AlarmSummaryQueue)
                {
                    if (aHelper.Gid == alarm.Gid)
                    {
                        alarmsToRemove.Add(aHelper);
                    }
                }
                foreach (AlarmHelper ah in alarmsToRemove)
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

        private void UpdateAlarm(AlarmHelper alarm)
        {
            lock (alarmSummaryLock)
            {
                foreach (AlarmHelper aHelper in AlarmSummaryQueue)
                {
                    if (aHelper.Gid.Equals(alarm.Gid) && aHelper.CurrentState.Contains(State.Active.ToString()))
                    {
                        if (!aHelper.CurrentState.Contains(AckState.Acknowledged.ToString()))
                        {
                            aHelper.CurrentState = alarm.CurrentState;
                        }
                        aHelper.Severity = alarm.Severity;
                        aHelper.Value = alarm.Value;
                        aHelper.Message = alarm.Message;
                        aHelper.TimeStamp = alarm.TimeStamp;
                    }
                }
                OnPropertyChanged(nameof(AlarmSummaryQueue));
            }
        }

        private void IntegirtyUpdate()
        {
            List<AlarmHelper> integirtyResult = AesIntegrityProxy.Instance.InitiateIntegrityUpdate();
            
            lock (alarmSummaryLock)
            {
                foreach (AlarmHelper alarm in integirtyResult)
                {                   
                    AlarmSummaryQueue.Add(alarm);
                   //aw.AlarmSummaryDataGrid.ItemsSource = AlarmSummaryQueue;

                    OnPropertyChanged(nameof(AlarmSummaryQueue));

                }
            }
        }

        private void AcknowledgeCommandExecute(AlarmHelper alarmHelper)
        {
            AlarmHelper alarmToRemove = new AlarmHelper();
            EmsContext e = new EmsContext();
            if (alarmHelper == null)
            {
                return;
            }

            if (alarmHelper.AckState == AckState.Unacknowledged)
            {
                
                lock (alarmSummaryLock)
                {
                    foreach (AlarmHelper alarm in AlarmSummaryQueue)
                    {
                        if (alarm.Gid.Equals(alarmHelper.Gid) && alarm.Persistent.Equals(PersistentState.Nonpersistent))
                        {
                            alarmHelper.AckState = AckState.Acknowledged;
                            alarmToRemove = alarm;
                            break;
                        }
                        else if (alarm.Gid.Equals(alarmHelper.Gid) && alarm.Persistent.Equals(PersistentState.Persistent))
                        {
                            alarmHelper.AckState = AckState.Acknowledged;
                            alarm.AckState = AckState.Acknowledged;
                            alarm.CurrentState = string.Format("{0} | {1}", alarm.CurrentState.Contains(State.Cleared.ToString()) ? State.Cleared.ToString() : State.Active.ToString(), alarm.AckState.ToString());
                            OnPropertyChanged(nameof(AlarmSummaryQueue));
                            CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Persistent alarm acknowledged");
                        }
                        
                    }
                    if (alarmToRemove != null)
                    {

                        AlarmSummaryQueue.Remove(alarmToRemove);
                        OnPropertyChanged(nameof(AlarmSummaryQueue));
                        CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Non persistent alarm acknowledged and removed from alarm summary collection");
                    }
                }
            }
            else
            {
                alarmHelper.AckState = AckState.Unacknowledged;
                string state = alarmHelper.CurrentState.Contains(State.Cleared.ToString()) ? State.Cleared.ToString() : State.Active.ToString();
                alarmHelper.CurrentState = string.Format("{0} | {1}", state, alarmHelper.AckState.ToString());
                OnPropertyChanged(nameof(AlarmSummaryQueue));
            }
            AlarmsEventsProxy.Instance.UpdateAckStatus(alarmHelper);
           
        }

        private void HideCLickExecute(object o)
       {
            ObservableCollection<AlarmHelper> altoremove = new ObservableCollection<AlarmHelper>();
            lock(alarmSummaryLock)
            {
                foreach (var a in AlarmSummaryQueue)
                {
                    if (a.AckState == AckState.Acknowledged)
                    {
                        altoremove.Add(a);
                    }
                }
                foreach (var a in altoremove)
                {
                    AlarmSummaryQueue.Remove(a);
                    OnPropertyChanged("AlarmSummaryQueue");
                }
                
            }
            


            
        }

       

    }

}
