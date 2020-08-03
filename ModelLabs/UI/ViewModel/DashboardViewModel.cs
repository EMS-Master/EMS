using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.PubSub;

namespace UI.ViewModel
{
    public class DashboardViewModel : ViewModelBase
    {

		private int MAX_DISPLAY_NUMBER = 10;
		private int MAX_DISPLAY_TOTAL_NUMBER = 20;
		private const int NUMBER_OF_ALLOWED_ATTEMPTS = 5; // number of allowed attempts to subscribe to the CE
		private int attemptsCount = 0;
		private double sizeValue;


		private CeSubscribeProxy ceSubscribeProxy;
        private float currentProduction;

		private readonly double graphSizeOffset = 18;

		private ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> generatorsContainer = new ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>>();
        private ObservableCollection<KeyValuePair<DateTime, float>> generationList = new ObservableCollection<KeyValuePair<DateTime, float>>();
        private Dictionary<long, bool> gidToBoolMap = new Dictionary<long, bool>();
		private double graphWidth;
		private double graphHeight;

        private ICommand visibilityCheckedCommand;
        private ICommand visibilityUncheckedCommand;

        public ICommand VisibilityCheckedCommand => visibilityCheckedCommand ?? (visibilityCheckedCommand = new RelayCommand<long>(VisibilityCheckedCommandExecute));

        public ICommand VisibilityUncheckedCommand => visibilityUncheckedCommand ?? (visibilityUncheckedCommand = new RelayCommand<long>(VisibilityUncheckedCommandExecute));
        private void VisibilityCheckedCommandExecute(long gid)
        {
            GidToBoolMap[gid] = true;
            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void VisibilityUncheckedCommandExecute(long gid)
        {
            GidToBoolMap[gid] = false;
            OnPropertyChanged(nameof(GidToBoolMap));
        }



        public double GraphWidth
		{
			get
			{
				return graphWidth;
			}

			set
			{
				graphWidth = value;
				OnPropertyChanged();
			}
		}

		public double GraphHeight
		{
			get
			{
				return graphHeight;
			}

			set
			{
				graphHeight = value;
				OnPropertyChanged();
			}
		}

		public double SizeValue
		{
			get
			{
				return sizeValue;
			}

			set
			{
				sizeValue = value;
				OnPropertyChanged();
				UpdateSizeWidget(value);
			}
		}



		public DashboardViewModel()
        {
            SubsrcibeToCE();
			ceSubscribeProxy.Optimization();

			SizeValue = 0;

			GraphWidth = 16 * graphSizeOffset;
			GraphHeight = 9 * graphSizeOffset;

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

				if (attemptsCount++ < NUMBER_OF_ALLOWED_ATTEMPTS)
				{
					SubsrcibeToCE();
				}
				else
				{
					CommonTrace.WriteTrace(CommonTrace.TraceError, "Could not connect to Publisher Service!  \n {0}", e.Message);
				}

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
				if (GenerationList.Count > MAX_DISPLAY_TOTAL_NUMBER)
				{
					GenerationList.RemoveAt(0);
				}
			});
        }

        private void AddMeasurmentTo(ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> container, List<MeasurementUI> measUIs)
        {
            foreach (var measUI in measUIs)
            {
                var keyPair = container.FirstOrDefault(x => x.Key == measUI.Gid);
                if (keyPair.Value == null)
                {
                    var tempQueue = new ObservableCollection<MeasurementUI>();
                    tempQueue.Add(measUI);
                    container.Add(new KeyValuePair<long, ObservableCollection<MeasurementUI>>(measUI.Gid, tempQueue));
                    if (!GidToBoolMap.ContainsKey(measUI.Gid))
                    {
                        GidToBoolMap.Add(measUI.Gid, true);
                    }

                }
                else
                {
                    keyPair.Value.Add(measUI);
					if (keyPair.Value.Count > MAX_DISPLAY_NUMBER)
					{
						keyPair.Value.RemoveAt(0);
					}
				}
            }
        }

		private void UpdateSizeWidget(double sliderValue)
		{
			GraphWidth = (sliderValue + 1) * 16 * graphSizeOffset;
			GraphHeight = (sliderValue + 1) * 9 * graphSizeOffset;
			MAX_DISPLAY_NUMBER = 10 * ((int)sliderValue + 1);

			foreach (var keyPair in GeneratorsContainer)
			{
				while (keyPair.Value.Count > MAX_DISPLAY_NUMBER)
				{
					keyPair.Value.RemoveAt(0);
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
