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
    public class LastValueToBool : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<MeasurementUI> measUIs = values as IEnumerable<MeasurementUI>;
            if (measUIs != null)
            {
                return measUIs.LastOrDefault().IsActive ;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
