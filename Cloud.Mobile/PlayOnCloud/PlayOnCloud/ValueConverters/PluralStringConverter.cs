using System;
using System.Globalization;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class PluralStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((value is int) && (parameter is string))
			{
				string result = parameter as string;
				if (!string.IsNullOrEmpty(result))
				{
					result = result.Replace("{0}", value.ToString());
					return result.Replace("[PLURAL_REPLACE]", ((int)value != 1) ? "s" : string.Empty);
				}
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
