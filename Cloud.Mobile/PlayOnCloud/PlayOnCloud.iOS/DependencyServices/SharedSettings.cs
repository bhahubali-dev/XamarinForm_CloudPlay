using Xamarin.Forms;
using Foundation;
using PlayOnCloud.iOS;

[assembly: Dependency(typeof(SharedSettings))]
namespace PlayOnCloud.iOS
{
	public class SharedSettings : ISharedSettings
	{
		public bool GetBoolValue(string key)
		{
			return NSUserDefaults.StandardUserDefaults.BoolForKey(key);
		}

		public string GetStringValue(string key)
		{
			return NSUserDefaults.StandardUserDefaults.StringForKey(key);
		}

		public void SetStringValue(string key, string value)
		{
			NSUserDefaults.StandardUserDefaults.SetString(value, key);
		}

		public void SetBoolValue(string key, bool value)
		{
			NSUserDefaults.StandardUserDefaults.SetBool(value, key);
		}

		public void Save()
		{
			NSUserDefaults.StandardUserDefaults.Synchronize();
		}
	}
}
