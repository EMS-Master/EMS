using CalculationEngineServ;
using CalculationEngineServ.DataBaseModels;
using CalculationEngineService;
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

            using (var db = new EmsContext())
            {
                List<Alarm> al = new List<Alarm>();

                foreach (var alarm in db.Alarms)
                {
                    al.Add(alarm);
                    AlarmSummaryDataGrid.ItemsSource = al;
                    //Console.WriteLine(alarm.Gid);
                }

            }
        }
    }
}
