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
    public class DMSTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }

            var type = (DMSType)value;

            switch (type)
            {
                case DMSType.DISCRETE:
                    return "Discrete";
                case DMSType.ANALOG:
                    return "Analog";
                case DMSType.GENERATOR:
                    return "Generator";
                case DMSType.SUBSTATION:
                    return "Substation";
                case DMSType.GEOGRAFICAL_REGION:
                    return "GeograficalRegion";
                case DMSType.BATTERY_STORAGE:
                    return "BatteryStorage";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.ToObject(targetType, value);
        }
    }
}
