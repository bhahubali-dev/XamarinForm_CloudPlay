using System.Threading.Tasks;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public static class ProductClient
	{
		public static Task<RecorderServiceProductCollection> Get()
		{
			return RestService.Instance.MakeRecorderAPIRestRequest<RecorderServiceProductCollection>("product", RequestMethod.GET, null);
		}
	}
}
