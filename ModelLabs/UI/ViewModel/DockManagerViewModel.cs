using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.ViewModel
{
    public class DockManagerViewModel
    {
        public ObservableCollection<ViewModelBase> Documents { get; private set; }

        public ObservableCollection<object> Anchorables { get; private set; }

        public DockManagerViewModel(IEnumerable<ViewModelBase> dockWindowViewModels)
        {
            this.Documents = new ObservableCollection<ViewModelBase>();
            this.Anchorables = new ObservableCollection<object>();

            foreach (var document in dockWindowViewModels)
            {
                document.PropertyChanged += DockWindowViewModel_PropertyChanged;
                if (!document.IsClosed)
                    this.Documents.Add(document);
            }
        }

        private void DockWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ViewModelBase document = sender as ViewModelBase;

            if (e.PropertyName == nameof(ViewModelBase.IsClosed))
            {
                if (!document.IsClosed)
                    this.Documents.Add(document);
                else
                    this.Documents.Remove(document);
            }
        }
    }
}
