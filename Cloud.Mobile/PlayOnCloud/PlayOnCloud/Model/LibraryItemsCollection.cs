using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public class LibraryItemsCollection
	{
		[JsonProperty("total_entries")]
		public int TotalEntries { get; set; }

		[JsonProperty("entries")]
		public IEnumerable<LibraryItem> Entries { get; set; }
	}
}
