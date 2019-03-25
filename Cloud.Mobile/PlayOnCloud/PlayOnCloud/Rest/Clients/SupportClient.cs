using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PlayOnCloud
{
	public static class SupportClient
	{
		public static Task<RestRequestResponse> Submit(string requestContent, string rid, string logContents)
		{
			var content = new MultipartFormDataContent
			{
				{ new StringContent(!string.IsNullOrEmpty(requestContent) ? requestContent : string.Empty), "description" },
			};

			if (!string.IsNullOrEmpty(rid))
				content.Add(new StringContent(rid), "rid");

			if (!string.IsNullOrEmpty(logContents))
			{
				var logAttachment = new ByteArrayContent(Encoding.UTF8.GetBytes(logContents));
				logAttachment.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
				content.Add(logAttachment, "attachment", "app_log.txt");
			}

			return RestService.Instance.MakeRecorderAPIRestRequest("support", RequestMethod.POST, content);
		}
	}
}
