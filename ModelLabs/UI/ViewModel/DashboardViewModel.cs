using CalculationEngineContracts;
using FTN.Common;
using FTN.ServiceContracts;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UI.Model;
using UI.PubSub;
using UI.View;

namespace UI.ViewModel
{
    public class DashboardViewModel : ViewModelBase
    {
        private bool onOff;
        private bool genDigitalCommand;
        public bool OnOff { get { return onOff; } set { onOff = value; OnPropertyChanged(); } }
        bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        private int MAX_DISPLAY_NUMBER = 10;
        private int MAX_DISPLAY_TOTAL_NUMBER = 20;
        private const int NUMBER_OF_ALLOWED_ATTEMPTS = 5; // number of allowed attempts to subscribe to the CE
        private int attemptsCount = 0;
        private double sizeValue;
        private int numOfIterations;
        private int numOfPuplation;
        private int elitsmPercent;
        private float mutationRate;
        private float maxValue = 10;
        private float minValue = 0;
        public float MinValue { get { return minValue; } set { minValue = value; OnPropertyChanged(); } }
        public float MaxValue { get { return maxValue; } set { maxValue = value; OnPropertyChanged(); } }
        public int NumOfIterations { get { return numOfIterations; } set { numOfIterations = value; OnPropertyChanged(); } }
        public int NumOfPuplation { get { return numOfPuplation; } set { numOfPuplation = value; OnPropertyChanged(); } }
        public int ElitsmPercent { get { return elitsmPercent; } set { elitsmPercent = value; OnPropertyChanged(); } }
        public float MutationRate { get { return mutationRate; } set { mutationRate = value; OnPropertyChanged(); } }
        private ICommand changeNumOfIterations;
        public ICommand ChangeNumOfIterations => changeNumOfIterations ?? (changeNumOfIterations = new RelayCommand<object>(ChangeNumOfIterationsExecute));

        private CeSubscribeProxy ceSubscribeProxy;
        private string currentProduction;
        private string currentConsumption;
        private WindSpeed w;
        private WindSpeed windPcnt;
        private float currentWindProduction;
        private float currentSolarProduction;
        private float currentHydroProduction;
        private float currentCoalProduction;
        private float currentOilProduction;
        private float currentGasProduction;

        private readonly double graphSizeOffset = 18;
        private bool isOptionsExpanded = false;

        private ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> generatorsContainer = new ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>>();
        private ObservableCollection<KeyValuePair<long, KeyValuePair<bool, ObservableCollection<MeasurementUI>>>> generatorsContainer2 = new ObservableCollection<KeyValuePair<long, KeyValuePair<bool, ObservableCollection<MeasurementUI>>>>();

        private ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> energyConsumersContainer = new ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>>();

        private ObservableCollection<ModelForCheckboxes> gen = new ObservableCollection<ModelForCheckboxes>();

        private ObservableCollection<KeyValuePair<DateTime, float>> generationList = new ObservableCollection<KeyValuePair<DateTime, float>>();
        private ObservableCollection<KeyValuePair<string, float>> generationByTypeList = new ObservableCollection<KeyValuePair<string, float>>() { new KeyValuePair<string, float>("Wind [kW]", 0), new KeyValuePair<string, float>("Solar [kW]", 0), new KeyValuePair<string, float>("Hydro [MW]", 0), new KeyValuePair<string, float>("Coal [kW]", 0), new KeyValuePair<string, float>("Oil [kW]", 0), new KeyValuePair<string, float>("Gas [kW]", 0), };
        private ObservableCollection<KeyValuePair<DateTime, float>> demandList = new ObservableCollection<KeyValuePair<DateTime, float>>();

        private Dictionary<long, bool> gidToBoolMap = new Dictionary<long, bool>();
        private double graphWidth;
        private double graphHeight;
        private ObservableCollection<WindSpeed> windspeed = new ObservableCollection<WindSpeed>();
        private ObservableCollection<WindSpeed> windPercent = new ObservableCollection<WindSpeed>();
        private ObservableCollection<ColumChartData> totalProductionColumnChart = new ObservableCollection<ColumChartData>() { new ColumChartData("Wind [kW]", 0), new ColumChartData("Solar [kW]", 0), new ColumChartData("Hydro [MW]", 0), new ColumChartData("Coal [kW]", 0), new ColumChartData("Oil [kW]", 0), new ColumChartData("Gas [kW]", 0) };
        private ICommand visibilityCheckedCommand;
        private ICommand visibilityUncheckedCommand;

        private ICommand expandCommand;

        public ICommand VisibilityCheckedCommand => visibilityCheckedCommand ?? (visibilityCheckedCommand = new RelayCommand<long>(VisibilityCheckedCommandExecute));

        public ICommand VisibilityUncheckedCommand => visibilityUncheckedCommand ?? (visibilityUncheckedCommand = new RelayCommand<long>(VisibilityUncheckedCommandExecute));

        private ICommand commandGenMessBox;
        public ICommand CommandGenMessBox => commandGenMessBox ?? (commandGenMessBox = new RelayCommand<object>(CommandGenMessBoxExecute));

        private ICommand activateGen;
        public ICommand ActivateGen => activateGen ?? (activateGen = new RelayCommand<object>(ActivateGenExecute));
        private ICommand deactivateGen;
        private ICommand defaultParamValues;
        public ICommand DeactivateGen => deactivateGen ?? (deactivateGen = new RelayCommand<object>(DeactivateGenExecute));

        public ICommand ExpandCommand => expandCommand ?? (expandCommand = new RelayCommand(ExpandCommandExecute));

        public ICommand DefaultParamValues => defaultParamValues ?? (defaultParamValues = new RelayCommand(DefaultParamValuesExecute));

        private ICommand applyParamValues;
        public ICommand ApplyParamValues => applyParamValues ?? (applyParamValues = new RelayCommand(ApplyParamValuesExecute));

        public WindSpeed W { get => w; set => w = value; }
        public WindSpeed WindPcnt { get => windPcnt; set => windPcnt = value; }
        private void ActivateGenExecute(object obj)
        {
            
                long gid = (long)obj;
                ScadaCommandingProxy.Instance.CommandDiscreteValues(gid, true);
            
                

        }
        private void DeactivateGenExecute(object obj)
        {
            long gid = (long)obj;
            ScadaCommandingProxy.Instance.CommandDiscreteValues(gid, false);
            ScadaCommandingProxy.Instance.CommandAnalogValues(gid, 0);
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

       
        public bool IsOptionsExpanded
        {
            get
            {
                return isOptionsExpanded;
            }

            set
            {
                isOptionsExpanded = value;
                OnPropertyChanged();
            }
        }
        public string CurrentConsumption
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

            var para = CalculationEngineUIProxy.Instance.GetAlgorithmOptions();
            NumOfIterations = para.Item1;
            NumOfPuplation = para.Item2;
            ElitsmPercent = para.Item3;
            MutationRate = para.Item4;

            SizeValue = 0;

            GraphWidth = 16 * graphSizeOffset;
            GraphHeight = 9 * graphSizeOffset;


            W = new WindSpeed();
            
            Windspeed.Add(W);
            Title = "Dashboard";
        }

        public string CurrentProduction
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

        //public ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> GeneratorsContainer { get => generatorsContainer; set => generatorsContainer = value; }
        public ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> GeneratorsContainer { get => generatorsContainer; set => generatorsContainer = value; }
        public ObservableCollection<KeyValuePair<long, ObservableCollection<MeasurementUI>>> EnergyConsumersContainer { get => energyConsumersContainer; set => energyConsumersContainer = value; }

        public ObservableCollection<WindSpeed> Windspeed
        {
            get => windspeed;
            set
            {
                windspeed = value;
                OnPropertyChanged();

            }
        }
        public ObservableCollection<WindSpeed> WindPercent
        {
            get => windPercent;
            set
            {
                windPercent = value;
                OnPropertyChanged();

            }
        }
        public ObservableCollection<ColumChartData> TotalProductionColumnChart
        {
            get => totalProductionColumnChart;
            set
            {
                totalProductionColumnChart = value;
                OnPropertyChanged();

            }
        }
        public ObservableCollection<ModelForCheckboxes> Gen { get => gen; set => gen = value; }


        public ObservableCollection<KeyValuePair<DateTime, float>> GenerationList { get => generationList; set => generationList = value; }
        public ObservableCollection<KeyValuePair<DateTime, float>> DemandList { get => demandList; set => demandList = value; }


        public Dictionary<long, bool> GidToBoolMap { get => gidToBoolMap; set => gidToBoolMap = value; }
        public bool GenDigitalCommand { get => genDigitalCommand; set { genDigitalCommand = value; OnPropertyChanged("GenDigitalCommand"); CommandDigitalValues(value); } }

        public float CurrentWindProduction { get => currentWindProduction; set  { currentWindProduction = value; OnPropertyChanged(); } }
        public float CurrentSolarProduction { get => currentSolarProduction; set { currentSolarProduction = value; OnPropertyChanged(); } }
        public float CurrentHydroProduction { get => currentHydroProduction; set { currentHydroProduction = value; OnPropertyChanged(); } }
        public float CurrentCoalProduction { get => currentCoalProduction; set { currentCoalProduction = value; OnPropertyChanged(); } }
        public float CurrentOilProduction { get => currentOilProduction; set { currentOilProduction = value; OnPropertyChanged(); } }
        public float CurrentGasProduction { get => currentGasProduction; set { currentGasProduction = value; OnPropertyChanged(); } }

        public ObservableCollection<KeyValuePair<string, float>> GenerationByTypeList { get => generationByTypeList; set { generationByTypeList = value; OnPropertyChanged(); } }

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
            UpdateWindSpeed();

            
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
                        CurrentConsumption = measUIs.Sum(x => x.CurrentValue).ToString("0.00");
                        float curC = measUIs.Sum(x => x.CurrentValue);
                        lock (DemandList)
                        {
                            DemandList.Add(new KeyValuePair<DateTime, float>(measUIs.Last().TimeStamp, curC));
                            if (DemandList.Count > MAX_DISPLAY_TOTAL_NUMBER)
                            {
                                DemandList.RemoveAt(0);
                            }
                        }
                    });
                }
                else
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {

                        AddMeasurmentTo(GeneratorsContainer, measUIs);
                        CurrentProduction = measUIs.Sum(x => x.CurrentValue).ToString("0.00");
                        float curP = measUIs.Sum(x => x.CurrentValue);
                        lock (GenerationList)
                        {
                            GenerationList.Add(new KeyValuePair<DateTime, float>(measUIs.Last().TimeStamp, curP));
                            if (GenerationList.Count > MAX_DISPLAY_TOTAL_NUMBER)
                            {
                                GenerationList.RemoveAt(0);
                            }
                        }

                        CurrentWindProduction = measUIs.Where(x => x.GeneratorType == GeneratorType.Wind).Sum(x => x.CurrentValue);
                        CurrentSolarProduction = measUIs.Where(x => x.GeneratorType == GeneratorType.Solar).Sum(x => x.CurrentValue);
                        CurrentHydroProduction = measUIs.Where(x => x.GeneratorType == GeneratorType.Hydro).Sum(x => x.CurrentValue);
                        CurrentCoalProduction = measUIs.Where(x => x.GeneratorType == GeneratorType.Coal).Sum(x => x.CurrentValue) ;
                        CurrentOilProduction = measUIs.Where(x => x.GeneratorType == GeneratorType.Oil).Sum(x => x.CurrentValue);
                        CurrentGasProduction = measUIs.Where(x => x.GeneratorType == GeneratorType.Gas).Sum(x => x.CurrentValue);
                        //lock (GenerationByTypeList)
                        //{
                        //    GenerationByTypeList[0] = new KeyValuePair<string,float>("Wind [kW]",CurrentWindProduction );
                        //    GenerationByTypeList[1] = new KeyValuePair<string, float>("Solar [kW]", CurrentSolarProduction);
                        //    GenerationByTypeList[2] = new KeyValuePair<string, float>("Hydro [MW]", CurrentHydroProduction);
                        //    GenerationByTypeList[3] = new KeyValuePair<string, float>("Coal [kW]", CurrentCoalProduction);
                        //    GenerationByTypeList[4] = new KeyValuePair<string, float>("Oil [kW]", CurrentOilProduction);
                        //    GenerationByTypeList[5] = new KeyValuePair<string, float>("Gas [kW]", CurrentGasProduction);
                            

                        //}
                        ObservableCollection<ColumChartData> newList = new ObservableCollection<ColumChartData>();
                        newList.Add( new ColumChartData("Wind [MW]", CurrentWindProduction));
                        newList.Add(new ColumChartData("Solar [MW]", CurrentSolarProduction));
                        newList.Add( new ColumChartData("Hydro [MW]", CurrentHydroProduction));
                        newList.Add(new ColumChartData("Coal [MW]", CurrentCoalProduction));
                        newList.Add( new ColumChartData("Oil [MW]", CurrentOilProduction));
                        newList.Add( new ColumChartData("Gas [MW]", CurrentGasProduction));

                        TotalProductionColumnChart = newList;
                        //TotalProductionColumnChart.Where(k => k.Type == "Wind [kW]").FirstOrDefault().Production = CurrentWindProduction;
                        //TotalProductionColumnChart.Where(k => k.Type == "Solar [kW]").FirstOrDefault().Production = CurrentSolarProduction;
                        //TotalProductionColumnChart.Where(k => k.Type == "Hydro [MW]").FirstOrDefault().Production = CurrentHydroProduction;
                        //TotalProductionColumnChart.Where(k => k.Type == "Coal [kW]").FirstOrDefault().Production = CurrentCoalProduction;
                        //TotalProductionColumnChart.Where(k => k.Type == "Oil [kW]").FirstOrDefault().Production = CurrentOilProduction;
                        //TotalProductionColumnChart.Where(k => k.Type == "Gas [kW]").FirstOrDefault().Production = CurrentGasProduction;
                        //if (GenerationListWind.Count > MAX_DISPLAY_TOTAL_NUMBER)
                        //{
                        //    GenerationListWind.RemoveAt(0);
                        //}


                        MaxValue = GenerationList.Max(x => x.Value) + 20;
                        MinValue = GenerationList.Min(x => x.Value) - 20;
                        

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

        private void UpdateWindSpeed()
        {
            Random r = new Random();
            float a = r.Next(0, 70);
            a = a + (float)r.NextDouble();
            W.Speed = a;
            ObservableCollection<WindSpeed> newList = new ObservableCollection<WindSpeed>();
            newList.Add(W);

            Windspeed = newList;
        }
        protected override void OnDispose()
        {
            ceSubscribeProxy.Unsubscribe();
            base.OnDispose();
        }

        private void CommandDigitalValues(bool v)
        {
          //  ScadaCommandingProxy.Instance.CommandDiscreteValues(model.Key, v);
        }
        private void ExpandCommandExecute(object obj)
        {
            if (IsOptionsExpanded)
            {
                IsOptionsExpanded = false;
            }
            else
            {
                IsOptionsExpanded = true;
            }
        }
        private void ChangeNumOfIterationsExecute(object obj)
        {


            CalculationEngineUIProxy.Instance.SetAlgorithmOptions(NumOfIterations, NumOfPuplation, ElitsmPercent, MutationRate);


        }
        private void DefaultParamValuesExecute(object obj)
        {
            CalculationEngineUIProxy.Instance.SetAlgorithmOptionsDefault();
            var para = CalculationEngineUIProxy.Instance.GetAlgorithmOptions();
            NumOfIterations = para.Item1;
            NumOfPuplation = para.Item2;
            ElitsmPercent = para.Item3;
            MutationRate = para.Item4;

        }

        private void ApplyParamValuesExecute(object obj)
        {
            CalculationEngineUIProxy.Instance.SetAlgorithmOptions(NumOfIterations, NumOfPuplation, ElitsmPercent, MutationRate);
        }
    }
}
