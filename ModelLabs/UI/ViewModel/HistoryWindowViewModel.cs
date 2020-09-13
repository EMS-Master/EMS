using CalculationEngineContracts;
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
    public class HistoryWindowViewModel : ViewModelBase
    {
        long globalId;
        private DateTime startTime;
        private DateTime endTime;
        private PeriodValues selectedPeriod;
        private GraphSample graphSampling;

        private ICommand selectedPeriodCommand;
        public PeriodValues SelectedPeriod { get => selectedPeriod; set => selectedPeriod = value; }
        public GraphSample GraphSampling { get => graphSampling; set => graphSampling = value; }

        public ICommand ChangePeriodCommand => selectedPeriodCommand ?? (selectedPeriodCommand = new RelayCommand(SelectedPeriodCommandExecute));
        private ICommand showDataCommand;

        public ICommand ShowDataCommand => showDataCommand ?? (showDataCommand = new RelayCommand(ShowDataCommandExecute));
        private ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>> generator = new ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>>();
        public ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>> Generator { get => generator; set => generator = value; }

        public HistoryWindowViewModel(long gid)
        {
            globalId = gid;
            startTime = DateTime.Now.AddMinutes(-1);
            endTime = DateTime.Now;
            SelectedPeriod = PeriodValues.None;
        }

        private void ShowDataCommandExecute(object obj)
        {
            ObservableCollection<Tuple<double, DateTime>> tempData;
            ObservableCollection<Tuple<double, DateTime>> measurementsFromDb;
            ObservableCollection<Tuple<double, DateTime>> tempContainer = new ObservableCollection<Tuple<double, DateTime>>();


            Generator.Clear();
            if(startTime == DateTime.MinValue || endTime == DateTime.MinValue)
                measurementsFromDb = new ObservableCollection<Tuple<double, DateTime>>(CalculationEngineUIProxy.Instance.GetHistoryMeasurements(this.globalId, DateTime.Now.AddHours(-1), DateTime.Now));
            else
                measurementsFromDb = new ObservableCollection<Tuple<double, DateTime>>(CalculationEngineUIProxy.Instance.GetHistoryMeasurements(this.globalId, startTime, endTime));


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

                Generator.Add(new KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>(globalId, tempContainer));
            }
            else
            {
                Generator.Add(new KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>(globalId, measurementsFromDb));
            }
        }

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
    }
}

