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
            AlarmSummaryDataGrid.Items.SortDescriptions.Add(
                                new System.ComponentModel.SortDescription("TimeStamp", System.ComponentModel.ListSortDirection.Descending));

        }

    }

   
}
