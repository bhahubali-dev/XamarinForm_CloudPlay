using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MediaMallTechnologies;

namespace PlayOnCloud.Model
{
	public class Channel : ContentItem
	{
		public bool IsSearchable { get; set; }

		public bool IsAvailable { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public ChannelCredentialsType CredentialsType { get; set; }

		public ChannelLoginMetadata LoginMetadata { get; set; }
	}

	public class ChannelLoginMetadata
	{
		public string Description { get; set; }

		public string LearnMoreHeader { get; set; }

		public string LearnMoreDescription { get; set; }

		public string ToolbarText { get; set; }

		public string URL { get; set; }

		public string AccountName { get; set; }
	}
}
