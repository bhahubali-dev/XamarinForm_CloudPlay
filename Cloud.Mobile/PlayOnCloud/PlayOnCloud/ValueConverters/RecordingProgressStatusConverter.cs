using System;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public class RecordingProgressStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is RecordQueueItemProgress)
			{
				RecordQueueItemProgress progress = value as RecordQueueItemProgress;
				if ((progress.Status == RecordingStatus.Started) && progress.TimeSinceStarted.HasValue)
					return string.Format("{0} / {1}",
						StringTools.GetTimeString(progress.TimeSinceStarted.Value),
						StringTools.GetTimeString(progress.EstimatedDuration));
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
