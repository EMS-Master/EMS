using CalculationEngineContracts;
using FTN.Common;
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

        private ICommand showDataCommand;

        private ICommand totalProductionGraphCheckedCommand;
        private ICommand totalProductionGraphUnCheckedCommand;
        private ICommand productionForSelectedCheckedCommand;
        private ICommand productionForSelectedUnCheckedCommand;

        private ICommand totalLoadGraphCheckedCommand;
        private ICommand totalLoadGraphUnCheckedCommand;
        private ICommand loadForSelectedCheckedCommand;
        private ICommand loadForSelectedUnCheckedCommand;

        private bool isExpandedSeparated = false;
        private bool isExpandedTotalProduction = false;


        private bool totalProductionForSelectedVisible = false;
        private bool totalProductionGraphVisible = true;

        private bool totalLoadForSelectedVisible = false;
        private bool totalLoadGraphVisible = true;

        private ObservableCollection<long> generatorsFromNms = new ObservableCollection<long>();
        private ObservableCollection<long> batteryStoragesFromNms = new ObservableCollection<long>();

        private ObservableCollection<Tuple<double, DateTime>> totalProduction = new ObservableCollection<Tuple<double, DateTime>>();
        private ObservableCollection<Tuple<double, DateTime>> graphTotalProduction = new ObservableCollection<Tuple<double, DateTime>>();
        private ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>> generatorsContainer = new ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>>();
        private ObservableCollection<Tuple<double, DateTime>> graphTotalProductionForSelected = new ObservableCollection<Tuple<double, DateTime>>();
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
        public ObservableCollection<Tuple<double, DateTime>> TotalProduction { get => totalProduction; set => totalProduction = value; }
        public ObservableCollection<Tuple<double, DateTime>> GraphTotalProductionForSelected { get => graphTotalProductionForSelected; set => graphTotalProductionForSelected = value; }

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

            ObservableCollection<Tuple<double, DateTime>> measurementsFromDb;
            ObservableCollection<Tuple<double, DateTime>> tempData;
            ObservableCollection<Tuple<double, DateTime>> tempContainer = new ObservableCollection<Tuple<double, DateTime>>();


            GraphTotalProduction.Clear();
            GeneratorsContainer.Clear();
            GraphTotalProductionForSelected.Clear();

			GidToBoolMap.Add(12884901889, true);
			GidToBoolMap.Add(12884901891, true);
			GidToBoolMap.Add(12884901892, true);
			GidToBoolMap.Add(12884901893, true);
			GidToBoolMap.Add(12884901894, true);
			GidToBoolMap.Add(12884901895, true);
			GidToBoolMap.Add(12884901896, true);
			GidToBoolMap.Add(12884901897, true);

			foreach (KeyValuePair<long, bool> keyPair in GidToBoolMap)
            {
                if (keyPair.Value == true)
                {
                    try
                    {
                        measurementsFromDb = new ObservableCollection<Tuple<double, DateTime>>(CalculationEngineUIProxy.Instance.GetHistoryMeasurements(keyPair.Key, startTime, endTime));

                        if (measurementsFromDb == null)
                        {
                            continue;
                        }

                        if (graphSampling != GraphSample.None)
                        {
                            DateTime tempStartTime = startTime;
                            DateTime tempEndTime = IncrementTime(tempStartTime);

                            double averageProduction = 0;

                            while (tempEndTime <= endTime)
                            {
                                tempData = new ObservableCollection<Tuple<double, DateTime>>(measurementsFromDb.Where(x => x.Item2 > tempStartTime && x.Item2 < tempEndTime));
                                if (tempData != null && tempData.Count != 0)
                                {
                                    averageProduction = tempData.Average(x => x.Item1);
                                }
                                else
                                {
                                    averageProduction = 0;
                                }

                                tempStartTime = IncrementTime(tempStartTime);
                                tempEndTime = IncrementTime(tempEndTime);

                                tempContainer.Add(new Tuple<double, DateTime>(averageProduction, tempStartTime));
                            }
                            GeneratorsContainer.Add(new KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>(keyPair.Key, new ObservableCollection<Tuple<double, DateTime>>(tempContainer)));
                        }
                        else
                        {
                            GeneratorsContainer.Add(new KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>(keyPair.Key, new ObservableCollection<Tuple<double, DateTime>>(measurementsFromDb)));
                        }

                        measurementsFromDb.Clear();
                        measurementsFromDb = null;
                        tempContainer.Clear();
                    }
                    catch (Exception ex)
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceError, "[HistoryViewModel] Error ShowDataCommandExecute {0}", ex.Message);
                    }
                }
            }

            List<Tuple<double, DateTime>> allProductionValues = new List<Tuple<double, DateTime>>();
            List<DateTime> timestamps = new List<DateTime>();

            foreach (var keyPair in GeneratorsContainer)
            {
                allProductionValues.AddRange(keyPair.Value.ToList());
            }

            foreach (Tuple<double, DateTime> tuple in allProductionValues)
            {
                timestamps.Add(tuple.Item2);
            }
            timestamps = timestamps.Distinct().ToList();

            foreach (DateTime measTime in timestamps)
            {
                double production = 0;
                List<Tuple<double, DateTime>> tuples = allProductionValues.Where(x => x.Item2 == measTime).ToList();
                if (tuples != null)
                {
                    production = tuples.Sum(x => x.Item1);
                }
                tuples = null;
                GraphTotalProductionForSelected.Add(new Tuple<double, DateTime>(production, measTime));
            }

            //TotalProduction = new ObservableCollection<Tuple<double, DateTime>>(CalculationEngineUIProxy.Instance.GetTotalProduction(StartTime, EndTime));
            GraphTotalProduction = new ObservableCollection<Tuple<double, DateTime>>();

            if (graphSampling != GraphSample.None)
            {
                DateTime tempStartTime = startTime;
                DateTime tempEndTime = IncrementTime(tempStartTime);

                double averageProduction;

                while (tempEndTime <= endTime)
                {
                    tempData = new ObservableCollection<Tuple<double, DateTime>>(GraphTotalProductionForSelected.Where(x => x.Item2 >= tempStartTime && x.Item2 < tempEndTime));
                    if (tempData != null && tempData.Count != 0)
                    {
                        averageProduction = tempData.Average(x => x.Item1);
                    }
                    else
                    {
                        averageProduction = 0;
                    }

                    tempStartTime = IncrementTime(tempStartTime);
                    tempEndTime = IncrementTime(tempEndTime);
                    GraphTotalProduction.Add(new Tuple<double, DateTime>(averageProduction, tempStartTime));
                }
            }
            else
            {
                GraphTotalProduction = TotalProduction;
            }
            IsExpandedSeparated = true;
            IsExpandedTotalProduction = true;

            OnPropertyChanged(nameof(GraphTotalProductionForSelected));
            OnPropertyChanged(nameof(GraphTotalProduction));
            OnPropertyChanged(nameof(GeneratorsContainer));

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


        private DateTime IncrementTime(DateTime pointTime)
        {
            switch (graphSampling)
            {
                case GraphSample.HourSample:
                    pointTime = pointTime.AddMinutes(5);
                    return pointTime;
                case GraphSample.TodaySample:
                    pointTime = pointTime.AddHours(1);
                    return pointTime;
                case GraphSample.YearSample:
                    pointTime = pointTime.AddMonths(1);
                    return pointTime;
                case GraphSample.LastMonthSample:
                    pointTime = pointTime.AddDays(1);
                    return pointTime;
                case GraphSample.Last4MonthSample:
                    pointTime = pointTime.AddDays(7);
                    return pointTime;
                default:
                    return pointTime;
            }
        }

        #endregion


    }
}
