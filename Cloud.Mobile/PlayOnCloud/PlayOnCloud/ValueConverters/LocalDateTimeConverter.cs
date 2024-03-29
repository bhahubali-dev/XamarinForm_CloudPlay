﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class LocalDateTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime)
				return ((DateTime)value).ToLocalTime();

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
