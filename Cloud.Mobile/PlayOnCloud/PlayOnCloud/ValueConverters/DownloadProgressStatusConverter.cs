using System;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public class DownloadProgressStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is DownloadProgress)
			{
				DownloadProgress progress = value as DownloadProgress;
				if (progress.Status == DownloadStatus.Downloading)
				{
					//XXX: wait 3 sec before calculating the estimated time left
					if (progress.DownloadedDuration > 3000)
						return string.Format("{0} remaining - {1}/{2}",
							StringTools.GetTimeString(progress.ExpectedDuration - progress.DownloadedDuration),
							StringTools.GetFileSizeString(progress.DownloadedLength),
							StringTools.GetFileSizeString(progress.TotalLength));
					else
						return string.Format("{0}/{1}",
							StringTools.GetFileSizeString(progress.DownloadedLength),
							StringTools.GetFileSizeString(progress.TotalLength));
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
