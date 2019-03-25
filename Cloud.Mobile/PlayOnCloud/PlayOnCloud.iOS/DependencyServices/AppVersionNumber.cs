using Xamarin.Forms;
using Foundation;
using PlayOnCloud.iOS;

[assembly: Dependency(typeof(AppVersionNumber))]
namespace PlayOnCloud.iOS
{
	public class AppVersionNumber : IAppVersionNumber
	{
		public string GetVersion()
		{
			return NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
		}
	}
}