using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public enum LibraryItemStorage
	{
		Unknown = 0,
		iTunes,
		AppLocal
	}

	public class LibraryItem : NotifyPropertyChanged, IModel
	{
		private string id;
		private LibraryItemStorage storage;
		private string title;
		private string description;
		private string series;
		private string season;
		private string episode;
		private string airDate;
		private long duration;
		private string browsePath;
		private string contentRating;
		private string thumbnailUrl;
		private byte[] smallThumbnailData;
		private byte[] largeThumbnailData;
		private bool hasChapters;
		private List<Chapter> chapters;
		private DateTime updated;
		private DateTime recorded;
		private DownloadStatus downloadStatus;
		private DownloadProgress downloadProgress;
		private bool isChecked;
		private bool isLocal;
		private string localFilePath;
		private double bookmarkTime;
		private string hardLinkFileName;
		private string downloadUrl;
		private DateTime expires;
		private LibraryItem localItem;

		public string ID
		{
			get { return id; }
			set { SetField(ref id, value); }
		}

		public LibraryItemStorage Storage
		{
			get { return storage; }
			set { SetField(ref storage, value); }
		}

		[JsonProperty("Name")]
		public string Title
		{
			get
			{
				if (string.IsNullOrEmpty(title))
					return series;

				return title;
			}
			set { SetField(ref title, value); }
		}

		[JsonIgnore]
		public string FullTitle
		{
			get
			{
				if (!string.IsNullOrEmpty(Series) && !string.IsNullOrEmpty(Title) && !Title.StartsWith(Series))
					return Series + ": " + Title;

				return Title;
			}
		}

		public string Description
		{
			get { return description; }
			set { SetField(ref description, value); }
		}

		public string Series
		{
			get { return series; }
			set { SetField(ref series, value); }
		}

		public string Season
		{
			get { return season; }
			set { SetField(ref season, value); }
		}

		public string Episode
		{
			get { return episode; }
			set { SetField(ref episode, value); }
		}

		public string AirDate
		{
			get { return airDate; }
			set { SetField(ref airDate, value); }
		}

		public long Duration
		{
			get { return duration; }
			set { SetField(ref duration, value); }
		}

		public string BrowsePath
		{
			get { return browsePath; }
			set
			{
				if (SetField(ref browsePath, value))
					OnPropertyChanged("BrowsePathUI");
			}
		}

		public string BrowsePathUI
		{
			get
			{
				if (!string.IsNullOrEmpty(browsePath))
					return browsePath.Replace(" | ", " > ");

				return null;
			}
		}

		public string ContentRating
		{
			get { return contentRating; }
			set { SetField(ref contentRating, value); }
		}

		public string ThumbnailUrl
		{
			get { return thumbnailUrl; }
			set { SetField(ref thumbnailUrl, value); }
		}

		public byte[] SmallThumbnailData
		{
			get
			{
				if ((smallThumbnailData == null) && !IsLocal && !string.IsNullOrEmpty(thumbnailUrl))
					Task.Run(() => SmallThumbnailData = LibraryClient.GetSmallImageFromUrl(thumbnailUrl).Result);

				return smallThumbnailData;
			}
			set { SetField(ref smallThumbnailData, value); }
		}

		public byte[] LargeThumbnailData
		{
			get
			{
				if ((largeThumbnailData == null) && !IsLocal && !string.IsNullOrEmpty(thumbnailUrl))
					Task.Run(() => LargeThumbnailData = LibraryClient.GetLargeImageFromUrl(thumbnailUrl).Result);

				return largeThumbnailData;
			}
			set { SetField(ref largeThumbnailData, value); }
		}

		public string SmallThumbnailUri
		{
			get
			{
				if (!string.IsNullOrEmpty(thumbnailUrl))
				{
					UriBuilder builder = new UriBuilder(thumbnailUrl);
					return new Uri(builder.Uri, "small").ToString();
				}

				return null;
			}
		}

		public string LargeThumbnailUri
		{
			get
			{
				if (!string.IsNullOrEmpty(thumbnailUrl))
				{
					UriBuilder builder = new UriBuilder(thumbnailUrl);
					return new Uri(builder.Uri, "large").ToString();
				}

				return null;
			}
		}

		public bool HasChapters
		{
			get { return hasChapters; }
			set { SetField(ref hasChapters, value); }
		}

		public List<Chapter> Chapters
		{
			get { return chapters; }
			set { SetField(ref chapters, value); }
		}

		public DateTime Updated
		{
			get { return updated; }
			set { SetField(ref updated, value); }
		}

		[JsonIgnore]
		public DateTime Recorded
		{
			get { return recorded; }
			set { SetField(ref recorded, value); }
		}

		[JsonIgnore]
		public DownloadStatus DownloadStatus
		{
			get { return downloadStatus; }
			set { SetField(ref downloadStatus, value); }
		}

		public DownloadProgress DownloadProgress
		{
			get { return downloadProgress; }
			set { SetField(ref downloadProgress, value); }
		}

		public bool Checked
		{
			get { return isChecked; }
			set { SetField(ref isChecked, value); }
		}

		public bool IsLocal
		{
			get { return isLocal; }
			set { SetField(ref isLocal, value); }
		}

		public string LocalFilePath
		{
			get { return localFilePath; }
			set { SetField(ref localFilePath, value); }
		}

		public double BookmarkTime
		{
			get { return bookmarkTime; }
			set { SetField(ref bookmarkTime, value); }
		}

		public string HardLinkFileName
		{
			get { return hardLinkFileName; }
			set { SetField(ref hardLinkFileName, value); }
		}

		public string DownloadUrl
		{
			get { return downloadUrl; }
			set { SetField(ref downloadUrl, value); }
		}

		public DateTime Expires
		{
			get { return expires; }
			set { SetField(ref expires, value); }
		}

		[JsonIgnore]
		public bool DetailsLoaded { get; set; }

		[JsonIgnore]
		public LibraryItem LocalItem
		{
			get { return localItem; }
			set { SetField(ref localItem, value); }
		}

		public void UpdateFromDetails(LibraryItem details)
		{
			//XXX: Add details properties
			Description = details.Description;
			Series = details.Series;
			Season = details.Season;
			Episode = details.Episode;
			AirDate = details.AirDate;
			ContentRating = details.ContentRating;
			BrowsePath = details.BrowsePath;
			OnPropertyChanged("FullTitle");
		}

		public void RefreshImages()
		{
			OnPropertyChanged("SmallThumbnailUri");
			OnPropertyChanged("LargeThumbnailUri");
		}
	}
}
