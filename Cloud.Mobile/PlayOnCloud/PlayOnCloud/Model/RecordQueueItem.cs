using System;
using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public class RecordQueueItem : NotifyPropertyChanged, IModel
	{
		private string id;
		private string accountID;
		private int rank;
		private string title;
		private string series;
		private string description;
		private string airDate;
		private long duration;
		private long estimatedDuration;
		private string browsePath;
		private string providerID;
		private string contentRating;
		private string season;
		private string episode;
		private string releaseYear;
		private string thumbnailUrl;
		private DateTime? created;
		private DateTime? started;
		private DateTime? updated;
		private int retryCount;
		private string accessInfo;
		private RecordingStatus recordingStatus;
		private RecordQueueItemProgress recordingProgress;

		public string ID
		{
			get { return id; }
			set { SetField(ref id, value); }
		}

		public string AccountID
		{
			get { return accountID; }
			set { SetField(ref accountID, value); }
		}

		public int Rank
		{
			get { return rank; }
			set { SetField(ref rank, value); }
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

		public string Series
		{
			get { return series; }
			set { SetField(ref series, value); }
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

		public long EstimatedDuration
		{
			get { return estimatedDuration; }
			set { SetField(ref estimatedDuration, value); }
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

		public string ProviderID
		{
			get { return providerID; }
			set { SetField(ref providerID, value); }
		}

		[JsonIgnore]
		public string ProviderName
		{
			get
			{
				if (!string.IsNullOrEmpty(browsePath))
				{
					string[] parts = browsePath.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
					if ((parts != null) && parts.Length > 0)
						return parts[0].Trim();
				}

				return null;
			}
		}

		public string ContentRating
		{
			get { return contentRating; }
			set { SetField(ref contentRating, value); }
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

		public string ReleaseYear
		{
			get { return releaseYear; }
			set { SetField(ref releaseYear, value); }
		}

		public string ThumbnailUrl
		{
			get { return thumbnailUrl; }
			set { SetField(ref thumbnailUrl, value); }
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

		public string SmallThumbnailUri
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

		public DateTime? Created
		{
			get { return created; }
			set { SetField(ref created, value); }
		}

		public DateTime? Started
		{
			get { return started; }
			set { SetField(ref started, value); }
		}

		public DateTime? Updated
		{
			get { return updated; }
			set { SetField(ref updated, value); }
		}

		public int RetryCount
		{
			get { return retryCount; }
			set { SetField(ref retryCount, value); }
		}

		public string AccessInfo
		{
			get { return accessInfo; }
			set { SetField(ref accessInfo, value); }
		}

		[JsonProperty("Status")]
		public RecordingStatus RecordingStatus
		{
			get { return recordingStatus; }
			set { SetField(ref recordingStatus, value); }
		}

		public RecordQueueItemProgress RecordingProgress
		{
			get { return recordingProgress; }
			set { SetField(ref recordingProgress, value); }
		}

		public void UpdateFromDetails(RecordQueueItem details)
		{
			Description = details.Description;
			Series = details.Series;
			Season = details.Season;
			Episode = details.Episode;
			AirDate = details.AirDate;
			ContentRating = details.ContentRating;
			BrowsePath = details.BrowsePath;
		}
	}
}
