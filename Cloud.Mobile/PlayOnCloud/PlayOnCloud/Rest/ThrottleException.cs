using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayOnCloud
{
	public enum ThrottleExceptionType
	{
		Login,
		Signup,
		RestPassword
	}

	public class ThrottleException : Exception
	{
		public ThrottleException()
		{
		}

		public ThrottleException(string url, int minutes) : base(formatMessage(url, minutes))
		{
		}

		public ThrottleException(ThrottleExceptionType type, int minutes) : base(formatMessage(type, minutes))
		{
		}

		private static string formatMessage(ThrottleExceptionType type, int minutes)
		{
			var minutesPlural = "minutes";
			if (minutes < 2)
				minutesPlural = "minute";

			switch (type)
			{
				case ThrottleExceptionType.Login:
					return string.Format("You've exceeded the maximum login requests. Please wait {0} " + minutesPlural + " and then try again. If you didn't attempt multiple logins, please send an email to support@playon.tv", minutes);

				case ThrottleExceptionType.RestPassword:
					return string.Format("You've exceeded the maximum reset requests. Please wait {0} " + minutesPlural + " and then try again. If you didn't attempt multiple password resets, please send an email to support@playon.tv", minutes);

				case ThrottleExceptionType.Signup:
					return string.Format("You've exceeded the maximum signup requests. Please wait {0} " + minutesPlural + " and then try again. If you didn't attempt multiple signups, please send an email to support@playon.tv", minutes);
			}

			return null;
		}

		private static string formatMessage(string url, int minutes)
		{
			switch (url)
			{
				case PasswordClient.ResetUrl:
					return formatMessage(ThrottleExceptionType.RestPassword, minutes);

				case SignupClient.SignupUrl:
				case SignupClient.FBSignupUrl:
					return formatMessage(ThrottleExceptionType.Signup, minutes);

				case LoginClient.LoginUrl:
				case LoginClient.LoginWithFBUrl:
				case LoginClient.LoginWithTokenUrl:
					return formatMessage(ThrottleExceptionType.Login, minutes);
			}

			return null;
		}
	}
}
