using System.Net.Http;
using System.Threading.Tasks;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public static class NotificationClient
	{
		public static Task<RestRequestResponse> AddDevice(string type, string token)
		{
			var content = new MultipartFormDataContent
			{
				{ new StringContent(type), "type" },
				{ new StringContent(token), "token" },
			};

			return RestService.Instance.MakeRecorderAPIRestRequest("notification/device", RequestMethod.POST, content);
		}

		public static Task<RestRequestResponse> RemoveDevice(string type, string token)
		{
			return RestService.Instance.MakeRecorderAPIRestRequest("notification/device/" + type + "/" + token, RequestMethod.DELETE, null);
		}

		public static Task<Notification> Get(string nid, string tokenType, string tokenValue)
		{
			string tokenPart = !string.IsNullOrEmpty(tokenType) && !string.IsNullOrEmpty(tokenValue) ? "/" + tokenType + "/" + tokenValue : string.Empty;
			return RestService.Instance.MakeRecorderAPIRestRequest<Notification>("notification/" + nid + tokenPart, RequestMethod.GET, null);
		}

		public static Task<Notification> Get(string id)
		{
			return RestService.Instance.MakeRecorderAPIRestRequest<Notification>("notification/" + id, RequestMethod.GET, null);
		}

		public static Task<NotificationsCollection> Get()
		{
			return RestService.Instance.MakeRecorderAPIRestRequest<NotificationsCollection>("notification", RequestMethod.GET, null);
		}

		public static Task<RestRequestResponse> SetStatus(string id, NotificationStatus status)
		{
			return RestService.Instance.MakeRecorderAPIRestRequest("notification/" + id + "/" + (int)status, RequestMethod.PUT, null);
		}
	}
}
