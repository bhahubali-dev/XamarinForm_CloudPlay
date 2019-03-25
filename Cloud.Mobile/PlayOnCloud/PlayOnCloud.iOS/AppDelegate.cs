using Facebook.CoreKit;
using Foundation;
using MessageUI;
using Newtonsoft.Json;
using ObjCRuntime;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace PlayOnCloud.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		private nint backgroundTaskId;
		private Action backgroundSessionCompletionHandler;

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			Logger.Log("AppDelegate.FinishedLaunching");

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

			Xamarin.Forms.Forms.Init();

			//XXX: Must initialize the UAS first on a main thread before anything can use it
			UserAgentService.Init();

			FFImageLoading.Forms.Touch.CachedImageRenderer.Init();
			FFImageLoading.ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration()
			{
				HttpClient = RestService.Instance.Client,
				FadeAnimationEnabled = false,
				FadeAnimationForCachedImages = false,
				BitmapOptimizations = false,
				TransformPlaceholders = false
			});

			var downloadCache = (FFImageLoading.ImageService.Instance.Config.DownloadCache as FFImageLoading.Cache.DownloadCache);
			if (downloadCache != null)
				downloadCache.DelayBetweenRetry = TimeSpan.FromSeconds(0);
			else
				Logger.Log("WARNING: Unable to cast FFImageLoading DownloadCache!");

			PopupService.Init();

			Settings.AppID = "1626108114272416";
			Settings.DisplayName = "PlayOnCloud";

			AudioToolbox.AudioSession.Initialize();
			AudioToolbox.AudioSession.Category = AudioToolbox.AudioSessionCategory.MediaPlayback;

			LoadApplication(new App());

			var result = base.FinishedLaunching(app, options);
			RootController = app.KeyWindow.RootViewController;
			DisplayCrashReport(app.KeyWindow.RootViewController);

			if ((options != null) && options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
			{
				Logger.Log("AppDelegate.FinishedLaunching: Remote Notification");
				backgroundTaskId = UIApplication.SharedApplication.BeginBackgroundTask(() =>
				{
					UIApplication.SharedApplication.EndBackgroundTask(backgroundTaskId);
					backgroundTaskId = 0;
				});
			}

			//UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);

			Logger.Log("AppDelegate.FinishedLaunching: Complete");
			return result;
		}

		public override void OnResignActivation(UIApplication uiApplication)
		{
			Logger.Log("AppDelegate.OnResignActivation");
			base.OnResignActivation(uiApplication);
		}

		public override void DidEnterBackground(UIApplication uiApplication)
		{
			Logger.Log("AppDelegate.DidEnterBackground");
			base.DidEnterBackground(uiApplication);
		}

		public override void WillEnterForeground(UIApplication uiApplication)
		{
			Logger.Log("AppDelegate.WillEnterForeground");
			base.WillEnterForeground(uiApplication);
		}

		public override void OnActivated(UIApplication uiApplication)
		{
			Logger.Log("AppDelegate.OnActivated");
			base.OnActivated(uiApplication);
			var app = (Xamarin.Forms.Application.Current as App);
			app?.Activate();
		}

		public override void WillTerminate(UIApplication uiApplication)
		{
			Logger.Log("AppDelegate.WillTerminate");
			base.WillTerminate(uiApplication);
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, [Transient] UIWindow forWindow)
		{
			UIInterfaceOrientationMask orientationMask = UIInterfaceOrientationMask.Portrait;

			if (Xamarin.Forms.Device.Idiom == Xamarin.Forms.TargetIdiom.Phone)
			{
				if ((Xamarin.Forms.Application.Current != null) &&
					(Xamarin.Forms.Application.Current.MainPage != null) &&
					(Xamarin.Forms.Application.Current.MainPage.Navigation.ModalStack.LastOrDefault() is PlayOnCloud.MediaPlayer))
				{
					orientationMask |= UIInterfaceOrientationMask.Landscape;
				}
			}
			else
				orientationMask |= UIInterfaceOrientationMask.Landscape;

			return orientationMask;
		}

		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			Logger.Log("AppDelegate.OpenUrl: {0}", url);

			var isFb = ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
			if (isFb)
				return true;

			if ((url == null) || (url.PathComponents == null) || (url.PathComponents.Length < 2))
				return false;

			switch (url.Host.ToLower())
			{
				case "internal":
					if ((url.PathComponents.Length >= 3) && (url.PathComponents[1] == "rest"))
						RestService.Instance.SetDomainPrefixAndAppToken(url.PathComponents[2].Trim('/'), (url.PathComponents.Length == 4) ? url.PathComponents[3].Trim('/') : string.Empty);
					else
						return false;
					break;

				case "content":
					if ((url.PathComponents.Length >= 4) && (url.PathComponents[2] == "link"))
						AppNavigation.FireAppNavigate(new AppNavigationArgs()
						{
							Page = Model.CloudItem.Content,
							ProviderID = url.PathComponents[1],
							Token = url.PathComponents[3]
						});
					else
						return false;
					break;

				case "recording":
					AppNavigation.FireAppNavigate(new AppNavigationArgs() { Page = Model.CloudItem.Library, ItemID = url.PathComponents[1] });
					break;

				case "message":
					AppNavigation.FireAppNavigate(new AppNavigationArgs() { Page = Model.CloudItem.Notifications, ItemID = url.PathComponents[1] });
					break;

				case "queue":
					AppNavigation.FireAppNavigate(new AppNavigationArgs() { Page = Model.CloudItem.Queue, ItemID = url.PathComponents[1] });
					break;
			}

			return true;
		}

		public static UIViewController RootController
		{
			get; set;
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			Logger.Log("AppDelegate.RegisteredForRemoteNotifications");
			AppEvents.SetPushNotificationsDeviceToken(deviceToken);

			var token = deviceToken.Description;
			if (!string.IsNullOrWhiteSpace(token))
			{
				var deviceTokenValue = token.Trim('<').Trim('>').Replace(" ", "");
				Logger.Log("AppDelegate.RegisteredForRemoteNotifications: Token: {0}", deviceTokenValue);

				RemoteNotificationsService.Instance.SetTokenValue(deviceTokenValue);
				RemoteNotificationsService.Instance.FireRegisteredForNotifications(new RemoteNotificationsArgs(true, deviceTokenValue));
			}
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			Logger.Log("AppDelegate.FailedToRegisterForRemoteNotifications");
			Logger.Log("AppDelegate.FailedToRegisterForRemoteNotifications: Error: {0}", error.LocalizedDescription);
			RemoteNotificationsService.Instance.FireRegisteredForNotifications(new RemoteNotificationsArgs(false, error.LocalizedDescription));
		}

		public override async void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			var notificationType = userInfo["nt"]?.ToString();
			Logger.Log("AppDelegate.DidReceiveRemoteNotification: NT: {0}", notificationType);

			try
			{
				switch (notificationType)
				{
					case RemoteNotificationType.Notification:
						var nid = userInfo["nid"]?.ToString();
						Logger.Log("AppDelegate.DidReceiveRemoteNotification: NID: {0}", nid);

						var waitHandle = new TaskCompletionSource<bool>();
						if (RemoteNotificationsService.Instance.FireNotificationReceived(new AppNotificationMessage() { ID = nid, Type = AppNotificationType.RemoteNotification, WaitHandle = waitHandle }))
							await waitHandle.Task;

						Logger.Log("AppDelegate.DidReceiveRemoteNotification: Invoking completion handler");
						completionHandler?.Invoke(UIBackgroundFetchResult.NewData);
						break;

					case RemoteNotificationType.Link:
						AppEvents.LogPushNotificationOpen(userInfo);
						var lnk = userInfo["lnk"]?.ToString();

						bool result = false;
						if (!string.IsNullOrEmpty(lnk))
						{
							if (application.ApplicationState == UIApplicationState.Active)
							{
								var aps = userInfo["aps"] as NSDictionary;
								var alert = aps["alert"];

								var title = "Notification";
								var message = string.Empty;
								var action = "Open";

								if (alert is NSDictionary)
								{
									var alertDictionary = alert as NSDictionary;
									var alertTitle = alertDictionary["title"]?.ToString();
									var alertBody = alertDictionary["body"]?.ToString();
									var alertAction = alertDictionary["action-loc-key"]?.ToString();

									if (!string.IsNullOrEmpty(alertTitle))
										title = alertTitle;

									if (!string.IsNullOrEmpty(alertBody))
										message = alertBody;

									if (!string.IsNullOrEmpty(alertAction))
										action = alertAction;
								}
								else
									message = alert.ToString();

								if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(action))
								{
									var close = "Close";
									var closeLocalized = NSBundle.MainBundle.LocalizedString(close, close, null);
									var actionLocalized = NSBundle.MainBundle.LocalizedString(action, action, null);
									var av = new UIAlertView(title, message, null, closeLocalized, actionLocalized);
									av.Clicked += (s, e) =>
									{
										if (e.ButtonIndex == 1)
											if (!OpenUrl(application, new NSUrl(lnk), NSBundle.MainBundle.BundleIdentifier, userInfo))
												Logger.Log("AppDelegate.DidReceiveRemoteNotification: There was an error executing OpenUrl");
									};
									av.Show();
									result = true;
								}
							}
							else
							{
								BeginInvokeOnMainThread(() =>
								{
									if (!OpenUrl(application, new NSUrl(lnk), NSBundle.MainBundle.BundleIdentifier, userInfo))
										Logger.Log("AppDelegate.DidReceiveRemoteNotification: There was an error executing OpenUrl");
								});
								result = true;
							}
						}

						completionHandler?.Invoke(result ? UIBackgroundFetchResult.NewData : UIBackgroundFetchResult.NoData);
						break;

					case RemoteNotificationType.Card:
						AppEvents.LogPushNotificationOpen(userInfo);

#if __FBNOTIFICATIONS__
						FBNotificationsBindings.FBNotificationsManager.SharedManager()
							.PresentPushCardForRemoteNotificationPayload(userInfo,
								null,
								(c, e) =>
								{
									if (e != null)
									{
										Logger.Log("AppDelegate.DidReceiveRemoteNotification: There was an error displaying Facebook advertising notification");
										completionHandler?.Invoke(UIBackgroundFetchResult.Failed);
										Logger.Log("AppDelegate.DidReceiveRemoteNotification: Invoking completion handler");
									}
									else
									{
										Logger.Log("AppDelegate.DidReceiveRemoteNotification: Facebook advertising notification displayed successfully");
										completionHandler?.Invoke(UIBackgroundFetchResult.NewData);
										Logger.Log("AppDelegate.DidReceiveRemoteNotification: Invoking completion handler");
									}
								}
							);
#else
						Logger.Log("AppDelegate.DidReceiveRemoteNotification: Invoking completion handler");
						completionHandler?.Invoke(UIBackgroundFetchResult.NoData);
#endif
						break;

					default:
						Logger.Log("AppDelegate.DidReceiveRemoteNotification: Unrecognized notification type");
						completionHandler?.Invoke(UIBackgroundFetchResult.NoData);
						break;
				}
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: AppDelegate.DidReceiveRemoteNotification: " + ex);
				completionHandler?.Invoke(UIBackgroundFetchResult.Failed);
			}

			if (backgroundTaskId != 0)
			{
				UIApplication.SharedApplication.EndBackgroundTask(backgroundTaskId);
				backgroundTaskId = 0;
			}

			Logger.Log("AppDelegate.DidReceiveRemoteNotification: Event dispatch complete");
		}

		public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
		{
			Logger.Log("AppDelegate.ReceivedLocalNotification: Local notification received");

			if (notification.UserInfo?["nm"] != null)
			{
				try
				{
					var message = JsonConvert.DeserializeObject<AppNotificationMessage>(notification.UserInfo["nm"].ToString());
					Logger.Log("AppDelegate.ReceivedLocalNotification: NID: {0}", message.ID);

					RemoteNotificationsService.Instance.FireNotificationReceived(message);

					Logger.Log("AppDelegate.ReceivedLocalNotification: Event dispatch complete");
				}
				catch (Exception ex)
				{
					Logger.Log("ERROR: AppDelegate.ReceivedLocalNotification: " + ex);
				}
			}
		}

		/*public override void HandleAction(UIApplication application, string actionIdentifier, NSDictionary remoteNotificationInfo, Action completionHandler)
		{
			Logger.Log("AppDelegate.HandleAction: AID: {0}", actionIdentifier);

			if (actionIdentifier == "OPENLINK_IDENTIFIER")
				OpenUrl(application, new NSUrl(remoteNotificationInfo["lnk"]?.ToString()), NSBundle.MainBundle.BundleIdentifier, remoteNotificationInfo);

			completionHandler?.Invoke();
		}*/

		/* XXX: Disable background fetch temporarily
		public override async void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
		{
			UIBackgroundFetchResult result = UIBackgroundFetchResult.NoData;
			try
			{
				Logger.Log("AppDelegate.PerformFetch");

				var app = (Xamarin.Forms.Application.Current as App);
				if (await app?.PerformFetch())
					result = UIBackgroundFetchResult.NewData;
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: AppDelegate.PerformFetch: " + ex);
				result = UIBackgroundFetchResult.Failed;
			}
			finally
			{
				Logger.Log("AppDelegate.PerformFetch: Completed: " + result);
				completionHandler?.Invoke(result);
			}
		}*/

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Log("AppDelegate.CurrentDomain_UnhandledException");
			LogUnhandledException(e.ExceptionObject as Exception);
		}

		private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			Logger.Log("AppDelegate.TaskScheduler_UnobservedTaskException");
			LogUnhandledException(e.Exception);
		}

		internal static void LogUnhandledException(Exception exception)
		{
			if (Debugger.IsAttached)
				Debugger.Break();

			if (exception == null)
				return;

			try
			{
				string errorFileName = "Fatal.log";
				var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				var errorFilePath = Path.Combine(libraryPath, errorFileName);
				var errorMessage = string.Format("Time: {0}\r\nError: Unhandled Exception\r\n{1}", DateTime.UtcNow, exception.ToString());
				File.WriteAllText(errorFilePath, errorMessage);
			}
			catch
			{
			}
		}

		private void DisplayCrashReport(UIViewController root)
		{
			try
			{
				string errorFilename = "Fatal.log";
				var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				var errorFilePath = Path.Combine(libraryPath, errorFilename);

				if (!File.Exists(errorFilePath))
					return;

				var errorText = File.ReadAllText(errorFilePath);
				var alertView = new UIAlertView("Crash Report", errorText, null, "Send to Dev Team", "Clear") { UserInteractionEnabled = true };
				alertView.BecomeFirstResponder();
				alertView.Clicked += (sender, args) =>
				{
					if (args.ButtonIndex != 0)
						File.Delete(errorFilePath);
					else
						sendEmail(errorText, root);
				};

				alertView.Show();
			}
			catch
			{
			}
		}

		private void deleteCrashLogFile()
		{
			string errorFilename = "Fatal.log";
			var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var errorFilePath = Path.Combine(libraryPath, errorFilename);

			if (File.Exists(errorFilePath))
				File.Delete(errorFilePath);
		}

		private void sendEmail(string message, UIViewController root)
		{
			try
			{
				if (MFMailComposeViewController.CanSendMail)
				{
					var mailController = new MFMailComposeViewController();
					mailController.SetToRecipients(new string[] { "support@playon.tv" });
					mailController.SetSubject("Cloud Crash Report");
					mailController.SetMessageBody(message, false);

					try
					{
						var logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "log.log");
						if (File.Exists(logFile))
						{
							NSData logData = NSData.FromFile(logFile);
							if (logData != null)
								mailController.AddAttachmentData(logData, "text/plain", "log.txt");
						}
					}
					catch (Exception ex)
					{
						Logger.Log("ERROR AppDelegate.sendMail: Unable to attach log file: " + ex);
					}

					mailController.Finished += (object s, MFComposeResultEventArgs args) =>
					{
						BeginInvokeOnMainThread(() => args.Controller.DismissViewController(true, null));
						if (args.Result == MFMailComposeResult.Sent)
							deleteCrashLogFile();
					};

					root.PresentViewController(mailController, true, null);
				}
			}
			catch
			{
			}
		}

		public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
		{
			Logger.Log("AppDelegate.HandleEventsForBackgroundUrl: sessionIdentifier: {0}", sessionIdentifier);
			backgroundSessionCompletionHandler = completionHandler;
			ItemDownloaderService.Instance.CreateSession(sessionIdentifier);
		}

		public void FireBackgroundSessionCompletionHandler()
		{
			Action handler = backgroundSessionCompletionHandler;
			if (handler != null)
			{
				Logger.Log("AppDelegate.FireBackgroundSessionCompletionHandler: invoking handler");
				backgroundSessionCompletionHandler = null;
				handler.Invoke();
			}
			else
				Logger.Log("AppDelegate.FireBackgroundSessionCompletionHandler: handler is NULL");
		}
	}
}
