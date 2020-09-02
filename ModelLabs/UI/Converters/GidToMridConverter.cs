using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.Converters
{
    public class GidToMridConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
            {
                return "";
            }

            Dictionary<long, IdentifiedObject> nmsModelMap = values[1] as Dictionary<long, IdentifiedObject>;
            long gid = (long)values[0];

            IdentifiedObject idObj = null;
            if (nmsModelMap.TryGetValue(gid, out idObj))
            {
                return idObj.Name;
            }

            return gid;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
