using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using PlayOnCloud.Droid;

[assembly: Dependency(typeof(RemoteNotifications))]
namespace PlayOnCloud.Droid
{
	class RemoteNotifications : IRemoteNotifications
	{
		public event EventHandler<bool> BackgroundRefreshStatusChanged;
		public event EventHandler<AppNotificationMessage> NotificationReceived;
		public event EventHandler<RemoteNotificationsArgs> RegisteredForNotifications;

		public bool CanRegisterForNotifications()
		{
			//throw new NotImplementedException();
			return false;
		}

		public void FireBackgroundRefreshStatusChanged(bool backgroundRefreshEnabled)
		{
			throw new NotImplementedException();
		}

		public bool FireNotificationReceived(AppNotificationMessage args)
		{
			throw new NotImplementedException();
		}

		public void FireRegisteredForNotifications(RemoteNotificationsArgs args)
		{
			throw new NotImplementedException();
		}

		public string GetTokenType()
		{
			throw new NotImplementedException();
		}

		public string GetTokenValue()
		{
			throw new NotImplementedException();
		}

		public bool IsBackgoundRefreshEnabled()
		{
			//throw new NotImplementedException();
			return false;
		}

		public bool IsRegisteredForNotifications()
		{
			throw new NotImplementedException();
		}

		public void OpenGeneralAppSettings()
		{
			throw new NotImplementedException();
		}

		public void RegisterForNotifications()
		{
			throw new NotImplementedException();
		}

		public void SetTokenValue(string token)
		{
			throw new NotImplementedException();
		}

		public Task TriggerLocalNotification(AppNotificationMessage arg)
		{
			throw new NotImplementedException();
		}

		public Task TriggerLocalNotification(string message)
		{
			throw new NotImplementedException();
		}

		public Task TriggerLocalNotification(string message, string nid = "")
		{
			throw new NotImplementedException();
		}
	}
}