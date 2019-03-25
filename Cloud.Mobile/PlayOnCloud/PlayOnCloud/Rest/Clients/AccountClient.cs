using System.Threading.Tasks;

namespace PlayOnCloud
{
	public static class AccountClient
	{
		public static Task<RestRequestResponse> Get()
		{
			return RestService.Instance.MakeRecorderAPIRestRequest("account", RequestMethod.GET, null);
		}
	}
}
