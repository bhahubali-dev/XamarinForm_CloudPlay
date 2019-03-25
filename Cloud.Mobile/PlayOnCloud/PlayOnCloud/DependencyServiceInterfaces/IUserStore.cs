using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public interface IUserStore
	{
		User LoadUserCredentials();

		bool SaveUserCredentials(User user);

		bool DeleteUserCredentials();

		bool StoreRecordInKeychain(string key, string value);

		string GetRecordFromKeychain(string key);
	}

	public class UserStoreService
	{
		private static volatile IUserStore instance;
		private static object syncRoot = new object();

		public static IUserStore Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<IUserStore>();

				return instance;
			}
		}
	}
}
