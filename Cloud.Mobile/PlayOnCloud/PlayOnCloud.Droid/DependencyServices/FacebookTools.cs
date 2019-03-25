using System;
using Xamarin.Forms;
using PlayOnCloud.Droid;

[assembly: Dependency(typeof(FacebookTools))]
namespace PlayOnCloud.Droid
{
	public class FacebookTools : IFacebookTools
	{
		public void ActivateApp()
		{
			//throw new NotImplementedException();
		}

		public void LogCustomEvent(string eventName)
		{
			throw new NotImplementedException();
		}

		public void LogPurchase()
		{
			throw new NotImplementedException();
		}
	}
}