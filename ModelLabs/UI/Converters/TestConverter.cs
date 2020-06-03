using FTN.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.Converters
{
	public class TestConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
			//Dictionary<long, ObservableCollection<MeasurementUI>> dictionary = value as Dictionary<long, ObservableCollection<MeasurementUI>>;
			//if(dictionary == null || dictionary.Count == 0)
			//{
			//    return null;
			//}
			//return dictionary.ToList()[0].Value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
