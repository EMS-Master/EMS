using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UI.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        private bool disposed = false;

        protected bool Disposed { get { return disposed; } }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        //private ICommand _CloseCommand;
        //public ICommand CloseCommand
        //{
        //    get
        //    {
        //        if (_CloseCommand == null)
        //            _CloseCommand = new RelayCommand2(call => Close());
        //        return _CloseCommand;
        //    }
        //}
        private bool _IsClosed;
        public bool IsClosed
        {
            get { return _IsClosed; }
            set
            {
                if (_IsClosed != value)
                {
                    _IsClosed = value;
                    OnPropertyChanged(nameof(IsClosed));
                }
            }
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                if (_Title != value)
                {
                    _Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        public ViewModelBase()
        {
           
            this.IsClosed = false;
        }

        public void Close()
        {
            this.IsClosed = true;
        }

        protected virtual void OnDispose()
        {
        }


    }
}
