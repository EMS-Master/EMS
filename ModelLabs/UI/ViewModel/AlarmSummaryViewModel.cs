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
using System.Windows.Input;
using UI.PubSub;

namespace UI.ViewModel
{
   public class AlarmSummaryViewModel : ViewModelBase
    {
        

        private ObservableCollection<AlarmHelper> alarmSummaryQueue = new ObservableCollection<AlarmHelper>();

        private ICommand acknowledgeCommand;

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

            
        }

        public ICommand AcknowledgeCommand => acknowledgeCommand ?? (acknowledgeCommand = new RelayCommand<Alarm>(AcknowledgeCommandExecute));

        private  void AcknowledgeCommandExecute(Alarm alarmHelper)
        {
            using (var db = new EmsContext())
            {
                foreach (var alarm in db.Alarms.ToList())
                {
                    if (alarmHelper.AckState == AckState.Unacknowledged && alarm.Gid==alarmHelper.Gid)
                    {
                        alarm.AckState = AckState.Acknowledged;
                        
                       // db.Alarms.Add(alarm);
                        db.SaveChanges();

                    }
                }

            }
        }
        }

}
