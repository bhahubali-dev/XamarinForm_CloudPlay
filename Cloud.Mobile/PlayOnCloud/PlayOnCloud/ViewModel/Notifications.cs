using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud.ViewModel
{
	public class Notifications : ViewModelBase
	{
		private ObservableCollection<Notification> items;
		private Notification selectedItem;
		private bool failedRecordingTipVisible;
		private Cloud cloudViewModel;
		private object syncRoot = new object();

		public Notifications(Cloud cloudViewModel)
		{
			this.cloudViewModel = cloudViewModel;
			Delete = new Command<Notification>(async (parameter) => await delete(parameter));
			DeleteAll = new Command(async () => await deleteAll());
			ShowFailedRecordingTip = new Command<bool>((parameter) =>
			{
				if ((cloudViewModel != null) && (cloudViewModel.DeviceOrientation == DeviceOrientation.Portrait))
				{
					if (parameter)
					{
						DetailsPage details = new DetailsPage();
						details.InsertChild(new FailedRecordingTip());
						Application.Current.MainPage.Navigation.PushAsync(details);
					}
					else
						Application.Current.MainPage.Navigation.PopAsync();
				}
				else
					FailedRecordingTipVisible = parameter;
			});
		}

		public ICommand Delete { protected set; get; }

		public ICommand DeleteAll { protected set; get; }

		public ICommand ShowFailedRecordingTip { protected set; get; }

		public override async Task Init()
		{
			if (CurrentNetworkStatus == NetworkStatus.NotReachable)
				return;

			await LoadItems();
			await base.Init();
		}

		public async Task LoadItems()
		{
			IsLoading = true;

			try
			{
				await loadItems();
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task loadItems()
		{
			var collection = await NotificationClient.Get();
			if ((collection != null) && (collection.Entries != null))
			{
				if (NotificationItems == null)
					NotificationItems = new ObservableCollection<Notification>();

				await NotificationItems.UpdateItems(collection.Entries.OrderByDescending(q => q.Created));
				await NotificationItems.SortByDate();

				OnPropertyChanged(nameof(NotificationItemsCount));
				OnPropertyChanged(nameof(UnreadNotificationsCount));
			}
		}

		public ObservableCollection<Notification> NotificationItems
		{
			get { return items; }
			set { SetField(ref items, value); }
		}

		public int NotificationItemsCount
		{
			get { return (items != null) ? items.Count : 0; }
		}

		public int UnreadNotificationsCount
		{
			get
			{
				if (NotificationItems != null)
					return NotificationItems.Where(n => n.Status == NotificationStatus.Unread).Count();

				return 0;
			}
		}

		public Notification SelectedItem
		{
			get { return selectedItem; }
			set
			{
				if (SetField(ref selectedItem, value) && (selectedItem != null))
				{
					loadDetails(selectedItem);
					setNotificationStatus(selectedItem, NotificationStatus.Read);
					FailedRecordingTipVisible = false;
				}
			}
		}

		public bool FailedRecordingTipVisible
		{
			get { return failedRecordingTipVisible; }
			set { SetField(ref failedRecordingTipVisible, value); }
		}

		private async Task delete(Notification item)
		{
			await setNotificationStatusAsync(item, NotificationStatus.Deleted);
		}

		private async Task deleteAll()
		{
			if (await Application.Current.MainPage.DisplayAlert("Clear", "Are you sure you want to clear all messages?", "Yes", "No"))
			{
				try
				{
					IsLoading = true;
					var items = NotificationItems?.ToList();
					if (items != null)
					{
						for (int i = items.Count - 1; i >= 0; i--)
							await setNotificationStatusAsync(items[i], NotificationStatus.Deleted);
					}
				}
				catch (Exception ex)
				{
					LoggerService.Instance.Log("ERROR: Notifications.deleteAll: " + ex);
				}
				finally
				{
					IsLoading = false;
				}
			}
		}

		private async void loadDetails(Notification item)
		{
			await loadDetailsAsync(item);
		}

		private async Task loadDetailsAsync(Notification item)
		{
			try
			{
				if (item != null)
				{
					var notification = await NotificationClient.Get(item.ID);
					if (notification != null)
						item.RecordQueueItem = notification.RecordQueueItem;
				}
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Notifications.loadDetailsAsync: " + ex);
			}
		}

		private async void setNotificationStatus(Notification item, NotificationStatus status)
		{
			await setNotificationStatusAsync(item, status);
		}

		private async Task setNotificationStatusAsync(Notification item, NotificationStatus status)
		{
			try
			{
				if ((item != null) && (item.Status != status))
				{
					var response = await NotificationClient.SetStatus(item.ID, status);
					if ((response != null) && response.Success)
					{
						item.Status = status;
						if (item.Status == NotificationStatus.Deleted)
						{
							if (NotificationItems != null)
								lock (syncRoot)
									if (NotificationItems != null)
										NotificationItems.Remove(item);

							if (item.Equals(SelectedItem))
								SelectedItem = null;
						}

						OnPropertyChanged(nameof(NotificationItemsCount));
						OnPropertyChanged(nameof(UnreadNotificationsCount));
					}
				}
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Notifications.setNotificationStatusAsync: " + ex);
			}
		}

		public override async Task Logout()
		{
			if (NotificationItems != null)
				lock (syncRoot)
					if (NotificationItems != null)
					{
						NotificationItems.Clear();
						OnPropertyChanged(nameof(NotificationItemsCount));
						OnPropertyChanged(nameof(UnreadNotificationsCount));
					}

			SelectedItem = null;

			await base.Logout();
		}

		public override async Task Poll(bool showLoading)
		{
			try
			{
				if (showLoading)
					IsLoading = true;

				await loadItems();
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Notifications.Poll: " + ex);
			}
			finally
			{
				if (showLoading)
					IsLoading = false;
			}
		}

		protected override async Task refresh(bool silent)
		{
			await loadItems();
			await base.refresh(silent);
		}

		public override async Task NetworkStatusChanged(NetworkStatus newStatus)
		{
			if ((CurrentNetworkStatus == NetworkStatus.NotReachable) && (newStatus != NetworkStatus.NotReachable))
				await Poll(true);

			await base.NetworkStatusChanged(newStatus);
		}
	}
}
