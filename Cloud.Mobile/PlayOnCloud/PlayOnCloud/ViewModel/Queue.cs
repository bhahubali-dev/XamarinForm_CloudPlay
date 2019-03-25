using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud.ViewModel
{
	public class Queue : ViewModelBase
	{
		private static byte[] pngHeaderBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
		private static byte[] jpgHeaderBytes = new byte[] { 0xFF, 0xD8 };

		private RecordQueueItem selectedItem;
		private ObservableCollection<RecordQueueItem> items;
		private RecordQueueItem activeRecording;
		private Account accountViewModel;
		private MediaContent mediaContentViewModel;
		private Cloud cloudViewModel;
		private int newItemsCount = 0;

		public Queue()
		{
			Delete = new Command<RecordQueueItem>(async (parameter) => await delete(parameter));
			Move = new Command<List<int>>(async (p) =>
			{
				if ((p is List<int>) && (p as List<int>).Count > 1)
					await move((p as List<int>)[0], (p as List<int>)[1]);
			});

			MoveUp = new Command<RecordQueueItem>(
				async (p) => await moveUp(p),
				(p) => { return (p != null) && (items.IndexOf(p) > 0); });

			MoveDown = new Command<RecordQueueItem>(
				async (p) => await moveDown(p),
				(p) => { return (p != null) && (items.IndexOf(p) < (items.Count - 1)); });

			MoveToTop = new Command<RecordQueueItem>(
				async (p) => await moveToTop(p),
				(p) => { return (p != null) && (items.IndexOf(p) > 0); });

			MoveToBottom = new Command<RecordQueueItem>(
				async (p) => await moveToBottom(p),
				(p) => { return (p != null) && (items.IndexOf(p) < (items.Count - 1)); });

			SelectActiveRecording = new Command(() =>
			{
				SelectedItem = ActiveRecording;
				Fire_OnSelectedItemDetailsChanged();
			});
		}

		public Command Delete { protected set; get; }

		public Command Move { protected set; get; }

		public Command MoveUp { protected set; get; }

		public Command MoveDown { protected set; get; }

		public Command MoveToTop { protected set; get; }

		public Command MoveToBottom { protected set; get; }

		public Command SelectActiveRecording { protected set; get; }

		public Account Account
		{
			get { return accountViewModel; }
			set { SetField(ref accountViewModel, value); }
		}

		public MediaContent MediaContent
		{
			get { return mediaContentViewModel; }
			set { SetField(ref mediaContentViewModel, value); }
		}

		public Cloud Cloud
		{
			get { return cloudViewModel; }
			set { SetField(ref cloudViewModel, value); }
		}

		public override async Task Init()
		{
			if (CurrentNetworkStatus == NetworkStatus.NotReachable)
				return;

			await loadItems();
			await base.Init();
		}

		public async Task Update()
		{
			await loadItems();
		}

		private async Task loadItems()
		{
			IsLoading = true;

			try
			{
				await updateItems();
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Queue.loadItems: " + ex);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task updateItems()
		{
			var collection = await QueueClient.Get();
			if ((collection != null) && (collection.Entries != null))
			{
				var active = collection.Entries.FirstOrDefault(q => q.RecordingStatus == RecordingStatus.Started);
				if ((active == null) && collection.Entries.Any())
					active = collection.Entries.OrderBy(r => r.Created).FirstOrDefault();

				var serverCollection = collection.Entries.ToList();
				if ((active != null) && serverCollection.Contains(active))
					serverCollection.Remove(active);

				if (Items == null)
					Items = new ObservableCollection<RecordQueueItem>();

				int previousItemsCount = Items.Count;
				if (activeRecording != null)
					previousItemsCount++;

				var newItems = await Items.UpdateItems(serverCollection.Where(q => q.RecordingStatus != RecordingStatus.Started).OrderBy(q => q.Rank));

				if (((active == null) || (activeRecording == null)) || (activeRecording.ID != active.ID))
					ActiveRecording = active;

				if (ActiveRecording != null)
					await loadStatusAsync(ActiveRecording);

				var items = Items.ToList();
				if ((SelectedItem != null) && !items.Any(r => r.ID == SelectedItem.ID) && (SelectedItem != ActiveRecording))
					SelectedItem = null;

				if (!collection.Entries.Any())
					NewItemsCount = 0;
				else if ((previousItemsCount < collection.Entries.Count()) && (cloudViewModel.SelectedItem != CloudItem.Queue))
					NewItemsCount = collection.Entries.Count();

				OnPropertyChanged(nameof(ItemsCount));
			}
		}

		private async void loadDetails(RecordQueueItem item)
		{
			await loadDetailsAsync(item);
		}

		private async Task loadDetailsAsync(RecordQueueItem item)
		{
			try
			{
				var details = await QueueClient.Get(item.ID);
				if (details != null)
					item.UpdateFromDetails(details);
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Queue.loadDetailsAsync: " + ex);
			}
		}

		private async Task loadStatusAsync(RecordQueueItem item)
		{
			try
			{
				item.RecordingProgress = await QueueClient.GetStatus(item.ID);
				if (item.RecordingProgress != null)
					item.RecordingStatus = item.RecordingProgress.Status;
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Queue.loadStatusAsync: " + ex);
			}
		}

		public ObservableCollection<RecordQueueItem> Items
		{
			get { return items; }
			set { SetField(ref items, value); }
		}

		public int ItemsCount
		{
			get { return (items != null) ? items.Count : 0; }
		}

		public int NewItemsCount
		{
			get { return newItemsCount; }
			set { SetField(ref newItemsCount, value); }
		}

		public RecordQueueItem SelectedItem
		{
			get { return selectedItem; }
			set
			{
				if (SetField(ref selectedItem, value) && (selectedItem != null))
					loadDetails(selectedItem);
			}
		}

		public RecordQueueItem ActiveRecording
		{
			get { return activeRecording; }
			set { SetField(ref activeRecording, value); }
		}

		private async Task delete(RecordQueueItem item)
		{
			if (await Application.Current.MainPage.DisplayAlert("Delete Recording", "Are you sure you want to delete the selected video?", "Yes", "No"))
			{
				try
				{
					var response = await QueueClient.Delete(item.ID);
					if ((response != null) && response.Success)
					{
						Items.Remove(item);
						OnPropertyChanged(nameof(ItemsCount));

						if (item.Equals(SelectedItem))
							SelectedItem = null;

						await accountViewModel.FetchUserCreditsAsync();
						await Application.Current.MainPage.DisplayAlert("Recording Deleted", "The video has been successfully deleted.", "OK");
					}
					else if (response != null)
						await Application.Current.MainPage.DisplayAlert("Delete Recording", response.ErrorMessageClean, "OK");
				}
				catch (Exception ex)
				{
					//XXX : Handle error
					LoggerService.Instance.Log("ERROR: Queue.delete: " + ex);
				}
			}
		}

		private async Task move(int sourceIndex, int destinationIndex)
		{
			try
			{
				var sourceItem = Items[sourceIndex];
				var destinationItem = Items[destinationIndex];
				var desiredRank = destinationItem.Rank;
				if (destinationIndex > sourceIndex)
					desiredRank++;

				Items.Move(sourceIndex, destinationIndex);
				var response = await QueueClient.SetRank(sourceItem.ID, desiredRank);
				if ((response == null) || !response.Success)
					await Application.Current.MainPage.DisplayAlert("Error", response != null ? response.ErrorMessageClean : "Unable to move the selected video.", "OK");

				MoveUp.ChangeCanExecute();
				MoveDown.ChangeCanExecute();
				MoveToTop.ChangeCanExecute();
				MoveToBottom.ChangeCanExecute();
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Queue.move: " + ex);
			}
		}

		private async Task moveUp(RecordQueueItem item)
		{
			try
			{
				var sourceIndex = Items.IndexOf(item);
				var destinationIndex = sourceIndex - 1;
				await move(sourceIndex, destinationIndex);
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Queue.moveUp: " + ex);
			}
		}

		private async Task moveDown(RecordQueueItem item)
		{
			try
			{
				var sourceIndex = Items.IndexOf(item);
				var destinationIndex = sourceIndex + 1;
				await move(sourceIndex, destinationIndex);
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Queue.moveDown: " + ex);
			}
		}

		private async Task moveToTop(RecordQueueItem item)
		{
			try
			{
				var sourceIndex = Items.IndexOf(item);
				var destinationIndex = 0;
				await move(sourceIndex, destinationIndex);
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Queue.moveToTop: " + ex);
			}
		}

		private async Task moveToBottom(RecordQueueItem item)
		{
			try
			{
				var sourceIndex = Items.IndexOf(item);
				var destinationIndex = Items.Count - 1;
				await move(sourceIndex, destinationIndex);
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Queue.moveToBottom: " + ex);
			}
		}

		public async Task AddToQueue(ContentItemEx item)
		{
			Mark mark = MarkManager.GetMark();
			if (mark.HasFlag(Mark.Hulu) || mark.HasFlag(Mark.Netflix))
			{
				await Application.Current.MainPage.DisplayAlert("Error", "We're sorry but there is currently an issue with the PlayOn Cloud service. Please try again later.", "OK");
				return;
			}

			if (!SharedSettingsService.Instance.GetBoolValue("IsLegalDisclaimerAccepted"))
			{
				if (await MessagingCenterManager.ShowPopup(new LegalDisclaimer()))
					SharedSettingsService.Instance.SetBoolValue("IsLegalDisclaimerAccepted", true);
				else
					return;
			}

			if (await addToQueue(item))
				await Update();
			else
				await Application.Current.MainPage.DisplayAlert("Queue Error", "There was an error queueing the video for recording.", "OK");

			await accountViewModel.FetchUserCreditsAsync();
		}

		private void registerForNotifications()
		{
			//Ask the user for allowing notifications
			Device.BeginInvokeOnMainThread(() =>
			{
				if (RemoteNotificationsService.Instance.CanRegisterForNotifications() && !RemoteNotificationsService.Instance.IsRegisteredForNotifications())
					RemoteNotificationsService.Instance.RegisterForNotifications();
			});
		}

		private async Task<bool> addToQueue(ContentItemEx item)
		{
			try
			{
				registerForNotifications();

				if (mediaContentViewModel != null)
					mediaContentViewModel.IsDetailsLoading = true;

				var recordToken = await ContentClient.GetRecordToken(item.ID);

				var smallThumbUrl = ContentClient.GetSmallImageUrl(item.ID, 192, 128);
				var smallThumb = await ImageToolsService.Instance.GetImageFromCache(smallThumbUrl);
				if (smallThumb == null)
					smallThumb = await ContentClient.GetImageFromUrl(smallThumbUrl);

				var largeThumbUrl = ContentClient.GetLargeImageUrl(item.ID, 720, 480);
				var largeThumb = await ImageToolsService.Instance.GetImageFromCache(largeThumbUrl);
				if (largeThumb == null)
					largeThumb = await ContentClient.GetImageFromUrl(largeThumbUrl);

				var isSmallThumbValid = isValidImage(smallThumb);
				var isLargeThumbValid = isValidImage(largeThumb);
				if (!isSmallThumbValid || !isLargeThumbValid)
				{
					var channelThumbUrl = item.ChannelThumbnailUrl;
					if (!string.IsNullOrEmpty(channelThumbUrl))
					{
						var channelThumb = await ContentClient.GetImageFromUrl(channelThumbUrl);

						if (!isSmallThumbValid)
							smallThumb = channelThumb;

						if (!isLargeThumbValid)
							largeThumb = channelThumb;
					}
				}

				var response = await QueueClient.Add(recordToken, smallThumb, largeThumb);
				if ((response != null) && response.Success)
				{
					item.InQueue = true;
					if (mediaContentViewModel.SelectedItem is ContentItemEx)
						(mediaContentViewModel.SelectedItem as ContentItemEx).InQueue = true;

					FacebookToolsService.Instance.LogCustomEvent("AddToQueue");

					NewItemsCount++;
					return true;
				}

				return false;
			}
			finally
			{
				if (mediaContentViewModel != null)
					mediaContentViewModel.IsDetailsLoading = false;
			}
		}

		private bool isValidImage(byte[] bytes)
		{
			try
			{
				if ((bytes == null) || (bytes.Length == 0))
					return false;

				if ((bytes.Length > pngHeaderBytes.Length) && pngHeaderBytes.SequenceEqual(bytes.Take(pngHeaderBytes.Length)))
					return true;

				if ((bytes.Length > jpgHeaderBytes.Length) && jpgHeaderBytes.SequenceEqual(bytes.Take(jpgHeaderBytes.Length)))
					return true;
			}
			catch
			{
			}

			return false;
		}

		public override async Task Poll(bool showLoading)
		{
			try
			{
				if (showLoading)
					IsLoading = true;

				await updateItems();
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Queue.Poll: " + ex);
			}
			finally
			{
				if (showLoading)
					IsLoading = false;
			}
		}

		public override async Task Logout()
		{
			ActiveRecording = null;
			if (Items != null)
			{
				Items.Clear();
				OnPropertyChanged(nameof(ItemsCount));
			}

			SelectedItem = null;

			await base.Logout();
		}

		protected override async Task refresh(bool silent)
		{
			try
			{
				await updateItems();
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: Queue.refresh: " + ex);
			}

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
