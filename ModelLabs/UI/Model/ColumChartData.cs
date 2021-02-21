using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UI.Model
{
    public class ColumChartData : INotifyPropertyChanged
    {
        private float production;
        public string Type { get; set; }
        public float Production { get { return this.production; } set {
                if (value != this.production)
                {
                    this.production = value;
                    NotifyPropertyChanged();
                }
            } }

        public ColumChartData()
        {
        }
        public ColumChartData(string n, float s)
        {
            Type = n;
            Production = s;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });

        }
    }
}
