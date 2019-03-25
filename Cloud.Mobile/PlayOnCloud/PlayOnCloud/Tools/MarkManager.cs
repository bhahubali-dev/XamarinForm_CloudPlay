using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PlayOnCloud
{
	public enum Mark
	{
		None = 1,
		Hulu = 2,
		Netflix = 4
	}

	public class MarkManager
	{
		private const string huluMark = "HuluMark";
		private const string netflixMark = "NetflixMark";
		private static TimeSpan timerPeriod = TimeSpan.FromHours(1.0);

		private static Mark mark;
		private static object syncRoot = new object();
		private static Timer timer = new Timer((s) => inspectMarkSync(), null, Timeout.Infinite, Timeout.Infinite);

		static MarkManager()
		{
			loadMarks();
		}

		private static void saveMark()
		{
			if (mark.HasFlag(Mark.Hulu))
			{
				SharedSettingsService.Instance.SetStringValue(huluMark, "1");
				SharedSettingsService.Instance.Save();
			}

			if (mark.HasFlag(Mark.Netflix))
			{
				SharedSettingsService.Instance.SetStringValue(netflixMark, "1");
				SharedSettingsService.Instance.Save();
			}
		}

		private static void loadMarks()
		{
			mark = Mark.None;

			try
			{
				string markString = SharedSettingsService.Instance.GetStringValue(huluMark);
				if (!string.IsNullOrWhiteSpace(markString) && markString.Equals("1"))
					mark |= Mark.Hulu;

				markString = SharedSettingsService.Instance.GetStringValue(netflixMark);
				if (!string.IsNullOrWhiteSpace(markString) && markString.Equals("1"))
					mark |= Mark.Netflix;
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("Error: MarkManager.loadMarks: " + ex.Message);
			}
		}

		public static Mark GetMark()
		{
			lock (syncRoot)
				return mark;
		}

		private static IPAddress getPublicIPAddress()
		{
			var ipUrls = new string[] { "http://ipinfo.io/ip", "https://api.ipify.org/", "http://icanhazip.com/" };

			foreach (string ipUrl in ipUrls)
			{
				try
				{
					using (var hc = new HttpClient())
					{
						var htmlSource = hc.GetStringAsync(ipUrl).Result;
						if (!string.IsNullOrEmpty(htmlSource))
						{
							IPAddress ipAddress;
							if (IPAddress.TryParse(htmlSource.Trim(), out ipAddress))
								return ipAddress;
						}
					}
				}
				catch (Exception ex)
				{
					LoggerService.Instance.Log("Error: Reachability.GetPublicIPAddress downloading '" + ipUrl + "' : " + ex.Message);
				}
			}

			return null;
		}

		private static void inspectMarkSync()
		{
			lock (syncRoot)
			{
				try
				{
					var ipAddress = getPublicIPAddress();
					if ((ipAddress != null) && (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
					{
						var ipAddressStr = ipAddress.ToString();
						var digits = ipAddress.GetAddressBytes();
						var cachedMark = mark;

						if (!cachedMark.HasFlag(Mark.Hulu))
						{
							// 199.60.116.0 - 199.60.116.255
							// 208.91.156.0 - 208.91.159.255
							// 199.200.48.0 - 199.200.51.255

							if (ipAddressStr.StartsWith("199.60.116."))
								cachedMark |= Mark.Hulu;
							else if (ipAddressStr.StartsWith("208.91.15") && (digits[2] >= 156) && (digits[2] <= 159))
								cachedMark |= Mark.Hulu;
							else if (ipAddressStr.StartsWith("199.200.") && (digits[2] >= 48) && (digits[2] <= 51))
								cachedMark |= Mark.Hulu;
						}

						if (!cachedMark.HasFlag(Mark.Netflix))
						{
							// 208.75.76.0 - 208.75.79.255
							// 69.53.224.0 - 69.53.255.255

							if (ipAddressStr.StartsWith("208.75.7") && (digits[2] >= 76) && (digits[2] <= 79))
								cachedMark |= Mark.Netflix;
							else if (ipAddressStr.StartsWith("69.53.2") && (digits[2] >= 224))
								cachedMark |= Mark.Netflix;
						}

						if (!mark.Equals(cachedMark))
						{
							mark = cachedMark;
							saveMark();
						}
					}
				}
				catch (Exception ex)
				{
					LoggerService.Instance.Log("Error: MarkManager.InspectMark: " + ex.Message);
				}
			}
		}

		public static async void InspectMark()
		{
			await Task.Run(() => inspectMarkSync());
		}

		public static void StopMarkTimer()
		{
			timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		public static void StartMarkTimer()
		{
			StopMarkTimer();
			InspectMark();
			timer.Change(timerPeriod, timerPeriod);
		}
	}
}
