using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Model;

namespace UI.ViewModel
{
    public class HistoryViewModel : ViewModelBase
    {
        #region Fields

        private DateTime startTime;
        private DateTime endTime;
        private PeriodValues selectedPeriod;
        private GraphSample graphSampling;
        private ICommand allGeneratorsCheckedCommand;
        private ICommand allGeneratorsUnheckedCommand;
        private ICommand allBatteryStoragesCheckedCommand;
        private ICommand allBatteryStoragesUnheckedCommand;
        private ICommand selectedPeriodCommand;
        private ObservableCollection<long> generatorsFromNms = new ObservableCollection<long>();
        private ObservableCollection<long> batteryStoragesFromNms = new ObservableCollection<long>();
        private ICommand showDataCommand;
        private bool isExpandedSeparated = false;
        private ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>> generatorsContainer = new ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>>();
        private bool isExpandedTotalProduction = false;
        private ICommand totalProductionGraphCheckedCommand;
        private ICommand totalProductionGraphUnCheckedCommand;
        private ICommand productionForSelectedCheckedCommand;
        private ICommand productionForSelectedUnCheckedCommand;

        private ICommand totalLoadGraphCheckedCommand;
        private ICommand totalLoadGraphUnCheckedCommand;
        private ICommand loadForSelectedCheckedCommand;
        private ICommand loadForSelectedUnCheckedCommand;

        private bool totalProductionForSelectedVisible = false;
        private bool totalProductionGraphVisible = true;

        private bool totalLoadForSelectedVisible = false;
        private bool totalLoadGraphVisible = true;

        private ObservableCollection<Tuple<double, DateTime>> graphTotalProduction = new ObservableCollection<Tuple<double, DateTime>>();
        private Dictionary<long, bool> gidToBoolMap = new Dictionary<long, bool>();


        #endregion Fields

        #region Properties
        public ObservableCollection<long> GeneratorsFromNms
        {
            get
            {
                return generatorsFromNms;
            }
            set
            {
                generatorsFromNms = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<long> BatteryStoragesFromNms
        {
            get
            {
                return batteryStoragesFromNms;
            }
            set
            {
                batteryStoragesFromNms = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowDataCommand => showDataCommand ?? (showDataCommand = new RelayCommand(ShowDataCommandExecute));

        public bool IsExpandedSeparated
        {
            get
            {
                return isExpandedSeparated;
            }

            set
            {
                isExpandedSeparated = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>> GeneratorsContainer { get => generatorsContainer; set => generatorsContainer = value; }

        public bool IsExpandedTotalProduction
        {
            get
            {
                return isExpandedTotalProduction;
            }

            set
            {
                isExpandedTotalProduction = value;
                OnPropertyChanged();
            }
        }

        public ICommand TotalProductionGraphCheckedCommand => totalProductionGraphCheckedCommand ?? (totalProductionGraphCheckedCommand = new RelayCommand(TotalProductionGraphCheckedExecute));

        public ICommand TotalProductionGraphUnCheckedCommand => totalProductionGraphUnCheckedCommand ?? (totalProductionGraphUnCheckedCommand = new RelayCommand(TotalProductionGraphUnCheckedExecute));
        
        public ICommand ProductionForSelectedCheckedCommand => productionForSelectedCheckedCommand ?? (productionForSelectedCheckedCommand = new RelayCommand(ProductionForSelectedCheckedExecute));

        public ICommand ProductionForSelectedUnCheckedCommand => productionForSelectedUnCheckedCommand ?? (productionForSelectedUnCheckedCommand = new RelayCommand(ProductionForSelectedUnCheckedExecute));


        public ICommand TotalLoadGraphCheckedCommand => totalLoadGraphCheckedCommand ?? (totalLoadGraphCheckedCommand = new RelayCommand(TotalLoadGraphCheckedExecute));

        public ICommand TotalLoadGraphUnCheckedCommand => totalLoadGraphUnCheckedCommand ?? (totalLoadGraphUnCheckedCommand = new RelayCommand(TotalLoadGraphUnCheckedExecute));

        public ICommand LoadForSelectedCheckedCommand => loadForSelectedCheckedCommand ?? (loadForSelectedCheckedCommand = new RelayCommand(LoadForSelectedCheckedExecute));

        public ICommand LoadForSelectedUnCheckedCommand => loadForSelectedUnCheckedCommand ?? (loadForSelectedUnCheckedCommand = new RelayCommand(LoadForSelectedUnCheckedExecute));

        public ICommand ChangePeriodCommand => selectedPeriodCommand ?? (selectedPeriodCommand = new RelayCommand(SelectedPeriodCommandExecute));

        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                graphSampling = GraphSample.None;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        public DateTime EndTime
        {
            get { return endTime; }
            set
            {
                endTime = value;
                graphSampling = GraphSample.None;
                OnPropertyChanged(nameof(EndTime));
            }
        }
        public bool TotalProductionForSelectedVisible
        {
            get
            {
                return totalProductionForSelectedVisible;
            }
            set
            {
                totalProductionForSelectedVisible = value;
                OnPropertyChanged(nameof(TotalProductionForSelectedVisible));
            }
        }

        public bool TotalProductionGraphVisible
        {
            get
            {
                return totalProductionGraphVisible;
            }
            set
            {
                totalProductionGraphVisible = value;
                OnPropertyChanged(nameof(TotalProductionGraphVisible));
            }
        }

        public bool TotalLoadForSelectedVisible
        {
            get
            {
                return totalLoadForSelectedVisible;
            }
            set
            {
                totalLoadForSelectedVisible = value;
                OnPropertyChanged(nameof(TotalLoadForSelectedVisible));
            }
        }

        public bool TotalLoadGraphVisible
        {
            get
            {
                return totalLoadGraphVisible;
            }
            set
            {
                totalLoadGraphVisible = value;
                OnPropertyChanged(nameof(TotalLoadGraphVisible));
            }
        }

        public ObservableCollection<Tuple<double, DateTime>> GraphTotalProduction { get => graphTotalProduction; set => graphTotalProduction = value; }
        public PeriodValues SelectedPeriod { get => selectedPeriod; set => selectedPeriod = value; }
        public GraphSample GraphSampling { get => graphSampling; set => graphSampling = value; }

        public ICommand AllGeneratorsCheckedCommand => allGeneratorsCheckedCommand ?? (allGeneratorsCheckedCommand = new RelayCommand(AllGeneratorsCheckedCommandExecute));

        public ICommand AllGeneratorsUncheckedCommand => allGeneratorsUnheckedCommand ?? (allGeneratorsUnheckedCommand = new RelayCommand(AllGeneratorsUnheckedCommandExecute));

        public ICommand AllBatteryStoragesCheckedCommand => allBatteryStoragesCheckedCommand ?? (allBatteryStoragesCheckedCommand = new RelayCommand(AllBatteryStoragesCheckedCommandExecute));

        public ICommand AllBatteryStoragesUncheckedCommand => allBatteryStoragesUnheckedCommand ?? (allBatteryStoragesUnheckedCommand = new RelayCommand(AllBatteryStoragesUnheckedCommandExecute));

        public Dictionary<long, bool> GidToBoolMap
        {
            get
            {
                return gidToBoolMap;
            }

            set
            {
                gidToBoolMap = value;
            }
        }

        #endregion Properties
        public HistoryViewModel()
        {
            Title = "History";
            startTime = DateTime.Now.AddMinutes(-1);
            endTime = DateTime.Now;
            GraphSampling = GraphSample.None;
            SelectedPeriod = PeriodValues.None;

            IntegrityUpdateForGenerators();
        }
        public void IntegrityUpdateForGenerators()
        {
        }

        #region Commands

        private void ProductionForSelectedCheckedExecute(object obj)
        {
            TotalProductionForSelectedVisible = true;
        }

        private void ProductionForSelectedUnCheckedExecute(object obj)
        {
            TotalProductionForSelectedVisible = false;
        }

        private void TotalProductionGraphCheckedExecute(object obj)
        {
            TotalProductionGraphVisible = true;
        }

        private void TotalProductionGraphUnCheckedExecute(object obj)
        {
            TotalProductionGraphVisible = false;
        }

        private void LoadForSelectedCheckedExecute(object obj)
        {
            TotalLoadForSelectedVisible = true;
        }

        private void LoadForSelectedUnCheckedExecute(object obj)
        {
            TotalLoadForSelectedVisible = false;
        }

        private void TotalLoadGraphCheckedExecute(object obj)
        {
            TotalLoadGraphVisible = true;
        }

        private void TotalLoadGraphUnCheckedExecute(object obj)
        {
            TotalLoadGraphVisible = false;
        }



        private void ShowDataCommandExecute(object obj)
        {
        }

        private void AllGeneratorsCheckedCommandExecute(object obj)
        {
            GidToBoolMap = GidToBoolMap.ToDictionary(p => p.Key, p => true);
            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void AllGeneratorsUnheckedCommandExecute(object obj)
        {
            GidToBoolMap = GidToBoolMap.ToDictionary(p => p.Key, p => false);
            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void AllBatteryStoragesCheckedCommandExecute(object obj)
        {
            GidToBoolMap = GidToBoolMap.ToDictionary(p => p.Key, p => true);
            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void AllBatteryStoragesUnheckedCommandExecute(object obj)
        {
            GidToBoolMap = GidToBoolMap.ToDictionary(p => p.Key, p => false);
            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void SelectedPeriodCommandExecute(object obj)
        {
            UpdatePeriod();
        }

        private void UpdatePeriod()
        {
            switch (SelectedPeriod)
            {
                case PeriodValues.Last_Hour:
                    StartTime = DateTime.Now.AddHours(-1);
                    EndTime = DateTime.Now;
                    graphSampling = GraphSample.HourSample;
                    break;
                case PeriodValues.Today:
                    StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                    EndTime = DateTime.Now;
                    graphSampling = GraphSample.TodaySample;
                    break;
                case PeriodValues.Last_Month:
                    StartTime = DateTime.Now.AddMonths(-1);
                    EndTime = DateTime.Now;
                    graphSampling = GraphSample.LastMonthSample;
                    break;
                case PeriodValues.Last_3_Month:
                    StartTime = DateTime.Now.AddMonths(-3);
                    EndTime = DateTime.Now;
                    graphSampling = GraphSample.Last4MonthSample;
                    break;
                case PeriodValues.Last_6_Month:
                    StartTime = DateTime.Now.AddYears(-1);
                    EndTime = DateTime.Now;
                    graphSampling = GraphSample.YearSample;
                    break;
                default:
                    break;
            }
        }

        #endregion


    }
}
