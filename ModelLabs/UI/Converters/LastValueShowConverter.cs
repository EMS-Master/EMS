using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.Converters
{
    public class LastValueShowConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<MeasurementUI> measUIs = values[0] as IEnumerable<MeasurementUI>;
            if (measUIs != null)
            {
                return measUIs.LastOrDefault()?.CurrentValue.ToString("0.00") + " [MW]";
            }

            return "0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
