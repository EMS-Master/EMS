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
    public class GeneratorTypeToVIsibility : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<MeasurementUI> measUIs = values as IEnumerable<MeasurementUI>;
            if (measUIs != null)
            {
                var genType = measUIs.LastOrDefault().GeneratorType;
                if (genType == FTN.Common.GeneratorType.Wind || genType == FTN.Common.GeneratorType.Solar)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
