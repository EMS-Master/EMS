using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UI.ViewModel
{
    public class ImporterViewModel : ViewModelBase
    {
        private ICommand showOpenDialog;
        private ICommand convertCommand;
        private ICommand applyCommand;

        private string convertReport;
        private string applyReport;

        private string fileLocation;

        public string FileLocation
        {
            get { return fileLocation; }
            set { fileLocation = value; OnPropertyChanged(); }
        }

        public string ConvertReport
        {
            get { return convertReport; }
            set { convertReport = value; OnPropertyChanged(); }
        }

        public string ApplyReport
        {
            get { return applyReport; }
            set { applyReport = value; OnPropertyChanged(); }
        }

        public ImporterViewModel()
        {
            Title = "Importer";

        }
        private void ShowOpenDialogCommandExecute(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open CIM Document File..";
            openFileDialog.Filter = "CIM-XML Files|*.xml;*.txt;*.rdf|All Files|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                FileLocation = openFileDialog.FileName;
            }

        }

        public ICommand ShowOpenDialog => showOpenDialog ?? (showOpenDialog = new RelayCommand(ShowOpenDialogCommandExecute));
    }
}
