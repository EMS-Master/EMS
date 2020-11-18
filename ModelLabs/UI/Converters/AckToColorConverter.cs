using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace UI.Converters
{
    public class AckToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string a = value[0] as string;
            if (a.Contains("Unacknowledged"))
            {
                return new SolidColorBrush(Colors.LightSalmon);
            }
            else
            {
                return new SolidColorBrush(Colors.GreenYellow);
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    }
