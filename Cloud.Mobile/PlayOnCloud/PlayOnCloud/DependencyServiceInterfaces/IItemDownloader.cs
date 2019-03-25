using System;
using System.ComponentModel;
using System.Threading.Tasks;
using PlayOnCloud.Model;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public interface IItemDownloader
	{
		event EventHandler<AsyncCompletedEventArgs> DownloadComplete;

		event EventHandler<DownloadProgress> DownloadProgress;

		Task<Tuple<bool, string>> Download(DownloadItem item);

		void Cancel(string id);

		string GetDownloaderPath();

		bool CanDownloadItems(out string warningMessage, out string title);

		void CreateSession(string sessionID);
	}

	public class ItemDownloaderService
	{
		private static volatile IItemDownloader instance;
		private static object syncRoot = new object();

		public static IItemDownloader Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<IItemDownloader>();

				return instance;
			}
		}
	}
}
