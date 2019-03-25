using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PlayOnCloud
{
	public class RestRequestResponse
	{
		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("data")]
		public JToken Data { get; set; }

		[JsonProperty("error_code")]
		public string ErrorCode { get; set; }

		[JsonProperty("error_message")]
		public string ErrorMessage { get; set; }

		[JsonIgnore]
		public string ErrorMessageClean
		{
			get
			{
				string error = ErrorMessage;
				if (!string.IsNullOrEmpty(error) && error.StartsWith("API: "))
					error = error.Remove(0, "API: ".Length);

				return error;
			}
		}
	}
}
