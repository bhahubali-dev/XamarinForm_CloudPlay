using System;
using System.Globalization;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class AirDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string)
			{
				var dateValue = DateTime.MinValue;
				if (DateTime.TryParse(value as string, out dateValue) && (dateValue != DateTime.MinValue))
					return dateValue.ToString("MM\\/dd\\/yyyy");
			}

			if ((value is DateTime) && ((DateTime)value != DateTime.MinValue))
				return ((DateTime)value).ToString("MM\\/dd\\/yyyy");

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
