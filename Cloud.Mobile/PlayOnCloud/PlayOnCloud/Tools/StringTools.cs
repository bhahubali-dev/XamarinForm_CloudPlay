using System;

namespace PlayOnCloud
{
	public static class StringTools
	{
		public static string GetTimeString(long milliseconds)
		{
			return GetTimeString(TimeSpan.FromMilliseconds(milliseconds));
		}

		public static string GetTimeString(TimeSpan timeSpan)
		{
			string time = string.Empty;
			if (timeSpan.Days > 0)
				time = string.Format("{0}", timeSpan.Days) + "d";
			else if (timeSpan.Hours > 0)
				time = string.Format("{0:N0}", timeSpan.Hours) + ":" + timeSpan.Minutes.ToString("D2") + "h";
			else if (timeSpan.Minutes > 0)
				time = string.Format("{0:N0}", timeSpan.Minutes) + ":" + timeSpan.Seconds.ToString("D2") + "m";
			else
				time = timeSpan.Seconds.ToString() + "s";

			return time;
		}

		public static string GetTimeStringFromMinutes(long minutes)
		{
			return GetTimeString(TimeSpan.FromMinutes(minutes));
		}

		public static string GetFileSizeString(long length)
		{
			long B = 1; //byte
			long KB = 1024 * B; //kilobyte
			long MB = 1024 * KB; //megabyte
			long GB = 1024 * MB; //gigabyte

			string size = string.Empty;
			if (length < KB)
				size = string.Format("{0:0}b", Math.Max(0, length));
			else if (length < MB)
				size = string.Format("{0:0}KB", (length / KB) + ((length * 100 / KB) % 100) / 100.00);
			else if (length < GB)
				size = string.Format("{0:0}MB", (length / MB) + ((length * 100 / MB) % 100) / 100.00);
			else if (length >= GB)
				size = string.Format("{0:0.0}GB", (length / GB) + ((length * 100 / GB) % 100) / 100.00);

			return size;
		}
	}
}
