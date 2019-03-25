using System;
using System.Globalization;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class ChannelsLongNameConverter : IValueConverter
	{
		private static char[] splitChars = new char[] { ' ' };

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				string channelName = value.ToString();
				if (!string.IsNullOrWhiteSpace(channelName) && (channelName.Length > 14))
				{
					var nameSplitted = channelName.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
					if ((nameSplitted != null) && (nameSplitted.Length > 0))
						return nameSplitted[0];
				}
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
