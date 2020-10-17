using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace UI.ViewModel
{
    public class ImporterViewModel : ViewModelBase
    {
        private ICommand showOpenDialog;
        private ICommand convertCommand;
        private ICommand applyCommand;


        private CIMAdapter adapter = new CIMAdapter();
        private Delta nmsDelta = null;
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
            FileLocation = string.Empty;

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

        private void ApplyCommandExecute(object obj)
        {
            //// APPLY Delta
            if (nmsDelta != null)
            {
                try
                {
                    string log = adapter.ApplyUpdates(nmsDelta);
                    ApplyReport = log;
                    nmsDelta = null;
                    MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                    var mainWindVM = mainWindow.DataContext as MainWindowViewModel;
                    var historyVm = mainWindVM?.DockManagerViewModel.Documents.FirstOrDefault(x => x is HistoryViewModel) as HistoryViewModel;

                    mainWindVM.InitiateIntegrityUpdate();
                    if (historyVm != null)
                    {
                        historyVm.IntegrityUpdateForGenerators();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No data is imported into delta object.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void ConvertCommandExecute(object obj)
        {
            try
            {
                if (FileLocation == string.Empty)
                {
                    MessageBox.Show("Must enter CIM/XML file.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string log;
                nmsDelta = null;
                using (FileStream fs = File.Open(FileLocation, FileMode.Open))
                {
                    nmsDelta = adapter.CreateDelta(fs, SupportedProfiles.PowerTransformer, out log);
                    ConvertReport = log;
                }
                if (nmsDelta != null)
                {
                    //// export delta to file
                    using (XmlTextWriter xmlWriter = new XmlTextWriter(".\\deltaExport.xml", Encoding.UTF8))
                    {
                        xmlWriter.Formatting = Formatting.Indented;
                        nmsDelta.ExportToXml(xmlWriter);
                        xmlWriter.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            FileLocation = string.Empty;
        }

        public ICommand ShowOpenDialog => showOpenDialog ?? (showOpenDialog = new RelayCommand(ShowOpenDialogCommandExecute));

        public ICommand ConvertCommand => convertCommand ?? (convertCommand = new RelayCommand(ConvertCommandExecute, (param) => { return FileLocation != string.Empty; }));

        public ICommand ApplyCommand => applyCommand ?? (applyCommand = new RelayCommand(ApplyCommandExecute, (param) => { return nmsDelta != null; }));
    }

   
}
