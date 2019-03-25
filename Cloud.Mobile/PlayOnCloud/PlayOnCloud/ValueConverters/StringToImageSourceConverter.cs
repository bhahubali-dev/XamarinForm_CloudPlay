using System;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class StringToImageSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is string)
				return ImageSource.FromResource(value as string);

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
