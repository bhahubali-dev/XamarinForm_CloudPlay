namespace PlayOnCloud.Model
{
	public class DownloadProgress : NotifyPropertyChanged
	{
		private long downloadedLength;
		private long expectedDuration;
		private double percent;
		private long totalLength;
		private long downloadedDuration;
		private DownloadStatus status;
		private bool willRetryOnError;

		public string Id { get; set; }

		public string Title { get; set; }

		public string Url { get; set; }

		public string LocalFilePath { get; set; }

		public bool IsCustomError { get; set; }

		public long DownloadedLength
		{
			get { return downloadedLength; }
			set { SetField(ref downloadedLength, value); }
		}

		public long ExpectedDuration
		{
			get { return expectedDuration; }
			set { SetField(ref expectedDuration, value); }
		}

		public double Percent
		{
			get { return percent; }
			set { SetField(ref percent, value); }
		}

		public long TotalLength
		{
			get { return totalLength; }
			set { SetField(ref totalLength, value); }
		}

		public long DownloadedDuration
		{
			get { return downloadedDuration; }
			set { SetField(ref downloadedDuration, value); }
		}

		public DownloadStatus Status
		{
			get { return status; }
			set { SetField(ref status, value); }
		}

		public bool WillRetryOnError
		{
			get { return willRetryOnError; }
			set { SetField(ref willRetryOnError, value); }
		}
	}
}
