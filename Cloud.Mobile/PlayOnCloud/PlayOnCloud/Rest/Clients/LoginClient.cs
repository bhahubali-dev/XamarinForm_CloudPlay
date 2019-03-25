using System.Net.Http;
using System.Threading.Tasks;

namespace PlayOnCloud
{
	public static class LoginClient
	{
		public const string LoginWithTokenUrl = "login/at";
		public const string LoginUrl = "login";
		public const string LoginWithFBUrl = "login/fb";

		public static Task<RestRequestResponse> LoginWithToken(string token)
		{
			var content = new MultipartFormDataContent
			{
				{ new StringContent(token), "auth_token" }
			};

			return RestService.Instance.MakeRecorderAPIRestRequest(LoginWithTokenUrl, RequestMethod.POST, content, false);
		}

		public static Task<RestRequestResponse> LoginWithCredentials(string username, string password)
		{
			MultipartFormDataContent content = new MultipartFormDataContent
				{
					{ new StringContent(username), "email" },
					{ new StringContent(password), "password" }
				};

			return RestService.Instance.MakeRecorderAPIRestRequest(LoginUrl, RequestMethod.POST, content, false);
		}

		public static Task<RestRequestResponse> LoginWithFacebook(string id, string token)
		{
			MultipartFormDataContent content = new MultipartFormDataContent
			{
				{ new StringContent(id), "facebook_id" },
				{ new StringContent(token), "facebook_access_token" }
			};

			return RestService.Instance.MakeRecorderAPIRestRequest(LoginWithFBUrl, RequestMethod.POST, content, false);
		}
	}
}
