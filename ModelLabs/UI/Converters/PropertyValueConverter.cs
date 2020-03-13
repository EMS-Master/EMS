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
    public class PropertyValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "wrong value";
            }

            var prop = value as Property;

            switch (prop.Type)
            {
                case PropertyType.Float:
                    return prop.AsFloat();
                case PropertyType.Bool:
                case PropertyType.Byte:
                case PropertyType.Int32:
                case PropertyType.Int64:
                case PropertyType.TimeSpan:
                case PropertyType.DateTime:
                    if (prop.Id == ModelCode.IDOBJ)
                    {
                        return String.Format("0x{0:x16}", prop.AsLong());
                    }
                    else
                    {
                        return prop.AsLong();
                    }
                case PropertyType.Enum:
                    try
                    {
                        EnumDescs enumDescs = new EnumDescs();
                        return enumDescs.GetStringFromEnum(prop.Id, prop.AsEnum());
                    }
                    catch (Exception)
                    {
                        return prop.AsEnum();
                    }
                case PropertyType.Reference:
                    return String.Format("0x{0:x16}", prop.AsReference());
                case PropertyType.String:
                    if (prop.PropertyValue.StringValue == null)
                    {
                        prop.PropertyValue.StringValue = String.Empty;
                    }
                    return prop.AsString();
                case PropertyType.Int64Vector:
                case PropertyType.ReferenceVector:
                    if (prop.AsLongs().Count > 0)
                    {
                        var sb = new StringBuilder(100);
                        for (int j = 0; j < prop.AsLongs().Count; j++)
                        {
                            sb.Append(String.Format("0x{0:x16}", prop.AsLongs()[j])).Append(", ");
                        }

                        return sb.ToString(0, sb.Length - 2);
                    }
                    else
                    {
                        return "empty long/reference vector";
                    }
                case PropertyType.TimeSpanVector:
                    if (prop.AsLongs().Count > 0)
                    {
                        var sb = new StringBuilder(100);
                        for (int j = 0; j < prop.AsLongs().Count; j++)
                        {
                            sb.Append(String.Format("0x{0:x16}", prop.AsTimeSpans()[j])).Append(", ");
                        }

                        return sb.ToString(0, sb.Length - 2);
                    }
                    else
                    {
                        return "empty long/reference vector";
                    }
                case PropertyType.Int32Vector:
                    if (prop.AsInts().Count > 0)
                    {
                        var sb = new StringBuilder(100);
                        for (int j = 0; j < prop.AsInts().Count; j++)
                        {
                            sb.Append(String.Format("{0}", prop.AsInts()[j])).Append(", ");
                        }

                        return sb.ToString(0, sb.Length - 2);
                    }
                    else
                    {
                        return "empty int vector";
                    }
                case PropertyType.DateTimeVector:
                    if (prop.AsDateTimes().Count > 0)
                    {
                        var sb = new StringBuilder(100);
                        for (int j = 0; j < prop.AsDateTimes().Count; j++)
                        {
                            sb.Append(String.Format("{0}", prop.AsDateTimes()[j])).Append(", ");
                        }

                        return sb.ToString(0, sb.Length - 2);
                    }
                    else
                    {
                        return "empty DateTime vector";
                    }
                case PropertyType.BoolVector:
                    if (prop.AsBools().Count > 0)
                    {
                        var sb = new StringBuilder(100);
                        for (int j = 0; j < prop.AsBools().Count; j++)
                        {
                            sb.Append(String.Format("{0}", prop.AsBools()[j])).Append(", ");
                        }

                        return sb.ToString(0, sb.Length - 2);
                    }
                    else
                    {
                        return "empty int vector";
                    }
                case PropertyType.FloatVector:
                    if (prop.AsFloats().Count > 0)
                    {
                        var sb = new StringBuilder(100);
                        for (int j = 0; j < prop.AsFloats().Count; j++)
                        {
                            sb.Append(prop.AsFloats()[j]).Append(", ");
                        }

                        return sb.ToString(0, sb.Length - 2);
                    }
                    else
                    {
                        return "empty float vector";
                    }
                case PropertyType.StringVector:
                    if (prop.AsStrings().Count > 0)
                    {
                        var sb = new StringBuilder(100);
                        for (int j = 0; j < prop.AsStrings().Count; j++)
                        {
                            sb.Append(prop.AsStrings()[j]).Append(", ");
                        }

                        return sb.ToString(0, sb.Length - 2);
                    }
                    else
                    {
                        return "empty string vector";
                    }
                case PropertyType.EnumVector:
                    if (prop.AsEnums().Count > 0)
                    {
                        var sb = new StringBuilder(100);
                        EnumDescs enumDescs = new EnumDescs();

                        for (int j = 0; j < prop.AsEnums().Count; j++)
                        {
                            try
                            {
                                sb.Append(String.Format("{0}", enumDescs.GetStringFromEnum(prop.Id, prop.AsEnums()[j]))).Append(", ");
                            }
                            catch (Exception)
                            {
                                sb.Append(String.Format("{0}", prop.AsEnums()[j])).Append(", ");
                            }
                        }

                        return sb.ToString(0, sb.Length - 2);
                    }
                    else
                    {
                        return "empty enum vector";
                    }
                default:
                    return "wrong value";
            }
         }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
