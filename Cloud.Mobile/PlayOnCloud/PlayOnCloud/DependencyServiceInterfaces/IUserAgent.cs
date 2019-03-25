using Xamarin.Forms;

namespace PlayOnCloud
{
	public interface IUserAgent
	{
		void Init();

		string GetUserAgent();
	}

	public class UserAgentService
	{
		private static volatile IUserAgent instance;
		private static object syncRoot = new object();

		public static IUserAgent Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<IUserAgent>();

				return instance;
			}
		}

		public static void Init()
		{
			Instance.Init();
		}
	}
}
