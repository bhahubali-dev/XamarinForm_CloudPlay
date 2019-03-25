using System;
using Xamarin.Forms;
using UIKit;
using PlayOnCloud.iOS;

[assembly: Dependency(typeof(UserAgent))]
namespace PlayOnCloud.iOS
{
	class UserAgent : IUserAgent
	{
		private string userAgent = string.Empty;

		public void Init()
		{
			try
			{
				using (var agentWebView = new UIWebView())
					userAgent = agentWebView.EvaluateJavascript("navigator.userAgent");
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
