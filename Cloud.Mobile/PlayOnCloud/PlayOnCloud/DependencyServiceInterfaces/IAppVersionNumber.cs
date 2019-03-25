using Xamarin.Forms;

namespace PlayOnCloud
{
	public interface IAppVersionNumber
	{
		string GetVersion();
	}

	public class AppVersionNumberService
	{
		private static volatile IAppVersionNumber instance;
		private static object syncRoot = new object();

		public static IAppVersionNumber Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<IAppVersionNumber>();

				return instance;
			}
		}
	}
}
