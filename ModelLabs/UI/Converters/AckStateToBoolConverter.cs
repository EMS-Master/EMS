using CommonMeas;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.Converters
{
    public class AckStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (!(value is AckState))
            {
                return null;
            }

            switch ((AckState)value)
            {
                case AckState.Acknowledged:
                    return true;
                case AckState.Unacknowledged:
                    return false;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value.Equals(true))
            {
                return AckState.Acknowledged;
            }

            if (value.Equals(false))
            {
                return AckState.Unacknowledged;
            }

            throw new NotImplementedException();
        
    }
    }
}
