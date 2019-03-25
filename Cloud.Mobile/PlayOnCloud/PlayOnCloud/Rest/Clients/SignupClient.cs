using System.Net.Http;
using System.Threading.Tasks;

namespace PlayOnCloud
{
	public static class SignupClient
	{
		public const string SignupUrl = "signup";
		public const string FBSignupUrl = "signup/fb";

		public static Task<RestRequestResponse> SignupWithCredentials(string name, string email, string password)
		{
			var content = new MultipartFormDataContent
			{
				{ new StringContent(name), "name" },
				{ new StringContent(email), "email" },
				{ new StringContent(password), "password" },
				{ new StringContent(password), "password_confirm" }
			};

			return RestService.Instance.MakeRecorderAPIRestRequest(SignupUrl, RequestMethod.POST, content, false);
		}

		public static Task<RestRequestResponse> SignupWithFacebook(string id, string token)
		{
			MultipartFormDataContent content = new MultipartFormDataContent
			{
				{ new StringContent(id), "facebook_id" },
				{ new StringContent(token), "facebook_access_token" }
			};

			return RestService.Instance.MakeRecorderAPIRestRequest(FBSignupUrl, RequestMethod.POST, content, false);
		}
	}
}
