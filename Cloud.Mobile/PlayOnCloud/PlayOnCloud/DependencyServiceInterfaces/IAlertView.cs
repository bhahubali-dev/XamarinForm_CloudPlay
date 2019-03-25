using Xamarin.Forms;

namespace PlayOnCloud
{
	public interface IAlertView
	{
		void ShowAlert(string message);
	}

	public class AlertViewService
	{
		private static volatile IAlertView instance;
		private static object syncRoot = new object();

		public static IAlertView Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<IAlertView>();

				return instance;
			}
		}
	}
}
