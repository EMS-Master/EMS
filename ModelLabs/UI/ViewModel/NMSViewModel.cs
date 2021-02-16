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

        private ObservableCollection<ResourceDescription> resList = new ObservableCollection<ResourceDescription>();
        private ObservableCollection<ModelCode> avaliableProperties;

        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        //private RelayCommand goToReferenceCommand;

        private ICommand findCommand;
        private ICommand typeCheckBoxChangedCommand;

        private Dictionary<ModelCode, List<ModelCode>> propertyMap = new Dictionary<ModelCode, List<ModelCode>>();

        public NMSViewModel(NMSView mainWindow)
        {
            Title = "NMS";
            this.NMSview = mainWindow;
            this.NMSview.Loaded += View_Loaded;
           // ResList = new ObservableCollection<ResourceDescription>();
            AvaliableProperties = new ObservableCollection<ModelCode>();
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
        public ObservableCollection<ModelCode> AvaliableProperties
        {
            get
            {
                return avaliableProperties;
            }

            set
            {
                avaliableProperties = value;
            }
        }

        public ICommand FindCommand => findCommand ?? (findCommand = new RelayCommand<string>(FindCommandExecute));
        //public ICommand GoToReferenceCommand => goToReferenceCommand ?? (goToReferenceCommand = new RelayCommand(GoToReferenceCommandExecute));
        public ICommand TypeCheckBoxChangedCommand => typeCheckBoxChangedCommand ?? (typeCheckBoxChangedCommand = new RelayCommand(TypeCheckBoxChangedCommandExecute));

        //private void GoToReferenceCommandExecute(object obj)
        //{
        //    var grid = obj as Grid;
        //    var property = grid.DataContext as Property;
        //    var resDesc = grid.Tag as ResourceDescription;

        //    List<ResourceDescription> refResList = new List<ResourceDescription>();

        //    //ReferenceView RefView = new ReferenceView(testGda, resDesc.Id, property);
        //    //RefView.Visibility = System.Windows.Visibility.Visible;
        //}

        private void View_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ModelResourcesDesc resDesc = new ModelResourcesDesc();

            string message = string.Format("Network Model Service Test Client is up and running...");
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

            message = string.Format("Result directory: {0}", Config.Instance.ResultDirecotry);
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

            testGda = new TestGDA();
        }

        private void FindCommandExecute(string textForFind)
        {
            var hex_val = textForFind;
            ResList.Clear();
            
            List<ModelCode> forFind = getModelCodes();

            var allSelected  = getSelectedProp();
            foreach (var modCode in forFind)
            {
                var myProps = modelResourcesDesc.GetAllPropertyIds(ModelCodeHelper.GetTypeFromModelCode(modCode));
                var mySelected = myProps.Where(x => allSelected.Contains(x));
                var retExtentValues = testGda.GetExtentValues(modCode, mySelected.ToList());
                foreach (var res in retExtentValues)
                {
                    
                    ResList.Add(res);
                }
            }

            //if (hex_val.Trim() != string.Empty)
            //{
            //    try
            //    {
            //        long gid = Convert.ToInt64(hex_val, 16);
            //        ResourceDescription rd = testGda.GetValues(gid);
            //        if (!ResList.ToList().Exists((x) => x.Id == rd.Id))
            //        {
            //            ResList.Add(rd);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //    }
            //}
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

        private void TypeCheckBoxChangedCommandExecute(object obj)
        {
            UpdatePropertyFilter();
        }

        private void UpdatePropertyFilter()
        {
            List<ModelCode> selectedModelCodes = getModelCodes();
            List<ModelCode> properties;
            AvaliableProperties.Clear();
            foreach (var modelCode in selectedModelCodes)
            {
                properties = modelResourcesDesc.GetAllPropertyIds(modelCode);

                foreach (var prop in properties)
                {
                    if (!AvaliableProperties.Contains(prop))
                    {
                        AvaliableProperties.Add(prop);
                    }
                }

            }
        }


        public override void Dispose()
        {
            NMSview.Loaded -= View_Loaded;
            base.Dispose();
            
        }

         private List<ModelCode> getSelectedProp()
          {
              List<ModelCode> retList = new List<ModelCode>();
              foreach (var item in NMSview.PropertiesContainer.Items)
              {
                  var c = (ContentPresenter)NMSview.PropertiesContainer.ItemContainerGenerator.ContainerFromItem(item);
                  var chbox = c.ContentTemplate.FindName("PropCheckBox", c) as CheckBox;
                  if (chbox.IsChecked == true)
                  {
                      var propModelCode = (ModelCode)chbox.DataContext;
                    
                      retList.Add(propModelCode);

                  }
              }

              return retList;
          }
    }
}
