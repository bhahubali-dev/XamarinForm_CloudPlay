using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public static class RemoteNotificationType
	{
		public const string Notification = "n";
		public const string Link = "l";
		public const string Card = "c";
	}

	public class RemoteNotificationsArgs
	{
		public RemoteNotificationsArgs(bool success, string info)
		{
			Success = success;
			Info = info;
		}

		public bool Success { get; set; }

		public string Info { get; set; }
	}

	public class AppNotificationMessage
	{
		public AppNotificationMessage()
		{
		}

		public string ID { get; set; }

		public AppNotificationType Type { get; set; }

		public string Text { get; set; }

		[JsonIgnore]
		public TaskCompletionSource<bool> WaitHandle { get; set; }
	}

	public enum AppNotificationType
	{
		None,
		RecordingReady,
		RecordingFailed,
		DownloadComplete,
		DownloadExpiring,
		DownloadFailed,
		RemoteNotification
	}

	public interface IRemoteNotifications
	{
		void RegisterForNotifications();

		bool IsRegisteredForNotifications();

		bool CanRegisterForNotifications();

		Task TriggerLocalNotification(string message);

		Task TriggerLocalNotification(AppNotificationMessage arg);

		bool IsBackgoundRefreshEnabled();

		void OpenGeneralAppSettings();

		event EventHandler<RemoteNotificationsArgs> RegisteredForNotifications;

		void FireRegisteredForNotifications(RemoteNotificationsArgs args);

		event EventHandler<AppNotificationMessage> NotificationReceived;

		bool FireNotificationReceived(AppNotificationMessage args);

		event EventHandler<bool> BackgroundRefreshStatusChanged;

		void FireBackgroundRefreshStatusChanged(bool backgroundRefreshEnabled);

		string GetTokenType();

		string GetTokenValue();

		void SetTokenValue(string token);
	}

	public class RemoteNotificationsService
	{
		private static volatile IRemoteNotifications instance;
		private static object syncRoot = new object();

		public static IRemoteNotifications Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<IRemoteNotifications>();

				return instance;
			}
		}
	}
}
