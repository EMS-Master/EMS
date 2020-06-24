using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.Converters
{
    public class GidMapToVisibilityConverter : IMultiValueConverter
    {
        private BoolToVisibilityConverter boolToVisibilityConverter;

        public GidMapToVisibilityConverter()
        {
            boolToVisibilityConverter = new BoolToVisibilityConverter();
        }
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return null;

            Dictionary<long, bool> gidToBoolMap = values[0] as Dictionary<long, bool>;
            long gid = (long)values[1];

            return boolToVisibilityConverter.Convert(gidToBoolMap[gid], null, null, null);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
