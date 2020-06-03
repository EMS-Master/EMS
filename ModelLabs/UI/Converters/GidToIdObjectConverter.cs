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
	public class GidToIdObjectConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			//values[0] - Gid
			//values[1] - NMSModelMap

			if (values.Length < 2)
			{
				return null;
			}

			Dictionary<long, IdentifiedObject> nmsModelMap = values[1] as Dictionary<long, IdentifiedObject>;
			long gid = (long)values[0];

			IdentifiedObject idObj = null;
			if (nmsModelMap.TryGetValue(gid, out idObj))
			{
				return idObj;
			}

			return null;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
