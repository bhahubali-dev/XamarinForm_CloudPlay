using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlayOnCloud
{
	public class ReceiptClient
	{
		public static Task<RestRequestResponse> Add(string encodedReceipt)
		{
			Dictionary<string, string> postData = new Dictionary<string, string>();
			postData.Add("receipt", encodedReceipt);
			return RestService.Instance.MakeRecorderAPIPostRestRequest("receipt", postData);
		}
	}
}
