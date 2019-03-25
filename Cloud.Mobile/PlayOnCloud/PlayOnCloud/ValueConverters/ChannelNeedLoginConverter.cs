using System;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public class ChannelNeedLoginConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is ChannelEx)
			{
				var channel = value as ChannelEx;
				if (((channel.CredentialsType == ChannelCredentialsType.UsernamePassword) || (channel.CredentialsType == ChannelCredentialsType.UsernamePasswordServiceProvider) || (channel.CredentialsType == ChannelCredentialsType.ZipCode) || (channel.CredentialsType == ChannelCredentialsType.UsernamePasswordPin)) &&
					((channel.LoginInfo == null) || !channel.LoginInfo.HasCredentials))
					return true;
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	public class ChannelHasLoginConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is ChannelEx)
			{
				var channel = value as ChannelEx;
				if ((channel.CredentialsType != ChannelCredentialsType.Anonymous) && (channel.CredentialsType != ChannelCredentialsType.ServiceProvider))
					return true;
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
