using System;
using System.Globalization;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public class ContentItemDurationAirDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ContentItem)
			{
				ContentItem contentItem = value as ContentItem;
				var milliseconds = contentItem.Duration;
				string result = string.Empty;
				if (milliseconds > 0)
				{
					TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
					if (timeSpan.Hours > 0)
						result = string.Format("{0:N0}", timeSpan.Hours) + "h " + timeSpan.Minutes.ToString("D2") + "m";
					else if (timeSpan.Minutes > 0)
						result = string.Format("{0:N0}", timeSpan.Minutes) + "m";
					else
						result = timeSpan.Seconds.ToString("D") + "s";
				}

				if (contentItem.AirDate != DateTime.MinValue)
				{
					if (!string.IsNullOrEmpty(result))
						result += " - ";

					result += contentItem.AirDate.ToString("Air M/yy");
				}

				return result;
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
