using System.Net.Http;
using System.Threading.Tasks;

namespace PlayOnCloud
{
	public static class PasswordClient
	{
		public const string ResetUrl = "password/reset/request";

		public static Task<RestRequestResponse> ResetRequest(string email)
		{
			var content = new MultipartFormDataContent()
			{
				{ new StringContent(email), "email" }
			};

			return RestService.Instance.MakeRecorderAPIRestRequest(ResetUrl, RequestMethod.POST, content);
		}
	}
}
