using Xamarin.Forms;
using Facebook.CoreKit;
using PlayOnCloud.iOS;

[assembly: Dependency(typeof(FacebookTools))]
namespace PlayOnCloud.iOS
{
	public class FacebookTools : IFacebookTools
	{
		public void ActivateApp()
		{
			AppEvents.ActivateApp();
		}

		public void LogPurchase()
		{
			AppEvents.LogPurchase(1.0, "USD");
		}

		public void LogCustomEvent(string eventName)
		{
			AppEvents.LogEvent(eventName);
		}
	}
}