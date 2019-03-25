namespace PlayOnCloud.Model
{
	public class RecordQueueItemProgress : NotifyPropertyChanged
	{
		private RecordingStatus status;
		private long estimatedDuration;
		private long? timeSinceStarted;

		public long? TimeSinceStarted
		{
			get { return timeSinceStarted; }
			set { SetField(ref timeSinceStarted, value); }
		}

		public long EstimatedDuration
		{
			get { return estimatedDuration; }
			set { SetField(ref estimatedDuration, value); }
		}

		public RecordingStatus Status
		{
			get { return status; }
			set { SetField(ref status, value); }
		}

		public double Percent
		{
			get
			{
				if ((estimatedDuration == 0) || !timeSinceStarted.HasValue)
					return 0;

				var percent = (double)timeSinceStarted.Value / (double)estimatedDuration;
				if (percent > 1)
					percent = 1;

				if (percent < 0)
					percent = 0;

				return percent;
			}
		}
	}
}
