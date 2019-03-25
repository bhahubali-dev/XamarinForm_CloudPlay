using Xamarin.Forms;
using Android.Support.Design;
using PlayOnCloud.Droid;

[assembly: Dependency(typeof(AppVersionNumber))]
namespace PlayOnCloud.Droid
{
	public class AppVersionNumber : IAppVersionNumber
	{
		public string GetVersion()
		{
			return BuildConfig.VersionName;
		}
	}
}