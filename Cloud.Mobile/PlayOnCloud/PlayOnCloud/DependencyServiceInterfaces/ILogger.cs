using Xamarin.Forms;

namespace PlayOnCloud
{
	public interface ILogger
	{
		string GetLog();

		void Log(string message);
	}

	public class LoggerService
	{
		private static volatile ILogger instance;
		private static object syncRoot = new object();

		public static ILogger Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<ILogger>();

				return instance;
			}
		}
	}
}
