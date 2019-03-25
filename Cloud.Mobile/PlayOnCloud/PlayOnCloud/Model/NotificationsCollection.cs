using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public class NotificationsCollection
	{
		[JsonProperty("total_entries")]
		public int TotalEntries { get; set; }

		[JsonProperty("entries")]
		public IEnumerable<Notification> Entries { get; set; }
	}
}
