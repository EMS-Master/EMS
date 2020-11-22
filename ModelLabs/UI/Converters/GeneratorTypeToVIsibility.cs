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
    public class GeneratorTypeToVIsibility : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<MeasurementUI> measUIs = values[0] as IEnumerable<MeasurementUI>;
            if (measUIs != null)
            {
                var gen = measUIs.LastOrDefault();
                if (gen.IsActive)
                {
                    var genType = gen.GeneratorType;
                    if (genType == FTN.Common.GeneratorType.Wind || genType == FTN.Common.GeneratorType.Solar || genType == FTN.Common.GeneratorType.Hydro)
                        return Visibility.Collapsed;
                    else
                        return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
                
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
