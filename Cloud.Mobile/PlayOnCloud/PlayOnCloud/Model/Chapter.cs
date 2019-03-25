namespace PlayOnCloud.Model
{
	public class Chapter : NotifyPropertyChanged
	{
		private double startTime;
		private ChapterType type;
		private bool isSkipped;

		public bool IsSkipped
		{
			get { return isSkipped; }
			set { SetField(ref isSkipped, value); }
		}

		public double StartTime
		{
			get { return startTime; }
			set { SetField(ref startTime, value); }
		}

		public ChapterType Type
		{
			get { return type; }
			set { SetField(ref type, value); }
		}
	}
}
