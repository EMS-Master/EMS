using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.PubSub;

namespace UI.ViewModel
{
    public class DashboardViewModel : ViewModelBase
    {
        private CeSubscribeProxy ceSubscribeProxy;
        private float currentProduction;

        private ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> generatorsContainer = new ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>>();
        private ObservableCollection<KeyValuePair<DateTime, float>> generationList = new ObservableCollection<KeyValuePair<DateTime, float>>();
        private Dictionary<long, bool> gidToBoolMap = new Dictionary<long, bool>();



        public DashboardViewModel()
        {
            SubsrcibeToCE();
            Title = "Dashboard";
        }

        public float CurrentProduction
        {
            get
            {
                return currentProduction;
            }

            set
            {
                currentProduction = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> GeneratorsContainer { get => generatorsContainer; set => generatorsContainer = value; }
        public ObservableCollection<KeyValuePair<DateTime, float>> GenerationList { get => generationList; set => generationList = value; }
        public Dictionary<long, bool> GidToBoolMap { get => gidToBoolMap; set => gidToBoolMap = value; }

        private void SubsrcibeToCE()
        {
            try
            {
                ceSubscribeProxy = new CeSubscribeProxy(CallbackAction);
                ceSubscribeProxy.Subscribe();
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Could not connect to Publisher Service! \n ");
                Thread.Sleep(1000);

                SubsrcibeToCE();

            }
        }
        private void CallbackAction(object obj)
        {
            List<MeasurementUI> measUIs = obj as List<MeasurementUI>;

            if (obj == null)
            {
                throw new Exception("CallbackAction receive wrong parameter");
            }
            if (measUIs.Count == 0)
            {
                return;
            }

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                AddMeasurmentTo(GeneratorsContainer, measUIs);
                CurrentProduction = measUIs.Sum(x => x.CurrentValue);
                GenerationList.Add(new KeyValuePair<DateTime, float>(measUIs.Last().TimeStamp, CurrentProduction));
            });
        }

        private void AddMeasurmentTo(ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> container, List<MeasurementUI> measUIs)
        {
            foreach (var measUI in measUIs)
            {
                var keyPair = container.FirstOrDefault(x => x.Key == measUI.Gid);

                var tempQueue = new ObservableCollection<MeasurementUI>();
                tempQueue.Add(measUI);
                container.Add(new KeyValuePair<long, ObservableCollection<MeasurementUI>>(measUI.Gid, tempQueue));
                if (!GidToBoolMap.ContainsKey(measUI.Gid))
                {
                    GidToBoolMap.Add(measUI.Gid, true);
                }
            }
        }

        protected override void OnDispose()
        {
            ceSubscribeProxy.Unsubscribe();
            base.OnDispose();
        }

    }
}
