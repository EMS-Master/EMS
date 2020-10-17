using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using FTN.ServiceContracts;

namespace UI.Converters
{
	public class BoolToColorConverter : IMultiValueConverter
	{
		public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            if (value[0] is IEnumerable<MeasurementUI> measUIs)
            {
                return measUIs.LastOrDefault().IsActive ? new SolidColorBrush(Colors.GreenYellow) : new SolidColorBrush(Colors.DarkRed);
            }
            return new SolidColorBrush(Colors.DarkRed);
		}

		public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
