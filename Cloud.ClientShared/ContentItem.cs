using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PlayOnCloud.Model
{
	public enum ContentType
	{
		Folder,
		Video
	}

	public class ContentItem : NotifyPropertyChanged
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public ContentType Type { get; set; }

		public string ID { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public long Duration { get; set; }

		public DateTime AirDate { get; set; }
	}

	public class ContentItemDetails : ContentItem
	{
		public string Series { get; set; }

		public string Season { get; set; }

		public string Episode { get; set; }

		public string ReleaseYear { get; set; }

		public string ContentRating { get; set; }

		public string ProviderID { get; set; }

		public string BrowsePath { get; set; }

		public long EstimatedDuration { get; set; }
	}
}
