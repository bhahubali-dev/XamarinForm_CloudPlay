using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Foundation;
using ObjCRuntime;
using UIKit;
using PlayOnCloud.iOS;
using Newtonsoft.Json;

[assembly: Dependency(typeof(RemoteNotifications))]
namespace PlayOnCloud.iOS
{
	class RemoteNotifications : IRemoteNotifications
	{
		private object syncRoot = new object();
		private SharedSettings sharedSettingsService = new SharedSettings();
		private string token;
		private NSObject BackgroundRefreshStatusNotification;

		public event EventHandler<RemoteNotificationsArgs> RegisteredForNotifications;

		public event EventHandler<AppNotificationMessage> NotificationReceived;

		public event EventHandler<bool> BackgroundRefreshStatusChanged;

		public void FireRegisteredForNotifications(RemoteNotificationsArgs args)
		{
			RegisteredForNotifications?.Invoke(this, args);
		}

		public bool FireNotificationReceived(AppNotificationMessage args)
		{
			var notificationReceived = NotificationReceived;
			if (notificationReceived != null)
			{
				notificationReceived(this, args);
				return true;
			}

			return false;
		}

		public void FireBackgroundRefreshStatusChanged(bool backgroundRefreshEnabled)
		{
			BackgroundRefreshStatusChanged?.Invoke(this, backgroundRefreshEnabled);
		}

		public void RegisterForNotifications()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				/*
				var recordAction = new UIMutableUserNotificationAction();
				recordAction.Title = "Record";
				recordAction.Identifier = "OPENLINK_IDENTIFIER";
				recordAction.ActivationMode = UIUserNotificationActivationMode.Foreground;

				var recordCategory = new UIMutableUserNotificationCategory();
				recordCategory.Identifier = "RECORD_CATEGORY";
				recordCategory.SetActions(new UIUserNotificationAction[] { recordAction }, UIUserNotificationActionContext.Default);

				var categories = new NSSet(recordCategory);
				*/
				NSSet categories = null;
				var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Alert | UIUserNotificationType.Sound | UIUserNotificationType.Badge, categories);

				UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
				UIApplication.SharedApplication.RegisterForRemoteNotifications();
			}
			else
				UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(UIRemoteNotificationType.Alert | UIRemoteNotificationType.Sound | UIRemoteNotificationType.Badge);
		}

		public bool CanRegisterForNotifications()
		{
			var result = (ObjCRuntime.Runtime.Arch == Arch.DEVICE);
			BackgroundRefreshStatusNotification = UIApplication.Notifications.ObserveBackgroundRefreshStatusDidChange((sender, args) =>
			{
				FireBackgroundRefreshStatusChanged(UIApplication.SharedApplication.BackgroundRefreshStatus == UIBackgroundRefreshStatus.Available);
			});

			return result;
		}

		public bool IsRegisteredForNotifications()
		{
			var isRegistered = false;

			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
				isRegistered = UIApplication.SharedApplication.IsRegisteredForRemoteNotifications;
			else
			{
				var types = UIApplication.SharedApplication.EnabledRemoteNotificationTypes;
				isRegistered = (types != UIRemoteNotificationType.None);
			}

			return isRegistered;
		}

		public async Task TriggerLocalNotification(string message)
		{
			await TriggerLocalNotification(new AppNotificationMessage() { Text = message });
		}

		public async Task TriggerLocalNotification(AppNotificationMessage message)
		{
			if ((message == null) || string.IsNullOrEmpty(message.Text))
				return;

			Logger.Log("RemoteNotifications.TriggerLocalNotification: message: " + message + " nid: " + message.ID);

			TaskCompletionSource<bool> localNotificationtriggered = new TaskCompletionSource<bool>();
			Device.BeginInvokeOnMainThread(() =>
			{
				Logger.Log("RemoteNotifications.TriggerLocalNotification: BeginInvokeOnMainThread: ");

				TriggerLocalNotificationUnsafe(message);
				localNotificationtriggered.SetResult(true);
			});

			await localNotificationtriggered.Task;
		}

		public void TriggerLocalNotificationUnsafe(AppNotificationMessage message)
		{
			if ((message == null) || string.IsNullOrEmpty(message.Text))
				return;

			if (UIApplication.SharedApplication.ApplicationState == UIApplicationState.Background)
			{
				var notification = new UILocalNotification
				{
					FireDate = NSDate.FromTimeIntervalSinceNow(1),
					AlertBody = message.Text,
					SoundName = UILocalNotification.DefaultSoundName
				};

				if (!string.IsNullOrEmpty(message.ID))
					notification.UserInfo = new NSDictionary("nm", JsonConvert.SerializeObject(message));

				UIApplication.SharedApplication.ScheduleLocalNotification(notification);
			}
			else
				Logger.Log("WARNING: RemoteNotifications.TriggerLocalNotification: notification not triggered because app is active");
		}

		public bool IsBackgoundRefreshEnabled()
		{
			return (UIApplication.SharedApplication.BackgroundRefreshStatus == UIBackgroundRefreshStatus.Available);
		}

		public void OpenGeneralAppSettings()
		{
			UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
		}

		public string GetTokenType()
		{
			return "ios";
		}

		public string GetTokenValue()
		{
			lock (syncRoot)
			{
				if (!string.IsNullOrEmpty(token))
					return token;

				token = sharedSettingsService.GetStringValue("DeviceToken");
				return token;
			}
		}

		public void SetTokenValue(string deviceToken)
		{
			lock (syncRoot)
			{
				token = deviceToken;
				sharedSettingsService.SetStringValue("DeviceToken", token);
			}
		}
	}
}
