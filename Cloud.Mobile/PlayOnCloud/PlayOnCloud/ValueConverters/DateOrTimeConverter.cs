using System;
using System.Globalization;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class DateOrTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime)
			{
				DateTime dateTime = ((DateTime)value).ToLocalTime();
				if (dateTime.Date == DateTime.Today)
					return dateTime.ToString("h:mm tt");

				return dateTime.ToString("MM\\/dd\\/yy");
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
