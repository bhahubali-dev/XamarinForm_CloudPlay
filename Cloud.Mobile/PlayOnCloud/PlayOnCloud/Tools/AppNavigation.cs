using System;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public class AppNavigationArgs
	{
		public CloudItem Page { get; set; }

		public string ItemID { get; set; }

		public string ProviderID { get; set; }

		public string Token { get; set; }
	}

	public class AppNavigation
	{
		public static event EventHandler<AppNavigationArgs> AppNavigate;

		public static void FireAppNavigate(AppNavigationArgs args)
		{
			AppNavigate?.Invoke(null, args);
		}
	}
}
