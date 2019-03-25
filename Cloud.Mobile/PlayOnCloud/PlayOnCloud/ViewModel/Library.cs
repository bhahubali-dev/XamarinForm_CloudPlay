using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FFImageLoading;
using FFImageLoading.Cache;
using PlayOnCloud.Model;
using Xamarin.Forms;

namespace PlayOnCloud.ViewModel
{
    public class Library : ViewModelBase
    {
        private readonly IItemDownloader itemDownloader;
        private Account accountViewModel;
        private bool? autoDownload;
        private ObservableCollection<LibraryItem> cloudItems;
        private bool edit;
        private LibrarySort librarySort = LibrarySort.Recent;
        private ObservableCollection<LibraryItem> localItems;
        private LibraryItem selectedCloudItem;
        private LibraryItem selectedItem;
        private LibraryItem selectedLibraryItem;
        private LibraryViewMode selectedView = LibraryViewMode.Cloud;

        public Library(Account accountViewModel)
        {
            this.accountViewModel = accountViewModel;
            Sort = new Command<LibrarySort>(p => LibrarySort = p);
            Manage = new Command(() => manage());
            Download = new Command<LibraryItem>(async p => await DownloadItem(p, true),
                p => { return p != null && !p.IsLocal && p.DownloadStatus != DownloadStatus.Downloading; });
            CancelDownload = new Command<LibraryItem>(p => cancelDownload(p),
                p => { return p != null && !p.IsLocal && p.DownloadStatus == DownloadStatus.Downloading; });
            Play = new Command<LibraryItem>(async p => await play(p),
                p => { return p != null && (p.IsLocal || p.LocalItem != null); });
            Delete = new Command<LibraryItem>(async p =>
                {
                    if (p == null)
                        return;

                    var message = string.Empty;
                    if (p.IsLocal)
                    {
                        message = "Are you sure you want to delete this recording from your device?" +
                                  Environment.NewLine + "It is no longer available to download from the cloud.";
                        LibraryItem cloudRecording = null;
                        if (CloudItems != null)
                            cloudRecording = CloudItems.ToList().FirstOrDefault(c => c.ID == p.ID);

                        if (cloudRecording != null)
                        {
                            var timeLeft = cloudRecording.Expires.ToLocalTime().Subtract(DateTime.Now);
                            message = "Are you sure you want to delete this recording from your device?" +
                                      Environment.NewLine + "You can download it again from the cloud within the next " +
                                      Math.Round(timeLeft.TotalDays) + " days.";
                        }
                    }
                    else
                    {
                        message = "Are you sure you want to delete this recording from the cloud?" + Environment.NewLine +
                                  "You will need to record it again to download and watch on any device.";
                        LibraryItem localRecording = null;
                        if (LocalItems != null)
                            localRecording = LocalItems.ToList().FirstOrDefault(l => l.ID == p.ID);

                        if (localRecording != null)
                            message = "Are you sure you want to delete this recording from the cloud?" +
                                      Environment.NewLine + "It will no longer be available for download on any device.";
                    }

                    if (await Application.Current.MainPage.DisplayAlert("Delete Recording", message, "Yes", "No"))
                        try
                        {
                            CancelDownload?.Execute(p);

                            IsLoading = true;
                            if (!await delete(p))
                                await Application.Current.MainPage.DisplayAlert("Error Deleting",
                                    "Unable to delete the selected recording.", "OK");
                            else
                                OnItemDeleted?.Invoke(this, null);

                            IsLoading = false;
                        }
                        catch (Exception ex)
                        {
                            LoggerService.Instance.Log("ERROR: Library.DeleteCommand: " + ex);
                        }
                },
                p => { return p != null && p.Storage != LibraryItemStorage.iTunes; });

            DownloadChecked = new Command(async () => await downloadChecked(),
                () => { return selectedView == LibraryViewMode.Cloud; });
            ManageDone = new Command(() => manageDone());
            DeleteChecked = new Command(async () =>
            {
                IEnumerable<LibraryItem> items = null;
                if (selectedView == LibraryViewMode.Cloud)
                    items = CloudItems.Where(l => l.Checked);
                else
                    items = LocalItems.Where(l => l.Checked);

                if (items == null || !items.Any())
                {
                    await Application.Current.MainPage.DisplayAlert("Delete Recordings",
                        "Please select one or more recordings to delete.", "OK");
                    return;
                }

                if (await Application.Current.MainPage.DisplayAlert("Delete Recordings",
                    "Are you sure you want to delete the selected recordings?", "Yes", "No"))
                    if (!await deleteMultipleItems(items.ToArray()))
                        await Application.Current.MainPage.DisplayAlert("Error Deleting",
                            "Unable to delete all of the selected recordings.", "OK");

                Edit = false;
            });

            RefreshCloudItems = new Command(async () =>
            {
                Edit = false;
                await getItems(true);
                try
                {
                    var clouds = cloudItems.ToList();
                    foreach (var cloudItem in clouds)
                    {
                        if (!string.IsNullOrEmpty(cloudItem.SmallThumbnailUri))
                            await ImageService.Instance.InvalidateCacheEntryAsync(cloudItem.SmallThumbnailUri,
                                CacheType.All);

                        if (!string.IsNullOrEmpty(cloudItem.LargeThumbnailUri))
                            await ImageService.Instance.InvalidateCacheEntryAsync(cloudItem.LargeThumbnailUri,
                                CacheType.All);

                        cloudItem.RefreshImages();
                    }
                }
                catch (Exception ex)
                {
                    //XXX : Handle error
                    LoggerService.Instance.Log("ERROR: Library.RefreshCloudItems: " + ex);
                }

                IsRefreshing = false;
            });

            RefreshLocalItems = new Command(async () =>
            {
                Edit = false;
                await updateLocalItems();
                IsRefreshing = false;
            });

            itemDownloader = ItemDownloaderService.Instance;
            itemDownloader.DownloadProgress += ItemDownloaderOnDownloadProgress;
            itemDownloader.DownloadComplete += ItemDownloaderOnDownloadComplete;
        }

        public int SelectedViewType { set; get; }
        public Command Sort { protected set; get; }

        public Command Manage { protected set; get; }

        public Command Download { protected set; get; }

        public Command CancelDownload { protected set; get; }

        public Command Play { protected set; get; }

        public Command Delete { protected set; get; }

        public Command DownloadChecked { protected set; get; }

        public Command ManageDone { protected set; get; }

        public Command DeleteChecked { protected set; get; }

        public Command RefreshCloudItems { protected set; get; }
        public Command RefreshLocalItems { protected set; get; }

        public Account Account
        {
            get { return accountViewModel; }
            set { SetField(ref accountViewModel, value); }
        }

        public ObservableCollection<LibraryItem> LocalItems
        {
            get { return localItems; }
            set { SetField(ref localItems, value); }
        }

        public int LocalItemsCount
        {
            get { return localItems != null ? localItems.Count : 0; }
        }

        public ObservableCollection<LibraryItem> CloudItems
        {
            get { return cloudItems; }
            set { SetField(ref cloudItems, value); }
        }

        public int CloudItemsCount
        {
            get { return cloudItems != null ? cloudItems.Count : 0; }
        }

        public LibraryItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (SetField(ref selectedItem, value) && selectedItem != null && !selectedItem.IsLocal)
                    loadDetails(selectedItem);
            }
        }

        public LibraryItem SelectedCloudItem
        {
            get { return selectedCloudItem; }
            set
            {
                if (SetField(ref selectedCloudItem, value))
                    SelectedItem = selectedCloudItem;
            }
        }

        public LibraryItem SelectedLibraryItem
        {
            get { return selectedLibraryItem; }
            set
            {
                if (SetField(ref selectedLibraryItem, value))
                    SelectedItem = selectedLibraryItem;
            }
        }

        public int SelectedView
        {
            get { return (int) selectedView; }
            set
            {
                if (CurrentNetworkStatus == NetworkStatus.NotReachable && value != (int) LibraryViewMode.Device)
                {
                    MessagingCenterManager.ShowPopup(new OfflinePopup());
                    return;
                }

                if (SetField(ref selectedView, (LibraryViewMode) value))
                    switch (selectedView)
                    {
                        case LibraryViewMode.Cloud:
                            SelectedItem = selectedCloudItem;
                            break;

                        case LibraryViewMode.Device:
                            SelectedItem = selectedLibraryItem;
                            break;
                    }
            }
        }

        public LibrarySort LibrarySort
        {
            get { return librarySort; }
            set
            {
#pragma warning disable 4014
                if (SetField(ref librarySort, value))
                    sort(librarySort);
#pragma warning restore 4014
            }
        }

        public bool Edit
        {
            get { return edit; }
            set { SetField(ref edit, value); }
        }

        public bool AutoDownload
        {
            get
            {
                if (!autoDownload.HasValue)
                    autoDownload = SharedSettingsService.Instance.GetBoolValue("AutoDownload");

                return autoDownload.Value;
            }
            set
            {
                if (SetField(ref autoDownload, value))
                    SharedSettingsService.Instance.SetBoolValue("AutoDownload", value);
            }
        }

        public event EventHandler OnItemDeleted;

        public override async Task Init()
        {
            if (CurrentNetworkStatus == NetworkStatus.NotReachable)
            {
                SelectedView = (int) LibraryViewMode.Device;
                await getLocalItems();

                return;
            }

            await Task.Run(async () => await loadItems());
            await base.Init();
        }

        private async Task loadItems()
        {
            IsLoading = true;

            try
            {
                await getItems();
            }
            catch (Exception ex)
            {
                //XXX : Handle error
                LoggerService.Instance.Log("ERROR: Library.loadItems: " + ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task getItems(bool cloudItemsOnly = false)
        {
            if (!cloudItemsOnly)
                await getLocalItems();
            else
                await updateLocalItems();

            var collection = await LibraryClient.Get();
            if (collection?.Entries != null)
            {
                if (CloudItems == null)
                    CloudItems = new ObservableCollection<LibraryItem>();

                var entries = new ObservableCollection<LibraryItem>(collection.Entries);
                switch (librarySort)
                {
                    case LibrarySort.Alphabetical:
                        await entries.SortByTitle();
                        break;

                    case LibrarySort.Recent:
                        await entries.SortByDate();
                        break;
                }

                var newItems = await CloudItems.UpdateItems(entries);
                if (Account != null && Account.SignedIn && Account.UserInfo != null &&
                    !string.IsNullOrEmpty(Account.UserInfo.Email))
                {
                    var cloud = CloudItems.ToList();
                    var email = Account.UserInfo.Email.ToLower();
                    if (!SharedSettingsService.Instance.GetBoolValue("AutoDownloadDBCreated" + email))
                    {
                        SharedSettingsService.Instance.SetBoolValue("AutoDownloadDBCreated" + email, true);
                        foreach (var cloudItem in cloud)
                            LocalLibraryService.Instance.CreateCloudItemId(cloudItem.ID, Account.UserInfo.Email);
                    }
                    else
                    {
                        foreach (var cloudItem in newItems)
                            if (
                                !LocalLibraryService.Instance.IsItemAddedForDownload(cloudItem.ID,
                                    Account.UserInfo.Email))
                            {
                                LocalLibraryService.Instance.CreateCloudItemId(cloudItem.ID, Account.UserInfo.Email);
                                if (AutoDownload)
                                {
                                    LoggerService.Instance.Log("Library.getItems: Found new item for download: ID: " +
                                                               cloudItem.ID);
                                    await DownloadItem(cloudItem, false);
                                }
                            }
                    }
                }
            }

            if (LocalItems != null && CloudItems != null)
            {
                var local = LocalItems.ToList();
                var cloud = CloudItems.ToList();
                foreach (var localItem in local)
                {
                    var cloudItemForLocalItem =
                        cloud.FirstOrDefault(
                            item => item.ID == localItem.ID && localItem.Storage == LibraryItemStorage.AppLocal);
                    if (cloudItemForLocalItem != null)
                    {
                        cloudItemForLocalItem.LocalItem = localItem;
                        if (cloudItemForLocalItem.DownloadStatus != DownloadStatus.Downloading)
                            cloudItemForLocalItem.DownloadStatus = DownloadStatus.Completed;
                    }
                }
            }

            await sort(librarySort);

            OnPropertyChanged(nameof(CloudItemsCount));
        }

        private async Task getLocalItems()
        {
            var localLibraryItems = LocalLibraryService.Instance.GetMediaItems();
            if (LocalItems == null)
                LocalItems = new ObservableCollection<LibraryItem>();

            if (localLibraryItems != null)
            {
                var locals = new ObservableCollection<LibraryItem>(localLibraryItems);
                switch (librarySort)
                {
                    case LibrarySort.Alphabetical:
                        await locals.SortByTitle();
                        break;

                    case LibrarySort.Recent:
                        await locals.SortByDate();
                        break;
                }

                await LocalItems.UpdateItems(locals);
            }

            OnPropertyChanged(nameof(LocalItemsCount));
        }

        private async Task updateLocalItems()
        {
            if (LocalItems != null)
            {
                var localPaths = LocalItems.ToList().Select(l => l.LocalFilePath).ToList();
                var newItems = LocalLibraryService.Instance.GetNewMediaItems(localPaths);
                await LocalItems.AddItems(newItems);
                var removed = LocalLibraryService.Instance.GetDeletedMediaItems(localPaths);
                if (removed != null && removed.Any())
                {
                    var removedItems = LocalItems.ToList().Where(l => removed.Contains(l.LocalFilePath));
                    if (removedItems != null && removedItems.Any())
                        await LocalItems.RemoveItems(removedItems);
                }

                OnPropertyChanged(nameof(LocalItemsCount));
            }
            else
            {
                await getLocalItems();
            }
        }

        private async void loadDetails(LibraryItem item)
        {
            await loadDetailsAsync(item);
        }

        private async Task loadDetailsAsync(LibraryItem item)
        {
            try
            {
                if (item == null)
                    return;

                if (item.DetailsLoaded)
                    return;

                var details = await LibraryClient.Get(item.ID);
                if (details != null)
                {
                    item.UpdateFromDetails(details);
                    item.DetailsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                //XXX : Handle error
                LoggerService.Instance.Log("ERROR: Library.loadDetailsAsync: " + ex);
            }
        }

        private async Task sort(LibrarySort sort)
        {
            Edit = false;
            switch (sort)
            {
                case LibrarySort.Alphabetical:
                    if (CloudItems != null)
                        await CloudItems.SortByTitle();

                    if (LocalItems != null)
                        await LocalItems.SortByTitle();
                    break;

                case LibrarySort.Recent:
                    if (CloudItems != null)
                        await CloudItems.SortByDate();

                    if (LocalItems != null)
                        await LocalItems.SortByDate();
                    break;
            }
        }

        private void manage()
        {
            Edit = true;
        }

        public async Task DownloadItem(LibraryItem item, bool interactive)
        {
            try
            {
                if (item == null)
                    return;

                LoggerService.Instance.Log("INFO: Library.DownloadItem called with Interactive set to: " + interactive);

                if (interactive)
                {
                    string warningMessage, title;
                    if (!itemDownloader.CanDownloadItems(out warningMessage, out title) &&
                        !string.IsNullOrWhiteSpace(warningMessage) && !string.IsNullOrEmpty(title))
                        Device.BeginInvokeOnMainThread(
                            () => Application.Current.MainPage.DisplayAlert(title, warningMessage, "OK"));
                }

                await doItemDownload(item);
            }
            catch (Exception ex)
            {
                LoggerService.Instance.Log("ERROR: Library.DownloadItem: " + ex);
            }
        }

        private async Task doItemDownload(LibraryItem item)
        {
            try
            {
                item.DownloadStatus = DownloadStatus.Downloading;
                var downloadResult =
                    await itemDownloader.Download(new DownloadItem {Id = item.ID, FileName = item.FullTitle + ".mp4"});
                if (!downloadResult.Item1)
                {
                    if (IsInBackground)
                        await RemoteNotificationsService.Instance.TriggerLocalNotification(
                            "We're sorry. This video cannot be downloaded because there is not enough space on your device. Please clear some space and try again.");
                    else
                        await Application.Current.MainPage.DisplayAlert("Error",
                            "We're sorry. This video cannot be downloaded because there is not enough space on your device. Please clear some space and try again.",
                            "OK");

                    item.DownloadUrl = downloadResult.Item2;
                    item.DownloadStatus = DownloadStatus.Failed;
                    item.DownloadProgress = null;
                }

                item.DownloadUrl = downloadResult.Item2;
                if (CloudItems != null && CloudItems.ToList().Contains(item))
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        SuspendOnSelectedItemDetailsChangedEvent = true;
                        Download.ChangeCanExecute();
                        CancelDownload.ChangeCanExecute();
                        Delete.ChangeCanExecute();
                        SuspendOnSelectedItemDetailsChangedEvent = false;

                        if (!downloadResult.Item1)
                            Play.ChangeCanExecute();
                    });
            }
            catch (Exception ex)
            {
                LoggerService.Instance.Log("ERROR: Failed to start downloading of item: Exception: " + ex);
            }
        }

        private void cancelDownload(LibraryItem item)
        {
            if (item == null || itemDownloader == null)
                return;

            itemDownloader.Cancel(item.ID);
        }

        private async Task play(LibraryItem item)
        {
            if (item == null)
                return;

            var itemToPlay = item;
            if (!item.IsLocal)
                itemToPlay = item.LocalItem;

            if (itemToPlay == null)
                return;

            var enableAdSkipper = false;

            if (itemToPlay.HasChapters)
                enableAdSkipper = await Application.Current.MainPage.DisplayAlert("AdSkip",
                    "Do you want to automatically skip advertisements?", "Yes", "No");

            var mediaPlayer = new MediaPlayer
            {
                BindingContext = Application.Current.BindingContext,
                SkipAdvertisements = enableAdSkipper,
                Video = itemToPlay
            };

            mediaPlayer.OnMediaPlayerFinished += async (sender, args) =>
            {
                LocalLibraryService.Instance.UpdateMediaItem(args.Video, true);
                await Application.Current.MainPage.Navigation.PopModalAsync();
            };

            await Application.Current.MainPage.Navigation.PushModalAsync(mediaPlayer);
        }

        private async Task<bool> delete(LibraryItem item)
        {
            if (item == null || item.Storage == LibraryItemStorage.iTunes)
                return false;

            LoggerService.Instance.Log("INFO: Library.delete: ID: " + item.ID);
            if (item.IsLocal)
            {
                if (item.Storage == LibraryItemStorage.AppLocal)
                {
                    var deleted = LocalLibraryService.Instance.DeleteMediaItem(item, true);
                    if (deleted)
                    {
                        if (CloudItems != null)
                        {
                            var deletedItem = CloudItems.FirstOrDefault(cloudItem => item.ID == cloudItem.ID);
                            if (deletedItem != null)
                            {
                                deletedItem.DownloadStatus = DownloadStatus.Unknown;
                                deletedItem.LocalItem = null;
                            }
                        }

                        if (LocalItems != null)
                        {
                            LocalItems.Remove(item);
                            OnPropertyChanged(nameof(LocalItemsCount));
                        }

                        if (item.Equals(SelectedItem))
                            SelectedItem = null;

                        if (item.Equals(SelectedLibraryItem))
                            SelectedLibraryItem = null;

                        return true;
                    }
                }
            }
            else
            {
                var response = await LibraryClient.Delete(item.ID);
                if (response != null && response.Success)
                {
                    if (Account != null && Account.SignedIn && Account.UserInfo != null &&
                        !string.IsNullOrEmpty(Account.UserInfo.Email))
                        LocalLibraryService.Instance.DeleteCloudItemId(item.ID, Account.UserInfo.Email);

                    CloudItems.Remove(item);
                    OnPropertyChanged(nameof(CloudItemsCount));

                    if (item.Equals(SelectedItem))
                        SelectedItem = null;

                    if (item.Equals(SelectedCloudItem))
                        SelectedCloudItem = null;

                    return true;
                }
            }

            return false;
        }

        private async Task downloadChecked()
        {
            if (CloudItems != null)
                try
                {
                    var downloadItems = CloudItems.Where(item => item.Checked).ToArray();
                    foreach (var downloadItem in downloadItems)
                        await doItemDownload(downloadItem);
                }
                catch (Exception ex)
                {
                    LoggerService.Instance.Log("ERROR: Library.downloadChecked: " + ex);
                }

            manageDone();
        }

        private void manageDone()
        {
            if (CloudItems != null)
                foreach (var item in CloudItems)
                    item.Checked = false;

            if (LocalItems != null)
                foreach (var item in LocalItems)
                    item.Checked = false;

            Edit = false;
        }

        private async Task<bool> deleteMultipleItems(IEnumerable<LibraryItem> items)
        {
            IsLoading = true;
            var success = true;

            try
            {
                foreach (var item in items)
                    success = await delete(item);
            }
            catch (Exception ex)
            {
                //XXX : Handle error
                LoggerService.Instance.Log("ERROR: Library.deleteMultipleItems: " + ex);
            }
            finally
            {
                IsLoading = false;
            }

            return success;
        }

        public override async Task Poll(bool showLoading)
        {
            try
            {
                if (showLoading)
                    IsLoading = true;

                await getItems(true);
            }
            catch (Exception ex)
            {
                //XXX : Handle error
                LoggerService.Instance.Log("ERROR: Library.Poll: " + ex);
            }
            finally
            {
                if (showLoading)
                    IsLoading = false;
            }
        }

        private async void ItemDownloaderOnDownloadComplete(object sender,
            AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            var id = ((DownloadProgress) asyncCompletedEventArgs.UserState).Id;
            var url = ((DownloadProgress) asyncCompletedEventArgs.UserState).Url;
            var willRetryOnError = ((DownloadProgress) asyncCompletedEventArgs.UserState).WillRetryOnError;
            var localFilePath = ((DownloadProgress) asyncCompletedEventArgs.UserState).LocalFilePath;
            var title = ((DownloadProgress) asyncCompletedEventArgs.UserState).Title;
            var isCustomError = ((DownloadProgress) asyncCompletedEventArgs.UserState).IsCustomError;

            if (!asyncCompletedEventArgs.Cancelled && asyncCompletedEventArgs.Error != null && willRetryOnError)
                return;

            LibraryItem itemBeingDownloaded = null;
            if (CloudItems != null)
                itemBeingDownloaded = CloudItems.FirstOrDefault(item => item.ID == id || item.DownloadUrl == url);

            if (itemBeingDownloaded != null)
            {
                if (asyncCompletedEventArgs.Cancelled)
                    itemBeingDownloaded.DownloadStatus = DownloadStatus.Canceled;
                else if (asyncCompletedEventArgs.Error != null)
                    itemBeingDownloaded.DownloadStatus = DownloadStatus.Failed;
                else
                    itemBeingDownloaded.DownloadStatus = DownloadStatus.Completed;

                itemBeingDownloaded.DownloadProgress = null;
            }

            if (!asyncCompletedEventArgs.Cancelled)
                if (asyncCompletedEventArgs.Error != null)
                {
                    var message = "There was a problem downloading your recording of \"" + title +
                                  "\". Please try again!";
                    if (isCustomError)
                        message = asyncCompletedEventArgs.Error.Message;

                    await RemoteNotificationsService.Instance.TriggerLocalNotification(new AppNotificationMessage
                    {
                        ID = id,
                        Text = message,
                        Type = AppNotificationType.DownloadFailed
                    });
                }
                else
                {
                    await RemoteNotificationsService.Instance.TriggerLocalNotification(new AppNotificationMessage
                    {
                        ID = id,
                        Text = "Your recording of \"" + title + "\" was downloaded successfully!",
                        Type = AppNotificationType.DownloadComplete
                    });

                    var downloadedItem = new LibraryItem
                    {
                        ID = id,
                        Storage = LibraryItemStorage.AppLocal,
                        LocalFilePath = localFilePath
                    };

                    if (LocalLibraryService.Instance.CreateMediaItem(downloadedItem, true))
                    {
                        if (itemBeingDownloaded != null)
                            itemBeingDownloaded.LocalItem = downloadedItem;

                        if (LocalItems != null)
                        {
                            var itemsToDelete =
                                LocalItems.Where(
                                    item =>
                                        item.LocalFilePath == downloadedItem.LocalFilePath &&
                                        item.Storage == downloadedItem.Storage).ToArray();
                            foreach (var itemToDelete in itemsToDelete)
                                LocalItems.Remove(itemToDelete);

                            LocalItems.Add(downloadedItem);
                            await sort(librarySort);

                            OnPropertyChanged(nameof(LocalItemsCount));
                        }
                    }
                    else
                    {
                        LoggerService.Instance.Log(
                            "Library.ItemDownloaderOnDownloadComplete: Unable to create media item");
                    }
                }

            if (itemBeingDownloaded != null)
                try
                {
                    SuspendOnSelectedItemDetailsChangedEvent = true;
                    Download.ChangeCanExecute();
                    CancelDownload.ChangeCanExecute();
                    Delete.ChangeCanExecute();
                    Play.ChangeCanExecute();
                    SuspendOnSelectedItemDetailsChangedEvent = false;
                }
                catch
                {
                }
        }

        private void ItemDownloaderOnDownloadProgress(object sender, DownloadProgress itemDownloaderProgress)
        {
            if (cloudItems == null)
                return;

            var itemBeingDownloaded = cloudItems.FirstOrDefault(item =>
                item.ID == itemDownloaderProgress.Id ||
                item.DownloadUrl == itemDownloaderProgress.Url);

            if (itemBeingDownloaded != null)
            {
                if (itemBeingDownloaded.DownloadStatus != DownloadStatus.Downloading)
                {
                    itemBeingDownloaded.DownloadStatus = DownloadStatus.Downloading;
                    try
                    {
                        SuspendOnSelectedItemDetailsChangedEvent = true;
                        Download.ChangeCanExecute();
                        CancelDownload.ChangeCanExecute();
                        Delete.ChangeCanExecute();
                        Play.ChangeCanExecute();
                        SuspendOnSelectedItemDetailsChangedEvent = false;
                    }
                    catch
                    {
                    }
                }

                itemBeingDownloaded.DownloadProgress = itemDownloaderProgress;
            }
        }

        public override async Task Logout()
        {
            if (CloudItems != null)
            {
                CloudItems.Clear();
                OnPropertyChanged(nameof(CloudItemsCount));
            }

            SelectedItem = null;

            await base.Logout();
        }

        public override async Task NetworkStatusChanged(NetworkStatus newStatus)
        {
            if (newStatus == NetworkStatus.NotReachable)
                IsLoading = false;

            if (CurrentNetworkStatus == NetworkStatus.NotReachable)
                if (newStatus != NetworkStatus.NotReachable)
                    await Poll(true);
                else
                    await updateLocalItems();

            await base.NetworkStatusChanged(newStatus);
        }
    }
}