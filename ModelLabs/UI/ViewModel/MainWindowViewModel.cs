using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public DockManagerViewModel DockManagerViewModel { get; private set; }

        public MainWindowViewModel()
        {
            var documents = new List<ViewModelBase>();

            documents.Add(new NMSViewModel(new View.NMSView()) { Title = "NMS" });
            documents.Add(new ImporterViewModel() { Title = "Importer" });

            this.DockManagerViewModel = new DockManagerViewModel(documents);
        }
    }
}
