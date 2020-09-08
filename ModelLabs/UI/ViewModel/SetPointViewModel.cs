using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UI.ViewModel
{
    public class SetPointViewModel:ViewModelBase
    {
        public long globalId { get; private set; }
        public float newValue { get; set; }

        private ICommand commandGenMessBox;
        public ICommand CommandGenMessBox => commandGenMessBox ?? (commandGenMessBox = new RelayCommand<object>(CommandGenMessBoxExecute));
       
        public SetPointViewModel(long gid)
        {
            globalId = gid;
        }

        private void CommandGenMessBoxExecute(object obj)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to command this element?", "Command", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                CommandGenExecute(obj);
                if (obj != null)
                {
                    var window = (Window)obj;

                    window.Close();
                }
            }
        }


        private void CommandGenExecute(object obj)
        {
            //ModelForCheckboxes model = (ModelForCheckboxes)obj;
            //if (model.IsActive)
            //{
                ScadaCommandingProxy.Instance.CommandAnalogValues(this.globalId,this.newValue);
            //}
        }
    }
}
