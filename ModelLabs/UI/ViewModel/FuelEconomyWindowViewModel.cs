using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using CalculationEngineContracts;
using FTN.Common;
using System.Windows.Input;
using UI.Communication;

namespace UI.ViewModel
{
    public class FuelEconomyWindowViewModel : ViewModelBase
    {
        long globalId;
        public string Name { get; set; }
        private PlotModel model;
        public PlotModel Model { get { return model; } set{  model = value; OnPropertyChanged(); } }
        private ICommand refreshFuelEconomy;
        public ICommand RefreshFuelEconomy => refreshFuelEconomy ?? (refreshFuelEconomy = new RelayCommand(RefreshFuelEconomyCommand));
        private UICalculationEngineClient proxy;
        public FuelEconomyWindowViewModel(long gid, string name, GeneratorType genType)
        {
            globalId = gid;
            Name = name;
            proxy = new UICalculationEngineClient("CalculationEngineUIEndpoint");

            List<float> points = proxy.GetPointForFuelEconomy(gid);
            Model = new PlotModel { Title = "Name: " + name + "\n Type: " + genType.ToString() };
            Model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, Title = "[%]",
                MajorGridlineStyle = LineStyle.Dot, MajorGridlineColor = OxyColors.Gray
            });
            
            Model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left,  Title = "[t/MW]", MajorGridlineStyle = LineStyle.Dot, MajorGridlineColor = OxyColors.Gray });
            var series1 = new OxyPlot.Series.LineSeries
            {
                //InterpolationAlgorithm = InterpolationAlgorithms.ChordalCatmullRomSpline,
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerStroke = OxyColors.Black
            };
            var series2 = new LineSeries
            {
                MarkerType = MarkerType.Cross,
                MarkerSize = 5,
                MarkerStroke = OxyColors.Red,
                //InterpolationAlgorithm = InterpolationAlgorithms.ChordalCatmullRomSpline
            };

            if (genType == GeneratorType.Coal)
            {
                series1.Points.Add(new DataPoint(0, 1.3));
                series1.Points.Add(new DataPoint(15, 1));
                series1.Points.Add(new DataPoint(40, 0.65));
                series1.Points.Add(new DataPoint(80, 0.35));
                series1.Points.Add(new DataPoint(100, 0.25));
                series2.Points.Add(new DataPoint(points[0], points[1]));
                series2.ToolTip = "X = " + points[0] + ", Y = " + points[1];
            }
            else if (genType == GeneratorType.Gas)
            {
                series1.Points.Add(new DataPoint(0, 1.3));
                series1.Points.Add(new DataPoint(10, 1.15));
                series1.Points.Add(new DataPoint(40, 0.75));
                series1.Points.Add(new DataPoint(70, 0.45));
                series1.Points.Add(new DataPoint(100, 0.15));
                series2.Points.Add(new DataPoint(points[0], points[1]));
                series2.ToolTip = "X = " + points[0] + ", Y = " + points[1];
            }
            else if (genType == GeneratorType.Oil)
            {
                series1.Points.Add(new DataPoint(0, 1.3));
                series1.Points.Add(new DataPoint(10, 1.15));
                series1.Points.Add(new DataPoint(30, 0.75));
                series1.Points.Add(new DataPoint(50, 0.40));
                series1.Points.Add(new DataPoint(100, 0.15));
                series2.Points.Add(new DataPoint(points[0], points[1]));
                series2.ToolTip = "X = " + points[0] + ", Y = " + points[1];
            }
            else
            {
                // do nothing
            }
            
            Model.Series.Add(series1);
            Model.Series.Add(series2);
        }

        public void RefreshFuelEconomyCommand(object obj)
        {
            List<float> points = proxy.GetPointForFuelEconomy(globalId);
            var series2 = new LineSeries
            {
                MarkerType = MarkerType.Cross,
                MarkerSize = 5,
                MarkerStroke = OxyColors.Red,
                //InterpolationAlgorithm = InterpolationAlgorithms.ChordalCatmullRomSpline
            };
            
            series2.Points.Add(new DataPoint(points[0], points[1]));
            series2.ToolTip = "X = " + points[0] + ", Y = " + points[1];
            Model.Series.RemoveAt(1);
            OnPropertyChanged("Model");
            Model.Series.Add(series2);
            Model.InvalidatePlot(true);
            OnPropertyChanged("Model");
        }
    }
}
