using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public static class QueueClient
	{
		public static Task<RecordQueueItemsCollection> Get()
		{
			return RestService.Instance.MakeRecorderAPIRestRequest<RecordQueueItemsCollection>("queue", RequestMethod.GET, null);
		}

		public static Task<RecordQueueItem> Get(string id)
		{
			return RestService.Instance.MakeRecorderAPIRestRequest<RecordQueueItem>("queue/" + id, RequestMethod.GET, null);
		}

		public static Task<RecordQueueItemProgress> GetStatus(string id)
		{
			return RestService.Instance.MakeRecorderAPIRestRequest<RecordQueueItemProgress>("queue/" + id + "/status", RequestMethod.GET, null);
		}

		public static Task<RestRequestResponse> Delete(string id)
		{
			return RestService.Instance.MakeRecorderAPIRestRequest("queue/" + id, RequestMethod.DELETE, null);
		}

		public static Task<RestRequestResponse> SetRank(string id, int rank)
		{
			return RestService.Instance.MakeRecorderAPIRestRequest("queue/" + id + "/" + rank, RequestMethod.PUT, null);
		}

		public static Task<RestRequestResponse> Add(string recordToken, byte[] smallThumb, byte[] largeThumb)
		{
			var content = new MultipartFormDataContent()
			{
				{ new StringContent(recordToken), "record_token" }
			};

			if (smallThumb != null)
			{
				var imageContent = new ByteArrayContent(smallThumb);
				imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
				content.Add(imageContent, "small_thumbnail", "image.png");
			}

			if (largeThumb != null)
			{
				var imageContent = new ByteArrayContent(largeThumb);
				imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
				content.Add(imageContent, "large_thumbnail", "image_large.png");
			}

			return RestService.Instance.MakeRecorderAPIRestRequest("queue", RequestMethod.POST, content);
		}
	}
}
