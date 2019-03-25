using System;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class ExpirationTimeRemainingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is DateTime)
			{
				var expirationDate = (DateTime)value;
				var timeRemaining = expirationDate.ToLocalTime().Subtract(DateTime.Now);
				return StringTools.GetTimeStringFromMinutes((long)timeRemaining.TotalMinutes);
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	public class ExpirationDaysRemainingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is DateTime)
			{
				var expirationDate = (DateTime)value;
				var timeRemaining = expirationDate.ToLocalTime().Subtract(DateTime.Now);
				long minutes = (long)timeRemaining.TotalMinutes;
				if (timeRemaining.TotalDays > 1)
					minutes = (long)(Math.Round(timeRemaining.TotalDays) * 24 * 60);

				return StringTools.GetTimeStringFromMinutes(minutes);
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
