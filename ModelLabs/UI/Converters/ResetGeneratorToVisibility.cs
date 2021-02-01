using CalculationEngineServ;
using CommonCloud.AzureStorage;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace UI.Converters
{
    public class ResetGeneratorToVisibility : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is IEnumerable<MeasurementUI> measUIs)
            {
                var generator = measUIs.LastOrDefault();
                if(generator.GeneratorType != FTN.Common.GeneratorType.Wind && generator.GeneratorType != FTN.Common.GeneratorType.Solar && generator.GeneratorType != FTN.Common.GeneratorType.Hydro)
                {

                    //List<CommandedGenerator> gen = new List<CommandedGenerator>();
                    //gen = DbManager.Instance.GetCommandedGenerators().Where(x => x.CommandingFlag && x.CommandingValue != 0).Select(x => x).ToList();
                    //EmsContext e = new EmsContext();
                    bool commandingFlag = AzureTableStorage.GetAllCommandedGenerators("UseDevelopmentStorage=true;","CommandedGenerators").FirstOrDefault(x => x.Gid == generator.Gid).CommandingFlag; //DbManager.Instance.GetCommandedGenerator(generator.Gid).CommandingFlag; //e.CommandedGenerators.FirstOrDefault(x => x.Gid == generator.Gid).CommandingFlag;
                    if (commandingFlag)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
