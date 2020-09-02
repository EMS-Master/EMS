using CalculationEngineServ;
using CalculationEngineServ.DataBaseModels;
using CalculationEngineService;
using CommonMeas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UI.View
{
    /// <summary>
    /// Interaction logic for AlarmSummaryView.xaml
    /// </summary>
    public partial class AlarmSummaryView : UserControl
    {
        // create a new timer
       public DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();


        public AlarmSummaryView()
        {
            
            InitializeComponent();


            //DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer.Start();
            

            //var timer = new System.Threading.Timer((e) =>
            //{
            //    this.Dispatcher.Invoke(() =>
            //    {
            //        AlarmSummaryDataGrid.Items.Refresh();
            //    });
            //}, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            ComboBox1.Text = "Select Type";

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DisplayData();

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        public void DisplayData()
        {
            using (var db = new EmsContext())
            {
                List<Alarm> al = new List<Alarm>();
                List<Alarm> alRemove = new List<Alarm>();

                foreach (var alarm in db.Alarms)
                {
                    if (alarm.AckState == AckState.Acknowledged)
                    {
                        alRemove.Add(alarm);
                    }
                    else
                    {
                        al.Add(alarm);
                    }
                }
                var alSort = al.OrderByDescending(x => x.AlarmTimeStamp).ToList();
                var alRemoveSort = alRemove.OrderByDescending(x => x.AlarmTimeStamp).ToList();

                foreach (var alarm in alRemoveSort)
                {
                    alSort.Add(alarm);
                }

                this.Dispatcher.Invoke(() =>
                {
                    AlarmSummaryDataGrid.ItemsSource = alSort;
                });

            }
        }

        
        private void ButtonHide_Click(object sender, RoutedEventArgs e)
        {

            dispatcherTimer.Stop();
            
            using (var db = new EmsContext())
            {
                List<Alarm> al = new List<Alarm>();
                List<Alarm> alRemove = new List<Alarm>();

                foreach (var alarm in db.Alarms)
                {
                    al.Add(alarm);
                }

                foreach (var a in al)
                {
                    if (a.AckState == AckState.Acknowledged)
                    {
                        alRemove.Add(a);
                    }
                }

                foreach (var a in alRemove)
                {
                    al.Remove(a);
                    AlarmSummaryDataGrid.ItemsSource = al;

                }
            }


        }


        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dispatcherTimer.Start();
            ComboBox2.SelectedValue = null;
            if (ComboBox1.SelectedValue.ToString().Contains("All Alarms"))
            {
                dispatcherTimer.Start();
                List<Alarm> alarms = new List<Alarm>();
                using (var db = new EmsContext())
                {
                    foreach (var alarm in db.Alarms)
                    {
                        alarms.Add(alarm);
                    }
                    AlarmSummaryDataGrid.ItemsSource = alarms;
                }
            }
            else if (ComboBox1.SelectedValue.ToString().Contains("Type Alarm"))
            {
                ComboBox2.ItemsSource = new List<string>() { "NORMAL", "HIGH", "LOW", "DOM" };
            }
            else if (ComboBox1.SelectedValue.ToString().Contains("Severity"))
            {
                ComboBox2.ItemsSource = new List<string>() { "HIGH", "LOW", "MEDIUM" };
            }
            else if (ComboBox1.SelectedValue.ToString().Contains("GID"))
            {
                using (var db = new EmsContext())
                {
                    var database = db.Alarms.Select(col => col.Gid).ToList();
                    ComboBox2.ItemsSource = database;
                }
            }
        }

        private void ComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dispatcherTimer.Stop();
            if (ComboBox1.SelectedValue.ToString().Contains("Type Alarm"))
            {
                string str = ComboBox2.SelectedValue.ToString();
                if (str.Equals("NORMAL") || str.Equals("HIGH") || str.Equals("LOW") || str.Equals("DOM"))
                {
                    Enum.TryParse(str, out AlarmType alarmType);
                    List<Alarm> alarms = new List<Alarm>();
                    using (var db = new EmsContext())
                    {
                        foreach (var alarm in db.Alarms)
                        {
                            if (alarm.AlarmType == alarmType)
                                alarms.Add(alarm);
                        }
                        AlarmSummaryDataGrid.ItemsSource = alarms;
                    }
                }
            }
            else if (ComboBox1.SelectedValue.ToString().Contains("Severity"))
            {
                string str = ComboBox2.SelectedValue.ToString();
                if (str.Equals("HIGH") || str.Equals("LOW") || str.Equals("MEDIUM"))
                {
                    Enum.TryParse(str, out SeverityLevel severityLevel);
                    List<Alarm> alarms = new List<Alarm>();
                    using (var db = new EmsContext())
                    {
                        foreach (var alarm in db.Alarms)
                        {
                            if (alarm.Severity == severityLevel)
                                alarms.Add(alarm);
                        }
                        AlarmSummaryDataGrid.ItemsSource = alarms;
                    }
                }
            }
            else if (ComboBox1.SelectedValue.ToString().Contains("GID"))
            {
                string str = ComboBox2.SelectedValue.ToString();
                if (!str.Equals("NORMAL") && !str.Equals("LOW") && !str.Equals("MEDIUM") && !str.Equals("DOM") && !str.Equals("HIGH"))
                {
                    long gid = (long)ComboBox2.SelectedValue;
                    List<Alarm> alarms = new List<Alarm>();
                    using (var db = new EmsContext())
                    {
                        foreach (var alarm in db.Alarms)
                        {
                            if (alarm.Gid == gid)
                                alarms.Add(alarm);
                        }
                        AlarmSummaryDataGrid.ItemsSource = alarms;
                    }
                }
            }
        }
    }
}
