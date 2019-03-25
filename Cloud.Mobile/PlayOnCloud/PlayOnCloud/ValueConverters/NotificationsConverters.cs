using System;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public class IsFailedNotificationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is NotificationType)
			{
				NotificationType type = (NotificationType)value;
				return (type == NotificationType.FailedRecording) || (type == NotificationType.BrowsingError) || (type == NotificationType.RecordingIssue);
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	public class IsSuccessNotificationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is NotificationType)
			{
				NotificationType type = (NotificationType)value;
				return type == NotificationType.NewRecording;
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
