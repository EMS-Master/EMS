using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.Converters
{
    public class GeneratorEnable :  IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return null;

            Dictionary<long, bool> gidToBoolMap = values[0] as Dictionary<long, bool>;
            int counter = 0;
            foreach (var item in gidToBoolMap.Values)
            {
                if (item)
                {
                    counter++;
                }
            }

            if (counter >= 3)
            {
                long gid = (long)values[1];
                if (gidToBoolMap[gid])
                    return true;
                return false;
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
