﻿using FTN.Common;
using FTN.ServiceContracts;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UI.PubSub;
using UI.View;

namespace UI.ViewModel
{
    public class DashboardViewModel : ViewModelBase
    {
       public CommandViewModel cwm = new CommandViewModel();
        private bool onOff;
        
        public bool OnOff { get { return onOff; } set { onOff = value; OnPropertyChanged(); } }
       
        private int MAX_DISPLAY_NUMBER = 10;
        private int MAX_DISPLAY_TOTAL_NUMBER = 15;
        private const int NUMBER_OF_ALLOWED_ATTEMPTS = 5; // number of allowed attempts to subscribe to the CE
        private int attemptsCount = 0;
        private double sizeValue;


        private CeSubscribeProxy ceSubscribeProxy;
        private float currentProduction;
        private float currentConsumption;


        private readonly double graphSizeOffset = 18;

        private ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> generatorsContainer = new ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>>();
        private ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> energyConsumersContainer = new ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>>();

        private ObservableCollection<ModelForCheckboxes> gen = new ObservableCollection<ModelForCheckboxes>();

        private ObservableCollection<KeyValuePair<DateTime, float>> generationList = new ObservableCollection<KeyValuePair<DateTime, float>>();
        private ObservableCollection<KeyValuePair<DateTime, float>> demandList = new ObservableCollection<KeyValuePair<DateTime, float>>();

        private Dictionary<long, bool> gidToBoolMap = new Dictionary<long, bool>();
        private double graphWidth;
        private double graphHeight;

        private ICommand visibilityCheckedCommand;
        private ICommand visibilityUncheckedCommand;

        private ICommand openHistory;


        public ICommand VisibilityCheckedCommand => visibilityCheckedCommand ?? (visibilityCheckedCommand = new RelayCommand<long>(VisibilityCheckedCommandExecute));

        public ICommand VisibilityUncheckedCommand => visibilityUncheckedCommand ?? (visibilityUncheckedCommand = new RelayCommand<long>(VisibilityUncheckedCommandExecute));

        private ICommand commandGenMessBox;
        public ICommand CommandGenMessBox => commandGenMessBox ?? (commandGenMessBox = new RelayCommand<object>(CommandGenMessBoxExecute));

        private ICommand activateGen;
        public ICommand ActivateGen => activateGen ?? (activateGen = new RelayCommand<object>(ActivateGenExecute));

        private void ActivateGenExecute(object obj)
        {
            KeyValuePair<long, ObservableCollection<MeasurementUI>> model = (KeyValuePair<long, ObservableCollection<MeasurementUI>>)obj;
            bool active = false;
            ModelForCheckboxes model2 = new ModelForCheckboxes();
            
            foreach(var id in cwm.Gens)
            {
                if (model.Key == id.Id)
                {
                    if (id.IsActive == false)
                    {
                        id.IsActive = true;
                        active = id.IsActive;
                    }
                    else
                    {
                        id.IsActive = false;
                        active = id.IsActive;

                    }
                }
            }
           
           // ModelForCheckboxes model = (ModelForCheckboxes)obj;
            ScadaCommandingProxy.Instance.CommandDiscreteValues(model.Key, active);
        }

        private void CommandGenMessBoxExecute(object obj)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to command this element?", "Command", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                CommandGenExecute(obj);
            }
        }


        private void CommandGenExecute(object obj)
        {
            ModelForCheckboxes model = (ModelForCheckboxes)obj;
            if (model.IsActive)
            {
                ScadaCommandingProxy.Instance.CommandAnalogValues(model.Id, model.InputValue);
            }
        }

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

        //public ICommand OpenHistory
        //{
        //    get
        //    {
        //        if (openHistory == null)
        //        {
        //            openHistory = new RelayCommand(param => this.OpenH());
        //        }
        //        return openHistory;
        //    }
        //}

        //public ICommand OpenHistory => openHistory ?? (openHistory = new RelayCommand<object>(OpenH));


        //public void OpenH(object obj)
        //{
        //    HistoryWindow historyWindow = new HistoryWindow();
        //    historyWindow.Show();
        //}


        public float CurrentConsumption
        {
            get
            {
                return currentConsumption;
            }

            set
            {
                currentConsumption = value;
                OnPropertyChanged();
            }
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
        public ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> EnergyConsumersContainer { get => energyConsumersContainer; set => energyConsumersContainer = value; }

        public ObservableCollection<ModelForCheckboxes> Gen { get => gen; set => gen = value; }


        public ObservableCollection<KeyValuePair<DateTime, float>> GenerationList { get => generationList; set => generationList = value; }
        public ObservableCollection<KeyValuePair<DateTime, float>> DemandList { get => demandList; set => demandList = value; }


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


            foreach(var item in measUIs)
            {
                if (item.CurrentValue <= 0)
                {
                    OnOff = false;
                }
                else
                {
                    OnOff = true;
                }
            }
            if (obj == null)
            {
                throw new Exception("CallbackAction receive wrong parameter");
            }
            if (measUIs.Count == 0)
            {
                return;
            }
            try
            {
                if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(measUIs[0].Gid) == DMSType.ENERGY_CONSUMER)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        AddMeasurmentTo(EnergyConsumersContainer, measUIs);
                        CurrentConsumption = measUIs.Sum(x => x.CurrentValue);
                        DemandList.Add(new KeyValuePair<DateTime, float>(measUIs.Last().TimeStamp, CurrentConsumption));
                        if (DemandList.Count > MAX_DISPLAY_TOTAL_NUMBER)
                        {
                            DemandList.RemoveAt(0);
                        }
                    });
                }
                else
                {
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
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "CES can not update measurement values on UI because UI instance does not exist. Message: {0}", e.Message);

            }
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

            foreach (var keyPair in EnergyConsumersContainer)
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
