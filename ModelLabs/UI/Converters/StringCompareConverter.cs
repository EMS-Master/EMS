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
    public class StringCompareConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values.Length != 2)
            {
                return false;
            }
            string s1 = (string)values[0];
            //string s2 = (string)values[1];

            if (s1.Length > 2)
            {
                s1 = s1.Substring(2);
            }

            return false;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
