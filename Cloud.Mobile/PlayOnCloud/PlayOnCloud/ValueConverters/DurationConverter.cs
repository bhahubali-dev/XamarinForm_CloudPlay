using System;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class DurationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is long)
			{
				var milliseconds = (long)value;
				if (milliseconds > 0)
				{
					var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
					if (timeSpan.Hours > 0)
						return string.Format("{0:N0}", timeSpan.Hours) + "h " + timeSpan.Minutes.ToString("D2") + "m";
					else if (timeSpan.Minutes > 0)
						return string.Format("{0:N0}", timeSpan.Minutes) + "m";

					return timeSpan.Seconds.ToString("D") + "s";
				}
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	public class DurationConverterShort : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is long)
			{
				var milliseconds = (long)value;
				if (milliseconds > 0)
				{
					var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
					if (timeSpan.Hours > 0)
						return string.Format("{0:N0}", timeSpan.Hours) + "h " + timeSpan.Minutes.ToString("D2") + "m";
					else if (timeSpan.Minutes > 0)
						return string.Format("{0:N0}", timeSpan.Minutes) + "m";

					return timeSpan.Seconds.ToString("D") + "s";
				}
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}