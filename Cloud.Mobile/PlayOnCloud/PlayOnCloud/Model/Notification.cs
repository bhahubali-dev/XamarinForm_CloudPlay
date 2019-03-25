using System;
using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public class Notification : NotifyPropertyChanged, IModel
	{
		private string id;
		private string recordingID;
		private string title;
		private string series;
		private string thumbnailUrl;
		private LibraryItem libraryItem;
		private RecordQueueItem recordQueueItem;
		private NotificationType type;
		private NotificationStatus status;
		private DateTime created;
		private DateTime updated;
		private bool isChecked;

		public string ID
		{
			get { return id; }
			set { SetField(ref id, value); }
		}

		public string RecordingID
		{
			get { return recordingID; }
			set { SetField(ref recordingID, value); }
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

		public string ThumbnailUrl
		{
			get { return thumbnailUrl; }
			set { SetField(ref thumbnailUrl, value); }
		}

		public string ThumbnailDataUri
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

		public LibraryItem LibraryItem
		{
			get { return libraryItem; }
			set { SetField(ref libraryItem, value); }
		}

		[JsonProperty("Recording")]
		public RecordQueueItem RecordQueueItem
		{
			get { return recordQueueItem; }
			set { SetField(ref recordQueueItem, value); }
		}

		public NotificationType Type
		{
			get { return type; }
			set { SetField(ref type, value); }
		}

		public NotificationStatus Status
		{
			get { return status; }
			set { SetField(ref status, value); }
		}

		public DateTime Created
		{
			get { return created; }
			set { SetField(ref created, value); }
		}

		public DateTime Updated
		{
			get { return updated; }
			set { SetField(ref updated, value); }
		}

		[JsonIgnore]
		public bool Checked
		{
			get { return isChecked; }
			set { SetField(ref isChecked, value); }
		}
	}
}
