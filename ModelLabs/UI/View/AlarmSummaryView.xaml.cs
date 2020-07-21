using CalculationEngineServ;
using CalculationEngineServ.DataBaseModels;
using CalculationEngineService;
using CommonMeas;
using System;
using System.Collections.Generic;
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

namespace UI.View
{
    /// <summary>
    /// Interaction logic for AlarmSummaryView.xaml
    /// </summary>
    public partial class AlarmSummaryView : UserControl
    {
        public AlarmSummaryView()
        {
            InitializeComponent();

            //AlarmSummaryDataGrid.Items.SortDescriptions.Add(
            //                 new System.ComponentModel.SortDescription("TimeStamp", System.ComponentModel.ListSortDirection.Descending));

            //SeveritySearch.Text = "Alarm Type...";
            ComboBox1.Text = "Select Type";

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
                    else if (alRemove.Count == 0)
                    {
                        AlarmSummaryDataGrid.ItemsSource = al;

                    }
                }
                foreach (var a in alRemove)
                {
                    al.Remove(a);
                }
                foreach (var a in alRemove)
                {
                    al.Add(a);
                    AlarmSummaryDataGrid.ItemsSource = al;
                }

            }
        }

        private void ButtonHide_Click(object sender, RoutedEventArgs e)
        {


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
            ComboBox2.SelectedValue = null;
            if (ComboBox1.SelectedValue.ToString().Contains("All Alarms"))
            {
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
            if (ComboBox1.SelectedValue.ToString().Contains("Type Alarm"))
            {
                string str = ComboBox2.SelectedValue.ToString();
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
            else if (ComboBox1.SelectedValue.ToString().Contains("Severity"))
            {
                string str = ComboBox2.SelectedValue.ToString();
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
            else if (ComboBox1.SelectedValue.ToString().Contains("GID"))
            {
                if (ComboBox2.SelectedValue == null)
                {
                    return;
                }
                long gid =(long)ComboBox2.SelectedValue;
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
