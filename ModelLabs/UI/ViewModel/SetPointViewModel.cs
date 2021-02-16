using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UI.Communication;

namespace UI.ViewModel
{
    public class SetPointViewModel:ViewModelBase
    {
        public long globalId { get; private set; }
        private UIScadaCommandClient proxyScada;

        public string newValue { get; set; }
		public float newValueFloat { get; set; }

        public string globalName { get; private set; }

        private ICommand commandGenMessBox;
        public ICommand CommandGenMessBox => commandGenMessBox ?? (commandGenMessBox = new RelayCommand<object>(CommandGenMessBoxExecute));
       
        public SetPointViewModel(long gid , string name)
        {
            globalId = gid;
            globalName = name;
            proxyScada = new UIScadaCommandClient("UIScadaCommandClientEndpoint");

        }

        private void CommandGenMessBoxExecute(object obj)
        {
			float value = 0;
			bool isValidValue = float.TryParse(this.newValue, out value);

			if (isValidValue)
			{
				this.newValueFloat = value;
				if (this.newValueFloat > 1000 || this.newValueFloat < 0)
				{
					MessageBoxResult messageBoxInvalidResult = System.Windows.MessageBox.Show("Commanded value is out of range (0 MW - 1000 MW)", "Command", System.Windows.MessageBoxButton.OK);
				}
				else
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
			}
			else
			{
				MessageBoxResult messageBoxInvalidResult = System.Windows.MessageBox.Show("Value must be a number.", "Command", System.Windows.MessageBoxButton.OK);
			}
        }


        private void CommandGenExecute(object obj)
        {
            //ModelForCheckboxes model = (ModelForCheckboxes)obj;
            //if (model.IsActive)
            //{
                proxyScada.CommandAnalogValues(this.globalId,this.newValueFloat);
            //}
        }
    }
}
