using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public class RecordQueueItemsCollection
	{
		[JsonProperty("total_entries")]
		public int TotalEntries { get; set; }

		[JsonProperty("entries")]
		public IEnumerable<RecordQueueItem> Entries { get; set; }
	}
}
