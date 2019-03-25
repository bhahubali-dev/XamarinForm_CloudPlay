using System;
using Xamarin.Forms;
using Android.OS;
using Android.Webkit;
using Plugin.CurrentActivity;
using PlayOnCloud.Droid;

[assembly: Dependency(typeof(UserAgent))]
namespace PlayOnCloud.Droid
{
	class UserAgent : IUserAgent
	{
		private string userAgent;

		public void Init()
		{
			try
			{
				if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
					userAgent = WebSettings.GetDefaultUserAgent(CrossCurrentActivity.Current.Activity);
				else
					userAgent = new Android.Webkit.WebView(CrossCurrentActivity.Current.Activity).Settings.UserAgentString;
			}
			catch
			{
			}
		}

		public string GetUserAgent()
		{
			if (string.IsNullOrEmpty(userAgent))
				throw new ApplicationException("User agent not initialized");

			return userAgent;
		}
	}
}