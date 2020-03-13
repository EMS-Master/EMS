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
    public class GlobalIdToEMSTypeStringConverter : IValueConverter
    {
        DMSTypeToStringConverter converter;

        public GlobalIdToEMSTypeStringConverter()
        {
            converter = new DMSTypeToStringConverter();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId((long)value);

            return converter.Convert(type, null, null, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
