using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public class ContentItemEx : ContentItemDetails, IContentItem
	{
		private ObservableCollection<IContentItem> children;
		private bool inQueue;
		private bool expired;
		private bool isFromSearch;
		private bool isDeepLink;

		public ObservableCollection<IContentItem> Children
		{
			get { return children; }
			set
			{
				if (SetField(ref children, value) && (children != null))
					foreach (var child in children)
						child.Parent = this;
			}
		}

		public IContentItem Parent { get; set; }

		public bool IsRoot
		{
			get { return string.IsNullOrEmpty(ID); }
		}

		public bool IsFolder
		{
			get { return Type == ContentType.Folder; }
		}

		public virtual bool IsChannel
		{
			get { return false; }
		}

		[JsonIgnore]
		public bool InQueue
		{
			get { return inQueue; }
			set { SetField(ref inQueue, value); }
		}

		[JsonIgnore]
		public bool Expired
		{
			get { return expired; }
			set { SetField(ref expired, value); }
		}

		[JsonIgnore]
		public string ChannelThumbnailUrl
		{
			get { return getChannelImageUrl(true); }
		}

		[JsonIgnore]
		public string SmallThumbnailUrl
		{
			get
			{
				if (IsFromSearch || IsDeepLink)
					return getChannelImageUrl(true);

				if (!string.IsNullOrEmpty(ID))
					return ContentClient.GetSmallImageUrl(ID, (ImageRatio > 1) ? 192 : 128, 128);

				return null;
			}
		}

		[JsonIgnore]
		public string LargeThumbnailUrl
		{
			get
			{
				if (IsFromSearch || IsDeepLink)
					return getChannelMasheadImageUrl();

				if (!string.IsNullOrEmpty(ID))
					return ContentClient.GetLargeImageUrl(ID, 720, 480);

				return null;
			}
		}

		[JsonIgnore]
		public string LargeThumbnailMastheadUrl
		{
			get
			{
				if (IsFromSearch || IsDeepLink)
					return getChannelMasheadImageUrl();

				if (!string.IsNullOrEmpty(ID))
					return ContentClient.GetLargeImageUrl(ID, 954, 480);

				return null;
			}
		}

		[JsonIgnore]
		public double ImageRatio { get; set; }

		[JsonIgnore]
		public string FullName
		{
			get
			{
				if (!string.IsNullOrEmpty(Series) && !string.IsNullOrEmpty(Name) && !Name.StartsWith(Series))
					return Series + ": " + Name;

				return Name;
			}
		}

		[JsonIgnore]
		public bool HasSeries
		{
			get { return !string.IsNullOrEmpty(Series); }
		}

		[JsonIgnore]
		public string BrowsePathUI
		{
			get
			{
				if (!string.IsNullOrEmpty(BrowsePath))
					return BrowsePath.Replace(" | ", " > ");

				return null;
			}
		}

		[JsonIgnore]
		public bool IsFromSearch
		{
			get { return isFromSearch; }
			set { SetField(ref isFromSearch, value); }
		}

		[JsonIgnore]
		public bool IsDeepLink
		{
			get { return isDeepLink; }
			set { SetField(ref isDeepLink, value); }
		}

		private string getChannelImageUrl(bool small)
		{
			var channel = getChannel();
			if (channel != null)
			{
				if (small)
					return channel.SmallThumbnailUrl;
				else
					return channel.LargeThumbnailUrl;
			}

			return null;
		}

		private string getChannelMasheadImageUrl()
		{
			var channel = getChannel();
			if (channel != null)
				return channel.MastheadUrl;

			return null;
		}

		private ChannelEx getChannel()
		{
			var current = Parent;
			while (current != null)
			{
				if (current.IsChannel && (current is ChannelEx))
					return (current as ChannelEx);

				current = current.Parent;
			}

			return null;
		}

		public string GetBrowsePath()
		{
			var parents = GetParents();
			if (parents.Any())
			{
				parents.Reverse();
				return parents.Select(p => p.Name).Aggregate((a, b) => a + " | " + b);
			}

			return null;
		}

		public List<IContentItem> GetParents()
		{
			var parents = new List<IContentItem>();

			var current = Parent;
			while (current != null)
			{
				if (current.IsRoot)
					break;

				parents.Add(current);
				current = current.Parent;
			}

			if (parents.Any())
				parents.Reverse();

			return parents;
		}

		public void UpdateFromDetails(ContentItemEx details)
		{
			if (details != null)
			{
				EstimatedDuration = details.EstimatedDuration;
				OnPropertyChanged("EstimatedDuration");
				Series = details.Series;
				OnPropertyChanged("Series");
				OnPropertyChanged("HasSeries");
				Season = details.Season;
				OnPropertyChanged("Season");
				Episode = details.Episode;
				OnPropertyChanged("Episode");
				ReleaseYear = details.ReleaseYear;
				OnPropertyChanged("ReleaseYear");
				BrowsePath = details.BrowsePath;
				OnPropertyChanged("BrowsePath");
				OnPropertyChanged("BrowsePathUI");
				ProviderID = details.ProviderID;
				OnPropertyChanged("ProviderID");
				ContentRating = details.ContentRating;
				OnPropertyChanged("ContentRating");
				OnPropertyChanged("FullName");
			}
		}
	}
}
