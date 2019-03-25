using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlayOnCloud
{
	public class DownloadItem
	{
		public string FileName { get; set; }

		public string Id { get; set; }

		public DateTime? TimeStarted { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("data")]
		public Dictionary<string, string> Headers { get; set; }

		[JsonIgnore]
		public int RetryCount { get; set; }
	}
}
