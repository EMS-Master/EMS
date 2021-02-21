using FTN.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace UI.View
{
    /// <summary>
    /// Interaction logic for ReferenceView.xaml
    /// </summary>
    public partial class ReferenceView : Window, IDisposable
    {

        private GDA tgda;
        private ObservableCollection<ResourceDescription> resList;
        private ObservableCollection<ModelCode> avaliableProperties;

        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private long resGlobalId;
        private Property prop;

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

        public ReferenceView(GDA tgda, long resGlobalId, Property prop)
        {

            this.tgda = tgda;
            this.resGlobalId = resGlobalId;
            this.prop = prop;

            Association ass = new Association();
            ass.PropertyId = prop.Id;
            //DMSType type = getPropertyModelCode();
            var list = tgda.GetRelatedValues(resGlobalId, ass);

            ResList = new ObservableCollection<ResourceDescription>(list);
            AvaliableProperties = new ObservableCollection<ModelCode>();
            DataContext = this;

            InitializeComponent();

        }

        private DMSType getPropertyModelCode()
        {
            DMSType dmsType = 0;
            long referenceGid = 0;
            if (prop.Type == PropertyType.Reference)
            {
                referenceGid = prop.AsReference();
            }
            else if (prop.Type == PropertyType.ReferenceVector)
            {
                var listRef = prop.AsReferences();
                if (listRef.Count > 0)
                    referenceGid = listRef[0];
            }
            if (referenceGid != 0)
            {
                dmsType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(referenceGid);

            }

            return dmsType;

        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as Grid;
            var property = grid.DataContext as Property;
            var resDesc = grid.Tag as ResourceDescription;

            List<ResourceDescription> refResList = new List<ResourceDescription>();

            ReferenceView RefView = new ReferenceView(tgda, resDesc.Id, property);
            RefView.Visibility = Visibility.Visible;
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            var b = borderHelper;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Results"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Extensible Markup Language (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                using (XmlTextWriter xmlWriter = new XmlTextWriter(filename, Encoding.Unicode))
                {
                    xmlWriter.Formatting = Formatting.Indented;
                    foreach (var rd in ResList)
                    {
                        rd.ExportToXml(xmlWriter);
                    }
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
