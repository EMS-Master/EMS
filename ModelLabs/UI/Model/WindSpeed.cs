using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UI.Model
{
    public class WindSpeed : INotifyPropertyChanged
    {
        private float speed;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
           
        }
        public string Name { get; set; }
        public float Speed { get { return this.speed; }  set {
                if (value != this.speed)
                {
                    this.speed = value;
                    NotifyPropertyChanged();
                }
            } }

        public WindSpeed()
        {
            Name = "";
            Speed = 0;
        }
        public WindSpeed(string n, float s)
        {
            Name = n;
            Speed = s;
        }
    }
}
