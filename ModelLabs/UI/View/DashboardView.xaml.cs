using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void MenuItemHistory_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            var dataCOntext = menu.DataContext;
            var v = (KeyValuePair<long, ObservableCollection<FTN.ServiceContracts.MeasurementUI>>)dataCOntext;
            long gid = v.Key;

            HistoryWindow historyWindow = new HistoryWindow(gid);
            historyWindow.Show();
        }

        private void MenuItemSetPoint_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            var dataCOntext = menu.DataContext;
            var v = (KeyValuePair<long, ObservableCollection<FTN.ServiceContracts.MeasurementUI>>)dataCOntext;
            long gid = v.Key;
            string name = v.Value[0].Name;

            SetPointWindow setPointWindow = new SetPointWindow(gid, name);
            setPointWindow.Show();
        }

        private void MenuItemFuelEconomy_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            var dataCOntext = menu.DataContext;
            var v = (KeyValuePair<long, ObservableCollection<FTN.ServiceContracts.MeasurementUI>>)dataCOntext;
            long gid = v.Key;
            string name = v.Value[0].Name;

            FuelEconomyWindow fuelEconomyWindow = new FuelEconomyWindow(gid);
            fuelEconomyWindow.Show();

        }
    }
}
