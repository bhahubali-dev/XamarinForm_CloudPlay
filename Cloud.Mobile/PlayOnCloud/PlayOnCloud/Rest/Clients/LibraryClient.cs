using System.Threading.Tasks;
using PlayOnCloud.Model;
using System;

namespace PlayOnCloud
{
	public class LibraryClient
	{
		public static Task<DownloadItem> GetDownload(string id)
		{
			return RestService.Instance.MakeRecorderAPIRestRequest<DownloadItem>("library/" + id + "/download", RequestMethod.GET, null);
		}

		public static Task<LibraryItemsCollection> Get()
		{
			return RestService.Instance.MakeRecorderAPIRestRequest<LibraryItemsCollection>("library", RequestMethod.GET, null);
		}

		public static Task<LibraryItem> Get(string id)
		{
			return RestService.Instance.MakeRecorderAPIRestRequest<LibraryItem>("library/" + id, RequestMethod.GET, null);
		}

		public static Task<RestRequestResponse> Delete(string id)
		{
			return RestService.Instance.MakeRecorderAPIRestRequest("library/" + id, RequestMethod.DELETE, null);
		}

		public static Task<byte[]> GetSmallImageFromUrl(string url)
		{
			return RestService.Instance.GetDataAsync(new Uri(new Uri(url), "small"));
		}

		public static Task<byte[]> GetLargeImageFromUrl(string url)
		{
			return RestService.Instance.GetDataAsync(new Uri(new Uri(url), "large"));
		}
	}
}
