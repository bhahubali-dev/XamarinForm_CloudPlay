using System.Threading.Tasks;

namespace PlayOnCloud
{
	public static class LogoutClient
	{
		public static Task<RestRequestResponse> Logout()
		{
			return RestService.Instance.MakeRecorderAPIRestRequest("logout", RequestMethod.POST, null);
		}
	}
}
