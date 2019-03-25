using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public class Product
	{
		public string ProductIdentifier { get; set; }
		public string LocalizedTitle { get; set; }
		public string LocalizedDescription { get; set; }
		public double Price { get; set; }
		public string LocalizedPrice { get; set; }
	}

	public class RecorderServiceProduct
	{
		public string ExternalPID { get; set; }
	}

	public class RecorderServiceProductCollection
	{
		[JsonProperty("total_entries")]
		public int TotalEntries { get; set; }

		[JsonProperty("entries")]
		public IEnumerable<RecorderServiceProduct> Entries { get; set; }
	}
}
