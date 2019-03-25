using Xamarin.Forms;
using Android.Content;
using PlayOnCloud.Droid;

[assembly: Dependency(typeof(SharedSettings))]
namespace PlayOnCloud.Droid
{
	public class SharedSettings : ISharedSettings
	{
		private const string prefsName = "PlayOnCloudPrefs";
		private ISharedPreferences prefs;
		private ISharedPreferencesEditor editor;

		public SharedSettings()
		{
			prefs = Android.App.Application.Context.GetSharedPreferences(prefsName, FileCreationMode.Private);
			editor = prefs.Edit();
		}

		public bool GetBoolValue(string key)
		{
			return prefs.GetBoolean(key, false);
		}

		public string GetStringValue(string key)
		{
			return prefs.GetString(key, string.Empty);
		}

		public void SetStringValue(string key, string value)
		{
			editor.PutString(key, value);
		}

		public void SetBoolValue(string key, bool value)
		{
			editor.PutBoolean(key, value);
		}

		public void Save()
		{
			editor.Apply();
		}
	}
}