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
            //IEnumerable<object> m1 = values[0] as IEnumerable<object>;
            //List<MeasurementUI> m2 = new List<MeasurementUI>();
            //foreach (var item in m1)
            //{
            //    MeasurementUI m = (MeasurementUI)item;
            //    MeasurementUI mm = item as MeasurementUI;

            //    m2.Add(m);
            //}
            IEnumerable<MeasurementUI> measUIs = values[0] as IEnumerable<MeasurementUI>;
            if (measUIs != null)
            {
                return measUIs.LastOrDefault()?.CurrentValue.ToString();
            }

            return "0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
