using System;
using System.Globalization;
using System.IO;
using Xamarin.Forms;

namespace PlayOnCloud
{
	class ByteArrayToImageSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is byte[])
				return ImageSource.FromStream(() => new MemoryStream(value as byte[]));

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
