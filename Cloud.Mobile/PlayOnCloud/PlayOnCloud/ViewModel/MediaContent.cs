using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Newtonsoft.Json;
using PlayOnCloud.Model;

namespace PlayOnCloud.ViewModel
{
	public class MediaContent : ViewModelBase
	{
		private static string[] browsePathSeparators = new string[] { " | " };

		private IContentItem root;
		private IContentItem selectedFolder;
		private IContentItem selectedItem;
		private IContentItem selectedMediaItem;
		private IContentItem selectedChannel;
		private ObservableCollection<IContentItem> breadcrumbs;
		private string searchPhrase;
		private bool canRefresh = true;
		private DateTime lastChannelsLoaded = DateTime.MinValue;
		private string mastheadLoadingPlaceholder;

		private Queue queueViewModel;
		private Account accountViewModel;
		private Products productsViewModel;

		public MediaContent(Queue queueViewModel, Account accountViewModel, Products productsViewModel)
		{
			this.queueViewModel = queueViewModel;
			this.accountViewModel = accountViewModel;
			this.productsViewModel = productsViewModel;

			breadcrumbs = new ObservableCollection<IContentItem>();

			SelectItem = new Command<IContentItem>((parameter) => SelectedItem = parameter);
			ChannelLogin = new Command<IContentItem>(async (parameter) => await PerformChannelLogin(parameter));
			ChannelLogout = new Command<IContentItem>(async (parameter) => await PerformChannelLogout(parameter));
			ChannelLoginFromAccountSection = new Command<IContentItem>(async (parameter) => await PerformChannelLogin(parameter));
			ChannelLoginHelp = new Command<IContentItem>((parameter) => MessagingCenterManager.ShowPopup(new ChannelLoginHelp() { BindingContext = this }));
			GoBack = new Command(() => goBack());
			Search = new Command<string>(async (parameter) => await search(parameter));
			ShowChannelLoginControls = new Command(() => MessagingCenterManager.ShowPopup(new ChannelLoginPopup() { BindingContext = this }));
			ShowLoginDropdown = new Command(() => MessagingCenterManager.ShowPopup(new LoginPopup()));
			ShowChannelLoginInfo = new Command(() => MessagingCenterManager.ShowPopup(new ChannelLoginInfoView()));
			HideLoginInfoBar = new Command(() =>
			{
				if (selectedChannel is ChannelEx)
				{
					if ((selectedChannel as ChannelEx).LoginInfo == null)
						(selectedChannel as ChannelEx).LoginInfo = new ChannelLoginInfo();

					(selectedChannel as ChannelEx).LoginInfo.HideLoginInfoBar = true;
				}
			});

			RefreshSelected = new Command(async () => await RefreshSelectedFolder());
		}

		public ICommand SelectItem { protected set; get; }

		public ICommand ChannelLogin { protected set; get; }

		public ICommand ChannelLogout { protected set; get; }

		public ICommand ChannelLoginFromAccountSection { protected set; get; }

		public ICommand ChannelLoginHelp { protected set; get; }

		public ICommand GoBack { protected set; get; }

		public ICommand Search { protected set; get; }

		public ICommand ShowChannelLoginControls { protected set; get; }

		public ICommand ShowChannelLoginInfo { protected set; get; }

		public ICommand ShowLoginDropdown { protected set; get; }

		public ICommand HideLoginInfoBar { protected set; get; }

		public ICommand RefreshSelected { protected set; get; }

		public override Task OnBackButtonPressed()
		{
			goBack();
			return base.OnBackButtonPressed();
		}

		public override async Task Init()
		{
			await base.Init();
			SelectedChannel = null;
			SelectedFolder = null;
			SelectedItem = null;

			if (Root != null)
				Root = null;

			Root = new ContentItemEx()
			{
				ID = "",
				Name = "Channels",
			};

			SelectedItem = Root;
			if (!Initialized)
				FacebookToolsService.Instance.LogCustomEvent("ChannelsRootLoaded");

			if (!productsViewModel.Initialized)
				await TaskQueue.EnqueueAsync(async () => await productsViewModel.Init());
		}

		public Products Products
		{
			get { return productsViewModel; }
			set { SetField(ref productsViewModel, value); }
		}

		public Account Account
		{
			get { return accountViewModel; }
			set { SetField(ref accountViewModel, value); }
		}

		public IContentItem Root
		{
			get { return root; }
			set { SetField(ref root, value); }
		}

		public ObservableCollection<IContentItem> Channels
		{
			get
			{
				if (Root != null)
					return Root.Children;

				return null;
			}
		}

		public string MastheadLoadingPlaceholder
		{
			get { return mastheadLoadingPlaceholder; }
			set { SetField(ref mastheadLoadingPlaceholder, value); }
		}

		public IContentItem SelectedItem
		{
			get { return selectedItem; }
			set
			{
				setMastheadLoadingPlaceholder(selectedItem as ContentItemEx, value as ContentItemEx);
				if (SetField(ref selectedItem, value) && (selectedItem != null))
				{
					if (selectedItem.IsFolder || selectedItem.IsChannel || selectedItem.IsRoot)
					{
						selectedItem.Children = null;
						SelectedFolder = selectedItem;
						updateBreadcrumbs();

						if (selectedItem.IsFromSearch && !string.IsNullOrWhiteSpace(selectedItem.ID))
							TaskQueue.Enqueue(async () => await search(selectedItem.ID));
						else if (selectedItem.IsDeepLink && !string.IsNullOrWhiteSpace(selectedItem.ID))
							TaskQueue.Enqueue(async () => await NavigateToDeepLink(selectedChannel, selectedItem.ID));
						else
							TaskQueue.Enqueue(async () => await loadChildrenAsync(selectedItem, false));
					}
					else
					{
						SelectedMediaItem = selectedItem;
						loadSelectedItemDetails();
					}
				}
			}
		}

		public IContentItem SelectedMediaItem
		{
			get { return selectedMediaItem; }
			set
			{
				if (SetField(ref selectedMediaItem, value))
					Products.PurchaseContentItem.ChangeCanExecute();
			}
		}

		public IContentItem SelectedChannel
		{
			get { return selectedChannel; }
			set
			{
				if (
					(selectedChannel != null) &&
					(selectedChannel != value) &&
					((selectedChannel as ChannelEx).LoginInfo != null) &&
					!(selectedChannel as ChannelEx).LoginInfo.LoginPerformed)
					(selectedChannel as ChannelEx).LoginInfo.Clear();

				if (SetField(ref selectedChannel, value) && (selectedChannel != null))
				{
					var channel = selectedChannel as ChannelEx;
					if (channel != null)
						MastheadLoadingPlaceholder = channel.MastheadUrl;

					if ((channel != null) &&
						(channel.CredentialsType == ChannelCredentialsType.UsernamePasswordServiceProvider) &&
						((channel.LoginInfo == null) || !channel.LoginInfo.HasCredentials || (channel.LoginInfo.CableProviders == null) || !channel.LoginInfo.CableProviders.Any()))
						Task.Run(async () => { await LoadCableProviders(channel); });
				}
			}
		}

		public IContentItem SelectedFolder
		{
			get { return selectedFolder; }
			set { SetField(ref selectedFolder, value); }
		}

		public ObservableCollection<IContentItem> Breadcrumbs
		{
			get { return breadcrumbs; }
		}

		public string SearchPhrase
		{
			get { return searchPhrase; }
			set { SetField(ref searchPhrase, value); }
		}

		internal ChannelEx GetContentItemChannel(ContentItem item)
		{
			if ((item != null) && !string.IsNullOrEmpty(item.ID) && (Channels != null) && Channels.Any())
			{
				string[] idParts = item.ID.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
				if (idParts.Length > 0)
					return Channels.Cast<ChannelEx>().FirstOrDefault(c => c.ID == idParts[0]);
			}

			return null;
		}

		public async void GoHome()
		{
			if (SelectedItem != root)
				SelectedItem = root;
			else
				await RefreshSelectedFolder();

			updateBreadcrumbs();
		}

		private async Task loadChildrenAsync(IContentItem item, bool silent)
		{
			if (item == null)
				return;

			if (item.Children != null)
				item.Children = new ObservableCollection<IContentItem>();

			if (item.IsChannel &&
				(((item as ChannelEx).CredentialsType == ChannelCredentialsType.UsernamePassword) || ((item as ChannelEx).CredentialsType == ChannelCredentialsType.UsernamePasswordServiceProvider) || ((item as ChannelEx).CredentialsType == ChannelCredentialsType.UsernamePasswordPin)) &&
				(!(item as ChannelEx).LoginInfo.HasCredentials))
			{
				SelectedChannel = item;
				return;
			}

			try
			{
				IsLoading = true;
				IEnumerable<IContentItem> children = null;

				if (item.IsRoot)
					LoggerService.Instance.Log("INFO: Loading channels list");

				if (item.IsChannel)
				{
					SelectedChannel = item;
					await ImageToolsService.Instance.PreloadUrl((item as ChannelEx).MastheadUrl);
				}

				if (item.Expired && !item.IsChannel)
					await renewItemID(item);

				try
				{
					children = await getItemChildren(item, silent);
				}
				catch (UpgradeException)
				{
					canRefresh = false;
				}
				catch (MaintenanceException)
				{
					canRefresh = false;
				}
				catch (SessionExpiredException)
				{
					if (await renewItemID(item))
					{
						markAllExpired();
						children = await getItemChildren(item, silent);
						item.Expired = false;
					}
				}

				if (children != null)
				{
					if (children.Any() && !item.IsRoot)
					{
						var firstChildImage = await ContentClient.GetSmallImage(children.First().ID, SizingMode.Auto);
						if ((firstChildImage == null) || (firstChildImage.Length == 0))
							firstChildImage = await ContentClient.GetSmallImage(item.ID, 192, 128);

						if ((firstChildImage != null) && (firstChildImage.Length > 0))
						{
							int width = 0;
							int height = 0;
							ImageToolsService.Instance.GetImageSize(firstChildImage, out width, out height);
							if ((width > 0) && (height > 0))
							{
								double ratio = width / (double)height;
								foreach (var child in children)
									(child as ContentItemEx).ImageRatio = ratio;
							}
						}
					}

					item.Children = new ObservableCollection<IContentItem>(children);
				}

				if (item.IsRoot && (children != null))
				{
					foreach (var channel in children)
						loadStoredChannelLoginInfo(channel as ChannelEx);

					accountViewModel.Channels = new ObservableCollection<IContentItem>(children);

					if (item.IsRoot)
						LoggerService.Instance.Log("INFO: Channels list loaded");
				}
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: MediaContent.loadChildrenAsync: " + ex);
			}
			finally
			{
				if (IsLoading)
					IsLoading = false;
			}
		}

		private async Task<bool> renewItemID(IContentItem item)
		{
			if (item is ContentItemEx)
			{
				var parents = (item as ContentItemEx).GetParents();
				var newID = await GetNewItemID(parents, item.Name);
				if (!string.IsNullOrEmpty(newID))
				{
					item.ID = newID;
					item.Expired = false;
					return true;
				}
			}

			return false;
		}

		private async Task<IEnumerable<IContentItem>> getItemChildren(IContentItem item, bool silent)
		{
			if (item.IsRoot)
			{
				lastChannelsLoaded = DateTime.Now;
				var result = await ContentClient.GetChannels();
				canRefresh = true;
				if ((result != null) && result.Any())
					return result;
				else
				{
					var isConnected = await RestService.Instance.GetIsConnected();
					if (!silent && isConnected)
						Device.BeginInvokeOnMainThread(() => Application.Current.MainPage.DisplayAlert("Error", "Unable to get channel list. Please make sure you are online, and try again.", "OK"));

					return null;
				}
			}
			else
			{
				if (item.IsChannel)
				{
					var channel = item as ChannelEx;
					if (!channel.IsAvailable)
						return null;

					if ((channel.LoginInfo != null) && channel.LoginInfo.HasCredentials)
					{
						channel.LoginInfo.LoginPerformed = true;
						var result = await ContentClient.LoginAndGetChildren(item.ID, channel.LoginInfo);
						if (result == null)
						{
							var validation = await SettingsClient.ValidateCredentials(channel.ID, channel.LoginInfo);
							if ((validation != null) && !validation.Success)
							{
								channel.LoginInfo.ValidationSuccessful = false;
								Device.BeginInvokeOnMainThread(() => AlertViewService.Instance.ShowAlert(validation.Message));
							}
						}
						else
						{
							channel.LoginInfo.ValidationSuccessful = true;
							UserStoreService.Instance.StoreRecordInKeychain(getAccountEmail() + "channel-" + item.ID, JsonConvert.SerializeObject((item as ChannelEx).LoginInfo));
						}

						channel.RefreshIsSearchVisible();

						return result;
					}
					else if (channel.CredentialsType != ChannelCredentialsType.Anonymous)
						return await ContentClient.LoginAndGetChildren(item.ID, new ChannelLoginInfo());
				}

				return await ContentClient.GetChildren(item.ID);
			}
		}

		private async void loadSelectedItemDetails()
		{
			await loadSelectedItemDetailsAsync();
		}

		private async Task loadSelectedItemDetailsAsync()
		{
			if (selectedMediaItem == null)
				return;

			try
			{
				IsDetailsLoading = true;
				ContentItemEx details = null;

				if (selectedMediaItem.Expired)
					await renewItemID(selectedMediaItem);

				try
				{
					details = await ContentClient.GetDetails(selectedMediaItem.ID);
					if ((details == null) && await renewItemID(selectedMediaItem))
						details = await ContentClient.GetDetails(selectedMediaItem.ID);
				}
				catch (SessionExpiredException)
				{
					if (await renewItemID(selectedMediaItem))
					{
						markAllExpired();
						details = await ContentClient.GetDetails(selectedMediaItem.ID);
					}
				}

				if ((details != null) && (selectedMediaItem is ContentItemEx))
					(selectedMediaItem as ContentItemEx).UpdateFromDetails(details);
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: MediaContent.loadSelectedItemDetailsAsync: " + ex);
			}
			finally
			{
				IsDetailsLoading = false;
			}
		}

		internal async Task LoadCableProviders(ChannelEx channel)
		{
			try
			{
				if (channel.LoginInfo == null)
					channel.LoginInfo = new ChannelLoginInfo();

				IsLoading = true;
				var providers = await SettingsClient.GetCableProviders(channel.ID);
				if (providers != null)
				{
					channel.LoginInfo.CableProviders = new ObservableCollection<ProviderCode>(providers);
					if (!string.IsNullOrEmpty(channel.LoginInfo.ProviderCode))
					{
						var selectedCableProvider = channel.LoginInfo.CableProviders.FirstOrDefault(c => c.Code == channel.LoginInfo.ProviderCode);
						if (selectedCableProvider != null)
							channel.LoginInfo.SelectedCableProviderIndex = channel.LoginInfo.CableProviders.IndexOf(selectedCableProvider);
					}
				}
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: MediaContent.LoadCableProviders: " + ex);
			}
			finally
			{
				IsLoading = false;
			}
		}

		public async Task<bool> PerformChannelLogin(IContentItem item)
		{
			var result = false;
			if ((item == null) || !(item is ChannelEx))
				return result;

			try
			{
				IsLoading = true;
				accountViewModel.IsLoading = true;
				var channel = item as ChannelEx;
				if ((channel != null) && (channel.LoginInfo != null))
				{
					try
					{
						var validation = await SettingsClient.ValidateCredentials(channel.ID, channel.LoginInfo);
						if (validation != null)
						{
							channel.LoginInfo.LoginPerformed = validation.Success;
							channel.LoginInfo.ValidationSuccessful = validation.Success;
							result = validation.Success;
							if (!validation.Success)
								Device.BeginInvokeOnMainThread(() => AlertViewService.Instance.ShowAlert(validation.Message));

							if (validation.Success)
								UserStoreService.Instance.StoreRecordInKeychain(getAccountEmail() + "channel-" + item.ID, JsonConvert.SerializeObject(channel.LoginInfo));
						}
					}
					finally
					{
						accountViewModel.IsLoading = false;
					}

					channel.RefreshIsSearchVisible();
					accountViewModel.RefreshSelectedChannel();

					if (channel.LoginInfo.LoginPerformed)
					{
						if (SelectedChannel == channel)
						{
							selectedItem = channel;
							OnPropertyChanged("SelectedItem");
							selectedItem.Children = null;
							SelectedFolder = selectedItem;
							updateBreadcrumbs();
							await TaskQueue.EnqueueAsync(async () => await loadChildrenAsync(selectedItem, false));
							OnPropertyChanged("SelectedChannel");
						}
					}
				}
			}
			finally
			{
				IsLoading = false;
			}

			return result;
		}

		public async Task<bool> PerformChannelLogout(IContentItem item)
		{
			if ((item == null) || !(item is ChannelEx))
				return false;

			try
			{
				accountViewModel.IsLoading = true;
				var channel = item as ChannelEx;
				if ((channel != null) && (channel.LoginInfo != null))
				{
					string email = channel.LoginInfo.Username;
					channel.LoginInfo = new ChannelLoginInfo();
					channel.LoginInfo.Username = email;
					channel.LoginInfo.ValidationSuccessful = false;
					UserStoreService.Instance.StoreRecordInKeychain(getAccountEmail() + "channel-" + item.ID, JsonConvert.SerializeObject(channel.LoginInfo));
					accountViewModel.RefreshSelectedChannel();
					if (SelectedChannel == channel)
					{
						selectedItem = channel;
						OnPropertyChanged("SelectedItem");
						selectedItem.Children = null;
						SelectedFolder = selectedItem;
						updateBreadcrumbs();
						await TaskQueue.EnqueueAsync(async () => await loadChildrenAsync(selectedItem, false));
						OnPropertyChanged("SelectedChannel");
					}

					return true;
				}
			}
			finally
			{
				accountViewModel.IsLoading = false;
			}

			return false;
		}

		private void loadStoredChannelLoginInfo(ChannelEx channel)
		{
			if (channel == null)
				return;

			string email = getAccountEmail();
			string storedChannelLoginData = UserStoreService.Instance.GetRecordFromKeychain(email + "channel-" + channel.ID);
			if (!string.IsNullOrEmpty(storedChannelLoginData))
			{
				var loginInfo = JsonConvert.DeserializeObject<ChannelLoginInfo>(storedChannelLoginData);
				if (loginInfo != null)
					channel.LoginInfo = loginInfo;
			}

			if ((channel.CredentialsType != ChannelCredentialsType.Anonymous) && (accountViewModel.UserInfo != null) && !string.IsNullOrEmpty(accountViewModel.UserInfo.Email))
			{
				if (channel.LoginInfo == null)
					channel.LoginInfo = new ChannelLoginInfo();

				if (string.IsNullOrEmpty(channel.LoginInfo.Username))
					channel.LoginInfo.Username = accountViewModel.UserInfo.Email;
			}
		}

		private void goBack()
		{
			if (IsLoading)
				IsLoading = false;

			if (selectedFolder == null)
				return;

			var previousItem = selectedFolder.Parent;
			if (previousItem != null)
			{
				if (previousItem.IsRoot || selectedFolder.IsDeepLink)
				{
					SelectedItem = previousItem;
					return;
				}

				selectedItem = previousItem;
				SelectedFolder = selectedItem;

				if (selectedItem.IsChannel)
					SelectedChannel = selectedItem;

				OnPropertyChanged("SelectedItem");
				updateBreadcrumbs();
			}
		}

		private async Task search(string searchTerms, bool isDeepLink = false)
		{
			if (string.IsNullOrEmpty(searchTerms) || (selectedChannel == null))
				return;

			IsLoading = true;
			try
			{
				await performSearch(searchTerms, isDeepLink);
			}
			catch (Exception ex)
			{
				//XXX : Handle error
				LoggerService.Instance.Log("ERROR: MediaContent.search: " + ex);
			}
			finally
			{
				if (SelectedFolder?.Children?.Count > 0)
					SearchPhrase = null;

				IsLoading = false;
			}
		}

		private async Task performSearch(string searchTerms, bool isDeepLink = false)
		{
			IEnumerable<IContentItem> children = null;
			try
			{
				if (isDeepLink)
					children = await ContentClient.DeepLink(selectedChannel.ID, searchTerms);
				else
					children = await ContentClient.Search(selectedChannel.ID, searchTerms);
			}
			catch (SessionExpiredException)
			{
				await getItemChildren(selectedChannel, true);
				if (isDeepLink)
					children = await ContentClient.DeepLink(selectedChannel.ID, searchTerms);
				else
					children = await ContentClient.Search(selectedChannel.ID, searchTerms);
			}

			var name = searchTerms;
			if (isDeepLink)
				name = "Videos";

			ContentItemEx searchResults = new ContentItemEx
			{
				Type = ContentType.Folder,
				ID = searchTerms,
				Parent = selectedChannel,
				Name = name,
				IsFromSearch = !isDeepLink,
				IsDeepLink = isDeepLink
			};

			if (children != null)
				searchResults.Children = new ObservableCollection<IContentItem>(children);

			selectedItem = searchResults;
			updateBreadcrumbs();
			SelectedFolder = selectedItem;
			OnPropertyChanged("SelectedItem");
		}

		public async Task NavigateToDeepLink(IContentItem provider, string link)
		{
			if (string.IsNullOrEmpty(link) || (provider == null) || !provider.IsChannel)
				return;

			IsLoading = true;
			try
			{
				var channel = provider as ChannelEx;
				if (!channel.IsAvailable)
					return;

				SelectedChannel = channel;
				selectedItem = provider;
				OnPropertyChanged("SelectedItem");
				SelectedFolder = selectedItem;
				if ((channel.LoginInfo != null) && channel.LoginInfo.HasCredentials && !channel.LoginInfo.LoginPerformed)
				{
					channel.LoginInfo.LoginPerformed = true;
					var result = await ContentClient.LoginAndGetChildren(channel.ID, channel.LoginInfo);
					if (result == null)
						return;

					channel.LoginInfo.ValidationSuccessful = true;
					channel.RefreshIsSearchVisible();
				}

				await performSearch(link, true);
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: MediaContent.NavigateToDeepLink: " + ex);
			}
			finally
			{
				IsLoading = false;
			}
		}

		public async Task<string> GetNewItemID(List<IContentItem> parents, string name)
		{
			if ((parents == null) || !parents.Any() || string.IsNullOrEmpty(name))
				return null;

			if ((Root == null) || (Root.Children == null))
				return null;

			var channel = Root.Children.FirstOrDefault(c => c.Name == parents[0].Name);
			if (channel == null)
				return null;

			string currentID = channel.ID;
			while (parents.Count > 0)
			{
				var currentParent = parents[0];
				parents.RemoveAt(0);
				string title = name;
				if (parents.Count > 0)
					title = parents[0].Name;

				IEnumerable<ContentItemEx> items = null;
				try
				{
					if ((currentID == channel.ID) && (channel is ChannelEx) && ((channel as ChannelEx).LoginInfo != null) && (channel as ChannelEx).LoginInfo.HasCredentials)
						items = await ContentClient.LoginAndGetChildren(currentID, (channel as ChannelEx).LoginInfo);
					else
					{
						if (currentParent.IsFromSearch)
							items = await ContentClient.Search(channel.ID, currentID);
						else if (currentParent.IsDeepLink)
							items = await ContentClient.DeepLink(channel.ID, currentID);
						else
							items = await ContentClient.GetChildren(currentID);
					}
				}
				catch
				{
				}

				if ((items == null) || !items.Any())
					return null;

				if (parents.Count == 0)
				{
					var item = items.FirstOrDefault(i => i.Name == name);
					if (item != null)
						return item.ID;
				}
				else
				{
					var item = items.FirstOrDefault(i => i.Name == title);
					if (item != null)
						currentID = item.ID;
					else if (parents[0].IsFromSearch || parents[0].IsDeepLink)
						currentID = parents[0].ID;
					else
						return null;
				}
			}

			return null;
		}

		private string getAccountEmail()
		{
			if ((accountViewModel.UserInfo != null) && !string.IsNullOrEmpty(accountViewModel.UserInfo.Email))
				return accountViewModel.UserInfo.Email;

			//XXX: fake key to store in keychain
			return "free@account.com";
		}

		public override async Task Logout()
		{
			SelectedChannel = null;
			SelectedFolder = null;
			SelectedItem = null;
			Root = null;
			updateBreadcrumbs();

			await base.Logout();
			await Init();
		}

		protected override async Task refresh(bool silent)
		{
            
			if (selectedFolder != null)
				await TaskQueue.EnqueueAsync(async () => await loadChildrenAsync(selectedFolder, silent));

			await base.refresh(silent);
		}

		public async Task RefreshSelectedFolder()
		{
			if (canRefresh && !IsLoading)
				await this.refresh(true);
		}

		public override async Task NetworkStatusChanged(NetworkStatus newStatus)
		{
			if ((CurrentNetworkStatus == NetworkStatus.NotReachable) && (newStatus != NetworkStatus.NotReachable))
				await refresh(false);

			await base.NetworkStatusChanged(newStatus);
		}

		private void markAllExpired()
		{
			if (breadcrumbs != null)
			{
				var items = breadcrumbs.ToList();
				foreach (var breadcrumb in items)
					markChildrenAsExpired(breadcrumb);
			}
		}

		private void markChildrenAsExpired(IContentItem item)
		{
			if ((item != null) && item.IsFolder && !item.IsRoot && (item.Children != null))
				foreach (var child in item.Children)
					child.Expired = true;
		}

		private void updateBreadcrumbs()
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				try
				{
					breadcrumbs.Clear();
					var selected = selectedItem;
					if (selected == null)
						return;

					if (selectedItem.IsChannel)
					{
						breadcrumbs.Add(selectedItem);
						return;
					}

					if (selectedItem is ContentItemEx)
					{
						var parents = (selectedItem as ContentItemEx).GetParents();
						foreach (var parent in parents)
							breadcrumbs.Add(parent);

						if (selectedItem.IsFolder)
							breadcrumbs.Add(selectedItem);
					}
				}
				catch (Exception ex)
				{
					LoggerService.Instance.Log("ERROR: updateBreadcrumbs: " + ex);
				}
			});
		}

		private void setMastheadLoadingPlaceholder(ContentItemEx previous, ContentItemEx next)
		{
			try
			{
				if ((next != null) && (previous != null))
				{
					if ((previous != null) && (previous.Children != null) && !previous.Children.Contains(next))
						return;

					if (next.IsRoot)
						return;

					var configuration = FFImageLoading.ImageService.Instance.Config;
					if (!configuration.DiskCache.ExistsAsync(configuration.MD5Helper.MD5(next.LargeThumbnailMastheadUrl)).Result &&
						configuration.DiskCache.ExistsAsync(configuration.MD5Helper.MD5(previous.LargeThumbnailMastheadUrl)).Result)
					{
						MastheadLoadingPlaceholder = previous.LargeThumbnailMastheadUrl;
						return;
					}
				}

				if (selectedChannel is ChannelEx)
					MastheadLoadingPlaceholder = (selectedChannel as ChannelEx).MastheadUrl;
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: setMastheadLoadingPlaceholder: " + ex);
			}
		}

		public override Task Poll(bool showLoading)
		{
			if ((SelectedItem == Root) && (DateTime.Now.Subtract(lastChannelsLoaded).TotalMinutes > 5))
				return RefreshSelectedFolder();

			return base.Poll(showLoading);
		}
	}
}
