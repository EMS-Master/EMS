using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.Converters
{
	public class PeriodEnumToStringConverter : IValueConverter
	{
		private string GetEnumDescription(Enum enumObj)
		{
			FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

			object[] attribArray = fieldInfo.GetCustomAttributes(false);

			if (attribArray.Length == 0)
			{
				return enumObj.ToString();
			}
			else
			{
				DescriptionAttribute attrib = null;

				foreach (var att in attribArray)
				{
					if (att is DescriptionAttribute)
						attrib = att as DescriptionAttribute;
				}

				if (attrib != null)
					return attrib.Description;

				return enumObj.ToString();
			}
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
			{
				return "";
			}

			Enum myEnum = (Enum)value;
			string description = GetEnumDescription(myEnum);
			return description;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return Enum.ToObject(targetType, value);
		}
	}
}
