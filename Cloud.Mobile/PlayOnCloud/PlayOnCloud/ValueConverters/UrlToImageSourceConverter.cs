using System;
using Xamarin.Forms;

namespace PlayOnCloud
{
	class UrlToImageSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is string)
			{
				return new UriImageSource()
				{
					CachingEnabled = false,
					Uri = new Uri(value as string)
				};
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
