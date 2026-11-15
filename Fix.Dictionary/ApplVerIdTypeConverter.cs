using System.ComponentModel;

namespace Fix;

public partial class Dictionary
{
    public class ApplVerIdTypeConverter : TypeConverter
    {
        readonly List<Version> _versions = new();

        public ApplVerIdTypeConverter()
        {
            foreach (var version in Versions)
            {
                if (version.BeginString.StartsWith("FIXT."))
                    continue;
                _versions.Add(version);
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
        {
            return new StandardValuesCollection(_versions);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            if (value as string != null)
            {
                foreach (var version in _versions)
                {
                    if (version.BeginString == value as string)
                        return version;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}

