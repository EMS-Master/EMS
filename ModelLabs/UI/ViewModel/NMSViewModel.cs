using FTN.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using UI.View;

namespace UI.ViewModel
{
    public class NMSViewModel: ViewModelBase
    {
        private TestGDA testGda;
        private NMSView NMSview;

        private ObservableCollection<ResourceDescription> resList;
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();

        private ICommand findCommand;

        public NMSViewModel(NMSView mainWindow)
        {
            Title = "NMS";
            ResList = new ObservableCollection<ResourceDescription>();
        }

        public ObservableCollection<ResourceDescription> ResList
        {
            get
            {
                return resList;
            }

            set
            {
                resList = value;
            }
        }

        public ICommand FindCommand => findCommand ?? (findCommand = new RelayCommand<string>(FindCommandExecute));

        private void FindCommandExecute(string textForFind)
        {
            var hex_val = textForFind;
            ResList.Clear();

            List<ModelCode> forFind = getModelCodes();

            //var allSelected = getSelectedProp();
            foreach (var modCode in forFind)
            {
                var myProps = modelResourcesDesc.GetAllPropertyIds(ModelCodeHelper.GetTypeFromModelCode(modCode));
               // var mySelected = myProps.Where(x => allSelected.Contains(x));
                //var retExtentValues = testGda.GetExtentValues(modCode, mySelected.ToList());
                //foreach (var res in retExtentValues)
                //{
                    
                //    ResList.Add(res);
                //}
            }

            if (hex_val.Trim() != string.Empty)
            {
                try
                {
                    long gid = Convert.ToInt64(hex_val, 16);
                    ResourceDescription rd = testGda.GetValues(gid);
                    if (!ResList.ToList().Exists((x) => x.Id == rd.Id))
                    {
                        ResList.Add(rd);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private List<ModelCode> getModelCodes()
        {
            List<ModelCode> retList = new List<ModelCode>();

            foreach (var item in NMSview.TypeContainer.Items)
            {
                ContentPresenter c = (ContentPresenter)NMSview.TypeContainer.ItemContainerGenerator.ContainerFromItem(item);
                CheckBox chbox = c.ContentTemplate.FindName("TypeCheckBox", c) as CheckBox;
                if (chbox.IsChecked == true)
                {
                    var type = (DMSType)chbox.DataContext;

                    //convert an enum to another type of enum
                    //After convert add to list
                    retList.Add((ModelCode)Enum.Parse(typeof(ModelCode), type.ToString()));
                }
            }

            return retList;
        }

        /*private List<ModelCode> getSelectedProp()
        {
            List<ModelCode> retList = new List<ModelCode>();
            foreach (var item in NMSview.PropertiesContainer.Items)
            {
                ContentPresenter c = (ContentPresenter)NMSview.PropertiesContainer.ItemContainerGenerator.ContainerFromItem(item);
                CheckBox chbox = c.ContentTemplate.FindName("PropCheckBox", c) as CheckBox;
                if (chbox.IsChecked == true)
                {
                    var propModelCode = (ModelCode)chbox.DataContext;

                    //convert an enum to another type of enum
                    //After convert add to list
                    retList.Add(propModelCode);

                }
            }

            return retList;
        }*/
    }
}
