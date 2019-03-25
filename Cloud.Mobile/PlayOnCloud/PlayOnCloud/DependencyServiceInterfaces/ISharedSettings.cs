using Xamarin.Forms;

namespace PlayOnCloud
{
	public interface ISharedSettings
	{
		string GetStringValue(string key);

		bool GetBoolValue(string key);

		void SetStringValue(string key, string value);

		void SetBoolValue(string key, bool value);

		void Save();
	}

	public class SharedSettingsService
	{
		private static volatile ISharedSettings instance;
		private static object syncRoot = new object();

		public static ISharedSettings Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<ISharedSettings>();

				return instance;
			}
		}
	}
}
