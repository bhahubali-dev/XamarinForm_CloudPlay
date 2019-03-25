using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud.ViewModel
{
	public class Cloud : ViewModelBase
	{
		private Register registerViewModel;
		private MediaContent mediaContentViewModel;
		private Account accountViewModel;
		private Library libraryViewModel;
		private Queue queueViewModel;
		private Notifications notificationsViewModel;
		private Products productsViewModel;
		private CloudItem selectedItem = CloudItem.None;

		public Cloud()
		{
			registerViewModel = new Register();
			accountViewModel = new Account();
			libraryViewModel = new Library(accountViewModel);
			queueViewModel = new Queue();
			queueViewModel.Cloud = this;
			notificationsViewModel = new Notifications(this);
			productsViewModel = new Products(this, accountViewModel, queueViewModel);
			mediaContentViewModel = new MediaContent(queueViewModel, accountViewModel, productsViewModel);
			accountViewModel.MediaContent = mediaContentViewModel;
			accountViewModel.Register = registerViewModel;
			queueViewModel.Account = accountViewModel;
			queueViewModel.MediaContent = mediaContentViewModel;

			SelectItem = new Command<CloudItem>((parameter) =>
			{
				SelectedItem = parameter;
			});
#if _IOS_
    
            RemoteNotificationsService.Instance.NotificationReceived += handleNotificationReceived;
#endif
            GoHome = new Command(() =>
			{
				SelectedItem = CloudItem.Content;
				mediaContentViewModel.GoHome();
			});

			OpenUrl = new Command<string>((parameter) => Device.OpenUri(new Uri(parameter)));
			AppNavigation.AppNavigate += AppNavigation_AppNavigate;

			NavigateToRecording = new Command<Notification>((parameter) =>
			{
				TaskQueue.Enqueue(() => navigateToRecordingFromNotification(parameter));
				if ((Application.Current.MainPage as NavigationPage).CurrentPage is DetailsPage)
					Application.Current.MainPage.Navigation.PopAsync();
			});

			ReachabilityHelperService.NetworkStatusChanged += async (sender, e) =>
			{
				await NetworkStatusChanged(e);
			};
				
			RestService.Instance.ServerConnectionLost += (sender, e) =>
			{
				if (IsInBackground)
					return;

				Device.BeginInvokeOnMainThread(async () =>
				{
					if (!e.Silent)
					{
						try
						{
							if (await Application.Current.MainPage.DisplayAlert("Can't connect",
								"We were unable to reach the server. Please check your internet connection and make sure you are online before hitting Refresh",
								"Go Offline",
								"Refresh"))
							{
								await NetworkStatusChanged(NetworkStatus.NotReachable);
								e.Cancel.SetResult(true);
							}
							else
								e.Cancel.SetResult(true);
						}
						catch (Exception ex)
						{
							e.Cancel.SetException(ex);
						}
					}
					else
					{
						await NetworkStatusChanged(NetworkStatus.NotReachable);
						e.Cancel.SetResult(true);
					}
				});
			};

			RestService.Instance.ServerConnectionRestored += async (sender, e) =>
			{
				await NetworkStatusChanged(NetworkStatus.ReachableViaWiFiNetwork);
			};

			mediaContentViewModel.OnSelectedItemDetailsChanged += MediaContentViewModel_OnSelectedItemDetailsChanged;
			libraryViewModel.OnSelectedItemDetailsChanged += LibraryViewModel_OnSelectedItemDetailsChanged;
			queueViewModel.OnSelectedItemDetailsChanged += QueueViewModel_OnSelectedItemDetailsChanged;
			notificationsViewModel.OnSelectedItemDetailsChanged += NotificationsViewModel_OnSelectedItemDetailsChanged;

			BackButtonHelper.BackButtonPressed += async (sender, args) =>
			{
				switch (SelectedItem)
				{
					case CloudItem.Account:
						break;
					case CloudItem.Content:
						await mediaContentViewModel.OnBackButtonPressed();
						break;
					case CloudItem.Library:
						break;
					case CloudItem.Notifications:
						break;
					case CloudItem.Queue:
						break;
				};

				//Calling this is kind of hacky but it does not hurt
				await registerViewModel.OnBackButtonPressed();
			};
		}

		public ICommand SelectItem { protected set; get; }

		public ICommand GoHome { protected set; get; }

		public ICommand OpenUrl { protected set; get; }

		public ICommand NavigateToRecording { protected set; get; }

		private async void handleNotificationReceived(object sender, AppNotificationMessage message)
		{
			try
			{
				LoggerService.Instance.Log("Cloud.handleNotificationReceived: Start notification processing");

				if (accountViewModel == null)
				{
					LoggerService.Instance.Log("Cloud.handleNotificationReceived: Account is null. Exiting");
					return;
				}

				if (!accountViewModel.SignedIn)
				{
					LoggerService.Instance.Log("Cloud.handleNotificationReceived: Account not logged in. Trying to log in.");

					var stored = accountViewModel.LoadUserFromCredentials();
					if (stored == null)
					{
						LoggerService.Instance.Log("Cloud.handleNotificationReceived: Account stored credits are null.");
						return;
					}

					if (string.IsNullOrEmpty(stored.AuthToken))
					{
						LoggerService.Instance.Log("Cloud.handleNotificationReceived: Account AuthToken is null.");
						return;
					}

					var error = await accountViewModel.SignInWithTokenAsync(stored, false);
					if (!string.IsNullOrEmpty(error))
						LoggerService.Instance.Log("ERROR: Cloud.handleNotificationReceived: SignIn Error: " + error);

					LoggerService.Instance.Log("Cloud.handleNotificationReceived: SignIn success");
				}

				switch (message.Type)
				{
					case AppNotificationType.DownloadComplete:
					case AppNotificationType.RecordingReady:
					case AppNotificationType.DownloadFailed:
						await handleLocalRecordingsMessage(message);
						break;

					case AppNotificationType.RecordingFailed:
					case AppNotificationType.DownloadExpiring:
						await handleNotificationMessage(message);
						break;

					case AppNotificationType.RemoteNotification:
						LoggerService.Instance.Log("Cloud.handleNotificationReceived: Loading Notification item");
						var notification = await NotificationClient.Get(message.ID, RemoteNotificationsService.Instance.GetTokenType(), RemoteNotificationsService.Instance.GetTokenValue());
						if (notification == null)
						{
							LoggerService.Instance.Log("ERROR: Cloud.handleNotificationReceived: Cannot get notification " + message.ID + " from server");
							return;
						}

						notification.LibraryItem = new LibraryItem()
						{
							ID = notification.RecordingID,
							Title = notification.RecordQueueItem != null ? notification.RecordQueueItem.Title : string.Empty,
							Series = notification.RecordQueueItem != null ? notification.RecordQueueItem.Series : string.Empty,
						};

						await handleRemoteNotification(notification);
						break;
				}
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("Error while processing notification: " + ex);
			}
			finally
			{
				message.WaitHandle?.SetResult(true);
			}
		}

		private async Task handleRemoteNotification(Notification notification)
		{
			if (notification.RecordQueueItem == null)
			{
				LoggerService.Instance.Log("ERROR: Cloud.handleRemoteNotification: notification.RecordQueueItem is null");
				return;
			}

			LoggerService.Instance.Log("Cloud.handleRemoteNotification: notification.type: " + notification.Type.ToString());
			switch (notification.Type)
			{
				case NotificationType.NewRecording:
					if (!libraryViewModel.AutoDownload)
					{
						var message = new AppNotificationMessage()
						{
							ID = notification.LibraryItem?.ID,
							Text = "Your recording \"" + notification.RecordQueueItem.FullTitle + "\" is ready to download!",
							Type = AppNotificationType.RecordingReady
						};

						await RemoteNotificationsService.Instance.TriggerLocalNotification(message);
						LoggerService.Instance.Log("Local notification triggered");
					}
					else
					{
						LoggerService.Instance.Log("Local notification not triggered because autodownload is enabled");
						if (libraryViewModel == null)
						{
							LoggerService.Instance.Log("ERROR: Cloud.handleRemoteNotification: libraryViewModel is null");
							return;
						}

						if (notification.LibraryItem != null)
						{
							if (!LocalLibraryService.Instance.IsItemAddedForDownload(notification.LibraryItem.ID, accountViewModel.UserInfo.Email))
							{
								LocalLibraryService.Instance.CreateCloudItemId(notification.LibraryItem.ID, accountViewModel.UserInfo.Email);
								await libraryViewModel.DownloadItem(notification.LibraryItem, false);
							}
							else
								LoggerService.Instance.Log("Cloud.handleRemoteNotification: item is already downloading/downloaded or has been ignored before enabling auto download.");
						}
					}
					return;

				case NotificationType.FailedRecording:
				case NotificationType.BrowsingError:
				case NotificationType.RecordingIssue:
					{
						var message = new AppNotificationMessage()
						{
							ID = notification.ID,
							Text = "Your recording \"" + notification.RecordQueueItem.FullTitle + "\" has failed!",
							Type = AppNotificationType.RecordingFailed
						};

						await RemoteNotificationsService.Instance.TriggerLocalNotification(message);
						LoggerService.Instance.Log("Local notification triggered");
					}
					return;

				case NotificationType.DownloadExpiring:
					{
						var message = new AppNotificationMessage()
						{
							ID = notification.ID,
							Text = "Your recording \"" + notification.RecordQueueItem.FullTitle + "\"  is about to expire soon!",
							Type = AppNotificationType.DownloadExpiring
						};

						await RemoteNotificationsService.Instance.TriggerLocalNotification(message);
						LoggerService.Instance.Log("Local notification triggered");
					}
					return;
				default:
					LoggerService.Instance.Log("Unrecognized notification type");
					return;
			}
		}

		private async Task handleLocalRecordingsMessage(AppNotificationMessage message)
		{
			LoggerService.Instance.Log("Cloud.handleLocalRecordingsMessage: notification received pulling the recordings");
			await libraryViewModel.Poll(false);
			LoggerService.Instance.Log("Cloud.handleLocalRecordingsMessage: pull completed");
			SelectedItem = CloudItem.Library;
			switch (message.Type)
			{
				case AppNotificationType.DownloadComplete:
					libraryViewModel.SelectedView = (int)LibraryViewMode.Device;
					var localItems = libraryViewModel.LocalItems.ToList();
					var localItem = localItems.FirstOrDefault(c => c.ID == message.ID);
					libraryViewModel.SelectedLibraryItem = localItem;
					if (localItem != null)
						libraryViewModel.ListViewItemSelected.Execute(localItem);

					break;

				case AppNotificationType.RecordingReady:
				case AppNotificationType.DownloadFailed:
					libraryViewModel.SelectedView = (int)LibraryViewMode.Cloud;
					var cloudItems = libraryViewModel.CloudItems.ToList();
					var cloudItem = cloudItems.FirstOrDefault(c => c.ID == message.ID);
					libraryViewModel.SelectedCloudItem = cloudItem;
					if (cloudItem != null)
						libraryViewModel.ListViewItemSelected.Execute(cloudItem);
					else
						LoggerService.Instance.Log("ERROR: Cloud.handleLocalRecordingsMessage: cannot find cloud item: " + message.ID);

					if (!libraryViewModel.AutoDownload && (cloudItem != null) && (message.Type == AppNotificationType.RecordingReady))
					{
						//XXX: Prevent application hang at loading screen
						Device.StartTimer(TimeSpan.FromSeconds(2), () =>
						{
							Task.Factory.StartNew(() =>
							{
								Device.BeginInvokeOnMainThread(async () =>
								{
									if (await Application.Current.MainPage.DisplayAlert("Download", "Do you want to download " + cloudItem.FullTitle + " to your device?", "Yes", "No"))
										await libraryViewModel.DownloadItem(cloudItem, true);
								});
							});

							return false;
						});
					}
					break;
			}
		}

		private async Task handleNotificationMessage(AppNotificationMessage message)
		{
			LoggerService.Instance.Log("Cloud.handleNotificationMessage: notification received pulling messages");
			await notificationsViewModel.Poll(false);
			LoggerService.Instance.Log("Cloud.handleNotificationMessage: pull completed");
			SelectedItem = CloudItem.Notifications;
			if (notificationsViewModel.NotificationItems != null)
			{
				var items = notificationsViewModel.NotificationItems.ToList();
				var item = items.FirstOrDefault(c => c.ID == message.ID);
				notificationsViewModel.SelectedItem = item;
				if (item != null)
					notificationsViewModel.ListViewItemSelected.Execute(item);
			}
		}

		private async void AppNavigation_AppNavigate(object sender, AppNavigationArgs e)
		{
			if (!accountViewModel.SignedIn)
			{
				accountViewModel.OnSignInComplete += async (s, arg) => { await performNavigation(e); };
				return;
			}

			await performNavigation(e);
		}

		private async Task performNavigation(AppNavigationArgs e)
		{
			if ((e == null) || !accountViewModel.SignedIn)
				return;

			try
			{
				switch (e.Page)
				{
					case CloudItem.Content:
						SelectedItem = CloudItem.Content;
						if (!mediaContentViewModel.Initialized)
							await mediaContentViewModel.Init();

						await mediaContentViewModel.TaskQueue.WaitAllTasks();
						if (mediaContentViewModel.Channels != null)
						{
							var channels = mediaContentViewModel.Channels.ToList();
							var provider = channels.FirstOrDefault(c => c.ID == e.ProviderID);
							if (provider != null)
								await mediaContentViewModel.NavigateToDeepLink(provider, e.Token);

							if ((mediaContentViewModel.SelectedFolder != null) &&
								(mediaContentViewModel.SelectedFolder.Children != null) &&
								(mediaContentViewModel.SelectedFolder.Children.Count == 1) &&
								!mediaContentViewModel.SelectedFolder.Children[0].IsRoot &&
								!mediaContentViewModel.SelectedFolder.Children[0].IsChannel &&
								!mediaContentViewModel.SelectedFolder.Children[0].IsFolder)
							{
								mediaContentViewModel.SelectedItem = mediaContentViewModel.SelectedFolder.Children[0];
								mediaContentViewModel.ListViewItemSelected.Execute(mediaContentViewModel.SelectedItem);
							}
						}
						break;

					case CloudItem.Library:
						libraryViewModel.SelectedView = (int)LibraryViewMode.Cloud;
						await libraryViewModel.Poll(true);
						if (libraryViewModel.CloudItems == null)
							return;

						SelectedItem = CloudItem.Library;
						libraryViewModel.SelectedCloudItem = libraryViewModel.CloudItems.FirstOrDefault(n => n.ID == e.ItemID);

						if (libraryViewModel.SelectedCloudItem != null)
							libraryViewModel.ListViewItemSelected.Execute(libraryViewModel.SelectedCloudItem);
						else
							LoggerService.Instance.Log("Cloud.performNavigation: LibraryItem is null");

						break;

					case CloudItem.Notifications:
						await notificationsViewModel.Poll(true);
						if (notificationsViewModel.NotificationItems == null)
							return;

						SelectedItem = CloudItem.Notifications;
						notificationsViewModel.SelectedItem = notificationsViewModel.NotificationItems.FirstOrDefault(n => n.ID == e.ItemID);

						if (notificationsViewModel.SelectedItem != null)
							notificationsViewModel.ListViewItemSelected.Execute(notificationsViewModel.SelectedItem);
						else
							LoggerService.Instance.Log("Cloud.performNavigation: Notification item is null");

						break;

					case CloudItem.Queue:
						await queueViewModel.Poll(true);
						if (queueViewModel.Items == null)
							return;

						SelectedItem = CloudItem.Queue;
						queueViewModel.SelectedItem = queueViewModel.Items.FirstOrDefault(n => n.ID == e.ItemID);

						if (queueViewModel.SelectedItem != null)
							queueViewModel.ListViewItemSelected.Execute(queueViewModel.SelectedItem);
						else
							LoggerService.Instance.Log("Cloud.performNavigation: Queue item is null");

						break;
				}
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: Cloud.performNavigation: " + ex);
			}
		}

		public CloudItem SelectedItem
		{
			get { return selectedItem; }
			set
			{
				if ((CurrentNetworkStatus == NetworkStatus.NotReachable) && (value != CloudItem.Library))
				{
					MessagingCenterManager.ShowPopup(new OfflinePopup());
					return;
				}

				if (value == CloudItem.Content)
				{
					if (!mediaContentViewModel.Initialized)
						TaskQueue.Enqueue(() => mediaContentViewModel.Init());
					else
					{
						if (selectedItem == CloudItem.Content)
						{
							if (!((Application.Current.MainPage as NavigationPage).CurrentPage is PlayOnCloud.Register))
								mediaContentViewModel.GoHome();
						}
						else if (mediaContentViewModel.SelectedItem == mediaContentViewModel.Root)
							TaskQueue.Enqueue(() => mediaContentViewModel.RefreshSelectedFolder());
					}
				}

				var previousValue = selectedItem;

				if (SetField(ref selectedItem, value))
				{
					switch (selectedItem)
					{
						case CloudItem.Content:
							if (!mediaContentViewModel.Initialized)
								TaskQueue.Enqueue(() => mediaContentViewModel.Init());
							break;

						case CloudItem.Library:
							if (!libraryViewModel.Initialized)
								TaskQueue.Enqueue(() => libraryViewModel.Init());
							break;

						case CloudItem.Queue:
							if (!queueViewModel.Initialized)
								TaskQueue.Enqueue(() => queueViewModel.Init());

							queueViewModel.NewItemsCount = 0;
							break;

						case CloudItem.Notifications:
							if (!notificationsViewModel.Initialized)
								TaskQueue.Enqueue(() => notificationsViewModel.Init());
							break;

						case CloudItem.Account:
							if (!accountViewModel.Initialized)
								TaskQueue.Enqueue(() => accountViewModel.Init());

							TaskQueue.Enqueue(() => accountViewModel.FetchUserCreditsAsync());
							break;
					}
				}
			}
		}

		public Register Register
		{
			get { return registerViewModel; }
			set { SetField(ref registerViewModel, value); }
		}

		public MediaContent MediaContent
		{
			get { return mediaContentViewModel; }
			set { SetField(ref mediaContentViewModel, value); }
		}

		public Account Account
		{
			get { return accountViewModel; }
			set { SetField(ref accountViewModel, value); }
		}

		public Library Library
		{
			get { return libraryViewModel; }
			set { SetField(ref libraryViewModel, value); }
		}

		public Queue Queue
		{
			get { return queueViewModel; }
			set { SetField(ref queueViewModel, value); }
		}

		public Notifications Notifications
		{
			get { return notificationsViewModel; }
			set { SetField(ref notificationsViewModel, value); }
		}

		public Products Products
		{
			get { return productsViewModel; }
			set { SetField(ref productsViewModel, value); }
		}

		public override DeviceOrientation DeviceOrientation
		{
			get { return base.DeviceOrientation; }
			set
			{
				base.DeviceOrientation = value;
				registerViewModel.DeviceOrientation = value;
				mediaContentViewModel.DeviceOrientation = value;
				accountViewModel.DeviceOrientation = value;
				libraryViewModel.DeviceOrientation = value;
				queueViewModel.DeviceOrientation = value;
				notificationsViewModel.DeviceOrientation = value;
				productsViewModel.DeviceOrientation = value;
			}
		}

		public override async Task Poll(bool showLoading)
		{
			if (CurrentNetworkStatus == NetworkStatus.NotReachable)
			{
				await RestService.Instance.CheckIfConnectionRestored();
				return;
			}

			if (mediaContentViewModel.Initialized)
			{
				if ((mediaContentViewModel.Channels == null) || !mediaContentViewModel.Channels.Any())
					await mediaContentViewModel.RefreshSelectedFolder();
				else if (SelectedItem == CloudItem.Content)
					await mediaContentViewModel.Poll(showLoading);
			}

			if (!Account.SignedIn)
				return;

			if (libraryViewModel.Initialized)
				await libraryViewModel.Poll(showLoading);
			else if (libraryViewModel.AutoDownload)
				await libraryViewModel.Init();

			if (!notificationsViewModel.Initialized)
				await notificationsViewModel.Init();
			else
				await notificationsViewModel.Poll(showLoading);

			if (!queueViewModel.Initialized)
				await queueViewModel.Init();
			else
				await queueViewModel.Poll(showLoading);
		}

		public override async Task Logout()
		{
			await accountViewModel.Logout();
			await libraryViewModel.Logout();
			await queueViewModel.Logout();
			await notificationsViewModel.Logout();
			await productsViewModel.Logout();
			await mediaContentViewModel.Logout();

			await base.Logout();
		}

		private async Task navigateToRecordingFromNotification(Notification notification)
		{
			if ((notification == null) || (notification.Type == NotificationType.BrowsingError) || (notification.Type == NotificationType.FailedRecording))
				return;

			await libraryViewModel.Poll(false);

			if (libraryViewModel.CloudItems != null)
			{
				var recording = libraryViewModel.CloudItems.FirstOrDefault(p => p.ID == notification.RecordingID);
				if (recording != null)
				{
					LibraryItem prevCloudItem = libraryViewModel.SelectedCloudItem;
					CloudItem prevSelectedItem = SelectedItem;

					SelectedItem = CloudItem.Library;
					libraryViewModel.SelectedView = (int)LibraryViewMode.Cloud;
					libraryViewModel.SelectedCloudItem = recording;

					if (!recording.Equals(prevCloudItem) || (prevSelectedItem != CloudItem.Library))
						Library.ListViewItemSelected.Execute(null);
				}
			}
		}

		private async void ReachabilityHelper_NetworkStatusChanged(object sender, NetworkStatus e)
		{
			await NetworkStatusChanged(e);
		}

		public override async Task NetworkStatusChanged(NetworkStatus newStatus)
		{
			if ((CurrentNetworkStatus == NetworkStatus.NotReachable) && (newStatus != NetworkStatus.NotReachable))
			{
				var restored = await RestService.Instance.CheckIfConnectionRestored();
				if (!restored)
					return;
			}

			var current = CurrentNetworkStatus;
			await base.NetworkStatusChanged(newStatus);

			if (current != newStatus)
			{
				await accountViewModel.NetworkStatusChanged(newStatus);
				await libraryViewModel.NetworkStatusChanged(newStatus);
				await mediaContentViewModel.NetworkStatusChanged(newStatus);
				await notificationsViewModel.NetworkStatusChanged(newStatus);
				await queueViewModel.NetworkStatusChanged(newStatus);

				if ((current != NetworkStatus.NotReachable) && (newStatus == NetworkStatus.NotReachable))
				{
					libraryViewModel.SelectedView = (int)LibraryViewMode.Device;
					SelectedItem = CloudItem.Library;
				}
			}
		}

		public void SetIsInBackground(bool isInBackground)
		{
			IsInBackground = isInBackground;

			registerViewModel.IsInBackground = isInBackground;
			accountViewModel.IsInBackground = isInBackground;
			libraryViewModel.IsInBackground = isInBackground;
			queueViewModel.IsInBackground = isInBackground;
			notificationsViewModel.IsInBackground = isInBackground;
			productsViewModel.IsInBackground = isInBackground;
			mediaContentViewModel.IsInBackground = isInBackground;
		}

		private void MediaContentViewModel_OnSelectedItemDetailsChanged(object sender, EventArgs e)
		{
			var selected = mediaContentViewModel.SelectedItem;
			if ((selected != null) && (SelectedItem == CloudItem.Content) && (DeviceOrientation == DeviceOrientation.Portrait) && !selected.IsFolder)
			{
				DetailsPage details = new DetailsPage() { BindingContext = this };
				details.InsertChild(new ContentItemDetailsView());
				Application.Current.MainPage.Navigation.PushAsync(details);
			}
		}

		private void LibraryViewModel_OnSelectedItemDetailsChanged(object sender, EventArgs e)
		{
			var selected = libraryViewModel.SelectedItem;
			if ((selected != null) && (SelectedItem == CloudItem.Library) && (DeviceOrientation == DeviceOrientation.Portrait))
			{
				DetailsPage details = new DetailsPage() { BindingContext = this };
				details.InsertChild(new LibraryItemDetails() { BindingContext = libraryViewModel });
				Application.Current.MainPage.Navigation.PushAsync(details);
			}
            
        }

		private void QueueViewModel_OnSelectedItemDetailsChanged(object sender, EventArgs e)
		{
			var selected = queueViewModel.SelectedItem;
			if ((selected != null) && (SelectedItem == CloudItem.Queue) && (DeviceOrientation == DeviceOrientation.Portrait))
			{
				DetailsPage details = new DetailsPage() { BindingContext = this };
				details.InsertChild(new QueueItemDetails() { BindingContext = queueViewModel });
				Application.Current.MainPage.Navigation.PushAsync(details);
			}
		}

		private void NotificationsViewModel_OnSelectedItemDetailsChanged(object sender, EventArgs e)
		{
			var selected = notificationsViewModel.SelectedItem;
			if ((selected != null) && (SelectedItem == CloudItem.Notifications) && (DeviceOrientation == DeviceOrientation.Portrait))
			{
				DetailsPage details = new DetailsPage() { BindingContext = this };
				details.InsertChild(new NotificationItemDetails());
				Application.Current.MainPage.Navigation.PushAsync(details);
			}
		}
	}
}
