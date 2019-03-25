using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PlayOnCloud.Model;
using Xamarin.Forms;

namespace PlayOnCloud.ViewModel
{
    public enum SignInAErrorType
    {
        LoginError,
        AccountCreationError
    }

    public class SignInArgs
    {
        public SignInArgs(bool signedIn, string error, SignInAErrorType errorType)
        {
            IsSignedIn = signedIn;
            SignedInError = error;
            ErrorType = errorType;
        }

        public bool IsSignedIn { get; private set; }

        public string SignedInError { get; private set; }

        public SignInAErrorType ErrorType { get; private set; }
    }

    public class Account : ViewModelBase
    {
        private ObservableCollection<IContentItem> channels;
        private ObservableCollection<IContentItem> channelsWithLoginOptions;
        private MediaContent mediaContentViewModel;
        private Register registerViewModel;
        private bool remoteNotificationsInitialized;
        private IContentItem selectedChannel;
        private User userInfo;

        public Account()
        {
            SubmitSupportRequest = new Command<LibraryItem>(async p => await submitSupportRequest(p));
            ChannelLogout =
                new Command<IContentItem>(async parameter => await mediaContentViewModel.PerformChannelLogout(parameter));
            PullUserCreditsAsync = new Command(async () => await FetchUserCreditsAsync());
            ChannelLoginHelp =
                new Command<IContentItem>(
                    parameter =>
                        MessagingCenterManager.ShowPopup(new ChannelLoginHelp {BindingContext = mediaContentViewModel}));
        }


        public ICommand SubmitSupportRequest { protected set; get; }

        public ICommand ChannelLogout { protected set; get; }

        public ICommand PullUserCreditsAsync { protected set; get; }

        public ICommand ChannelLoginHelp { protected set; get; }

        public ObservableCollection<IContentItem> Channels
        {
            get { return channels; }
            set
            {
                if (SetField(ref channels, value))
                {
                    if (channels == null)
                    {
                        ChannelsWithLoginOptions = null;
                        SelectedChannel = null;
                    }
                    else
                    {
                        ChannelsWithLoginOptions =
                            new ObservableCollection<IContentItem>(
                                channels.Cast<ChannelEx>()
                                    .Where(c => c.CredentialsType != ChannelCredentialsType.Anonymous));
                        if (SelectedChannel != null)
                            SelectedChannel = channels.FirstOrDefault(c => c.ID == SelectedChannel.ID);
                    }

                    OnPropertyChanged(nameof(ChannelsCount));
                }
            }
        }

        public int ChannelsCount
        {
            get
            {
                if (channels == null)
                    return 0;

                return channels.Count;
            }
        }

        public ObservableCollection<IContentItem> ChannelsWithLoginOptions
        {
            get { return channelsWithLoginOptions; }
            set { SetField(ref channelsWithLoginOptions, value); }
        }

        public IContentItem SelectedChannel
        {
            get { return selectedChannel; }
            set
            {
                if (SetField(ref selectedChannel, value))
                    if (mediaContentViewModel != null)
                        Task.Run(async () => { await loadCableProviders(selectedChannel as ChannelEx); });
            }
        }

        internal MediaContent MediaContent
        {
            get { return mediaContentViewModel; }
            set { SetField(ref mediaContentViewModel, value); }
        }

        internal Register Register
        {
            get { return registerViewModel; }
            set { SetField(ref registerViewModel, value); }
        }

        public User UserInfo
        {
            get { return userInfo; }
            set
            {
                if (SetField(ref userInfo, value))
                    OnPropertyChanged("SignedIn");
            }
        }

        public bool SignedIn => !string.IsNullOrEmpty(userInfo?.Token);

        public void TapGestureRecognizerTap(object p)
        {
            Device.OpenUri(new Uri(p.ToString()));
        }

        public event EventHandler<SignInArgs> OnSignInComplete;

        public event EventHandler<string> OnResetPasswordSuccess;

        public override async Task Init()
        {
            await base.Init();
        }

        public async Task Fire_OnSignInComplete(bool signedIn, string message, bool signedUp, SignInAErrorType errorType)
        {
            if (signedIn)
            {
#if _IOS_
			    

                if (!remoteNotificationsInitialized)
				{
					remoteNotificationsInitialized = true;
					RemoteNotificationsService.Instance.RegisteredForNotifications += async (sender, b) =>
					{
						string error = string.Empty;
						if (b.Success)
							await NotificationClient.AddDevice(RemoteNotificationsService.Instance.GetTokenType(), b.Info);
						else
							error = b.Info;

						//Inform user we failed to register for remote notifications
						if (!string.IsNullOrEmpty(error) && !IsInBackground)
							await Application.Current.MainPage.DisplayAlert("Notifications Subscription Error", b.Info, "OK");
					};
				}

				if (RemoteNotificationsService.Instance.CanRegisterForNotifications() && RemoteNotificationsService.Instance.IsRegisteredForNotifications())
					RemoteNotificationsService.Instance.RegisterForNotifications();

				//In case background app refresh is disabled inform the user
				RemoteNotificationsService.Instance.BackgroundRefreshStatusChanged += async (s, enabled) =>
				{
					if (!enabled  && !IsInBackground)
						await Application.Current.MainPage.DisplayAlert(string.Empty, "Please enable Background App Refresh for \"PlayOn Cloud\" in Settings-General in order to stay up-to-date with your recordings", "OK");
				};

				if (!RemoteNotificationsService.Instance.IsBackgoundRefreshEnabled())
				{
					if (await Application.Current.MainPage.DisplayAlert(string.Empty, "Please enable Background App Refresh for \"PlayOn Cloud\" in Settings-General in order to stay up-to-date with your recordings", "Yes", "No"))
						RemoteNotificationsService.Instance.OpenGeneralAppSettings();
				}
#endif
                //if (signedUp)
                //    await Application.Current.MainPage.DisplayAlert("Account Created!",
                //        "Check your email to receive your free recording credits.", "OK");
            }

            OnSignInComplete?.Invoke(this, new SignInArgs(signedIn, message, errorType));
        }

        public User LoadUserFromCredentials()
        {
            return UserStoreService.Instance.LoadUserCredentials();
        }

        public async Task FetchUserCreditsAsync()
        {
            try
            {
                if (!SignedIn)
                    return;

                var response = await AccountClient.Get();
                if (response != null && response.Success && response.Data != null)
                {
                    var user = response.Data.ToObject<User>();
                    UserInfo.Credits = user.Credits;
                }
            }
            catch (Exception ex)
            {
                LoggerService.Instance.Log("ERROR: Account.FetchUserCreditsAsync: " + ex);
            }
        }

        private void logMark(ref string logContents)
        {
            var networkName = string.Empty;

            var mark = MarkManager.GetMark();
            if (mark.HasFlag(Mark.Hulu) && mark.HasFlag(Mark.Netflix))
                networkName = "HN";
            else if (mark.HasFlag(Mark.Hulu))
                networkName = "H";
            else if (mark.HasFlag(Mark.Netflix))
                networkName = "N";

            if (!string.IsNullOrWhiteSpace(networkName))
                logContents += DateTime.UtcNow + ": MarkManager: Issue contacting the PlayOn Service. Error code: " +
                               networkName + ". Please forward to development for investigation." + Environment.NewLine;
        }

        public async Task<Tuple<bool, string>> SendSupportRequestAsync(string requestContent, string rid)
        {
            var logContents = LoggerService.Instance.GetLog();
            if (!string.IsNullOrEmpty(logContents))
                logMark(ref logContents);

            var errorMessage = string.Empty;
            var success = false;

            var response = await SupportClient.Submit(requestContent, rid, logContents);
            if (response != null)
                if (response.Success)
                    success = true;
                else
                    errorMessage = !string.IsNullOrEmpty(response.ErrorMessageClean)
                        ? response.ErrorMessageClean
                        : "Submission failed.";
            else
                errorMessage = "Submission failed.";

            return Tuple.Create(success, errorMessage);
        }

        public async Task<string> SignInWithTokenAsync(User storedUser, bool fireEvents = true)
        {
            try
            {
                LoggerService.Instance.Log("Account.SignInWithTokenAsync...");
                IsLoading = true;
                UserInfo = null;

                if (storedUser == null)
                {
                    LoggerService.Instance.Log("ERROR: Account.SignInWithTokenAsync: cannot get stored account");
                    var error = "There was an error logging in";
                    if (fireEvents)
                        await Fire_OnSignInComplete(SignedIn, error, false, SignInAErrorType.LoginError);

                    return error;
                }

                if (!storedUser.IsTokenExpired())
                {
                    LoggerService.Instance.Log("Account.SignInWithTokenAsync: account token NOT expired. reusing it");
                    UserInfo = storedUser;
                    await FetchUserCreditsAsync();
                    if (fireEvents)
                        await Fire_OnSignInComplete(SignedIn, null, false, SignInAErrorType.LoginError);

                    return null;
                }

                var errorMessage = string.Empty;
                var response = await LoginClient.LoginWithToken(storedUser.AuthToken);
                if (response != null)
                    if (response.Success)
                    {
                        var newUser = response.Data.ToObject<User>();
                        newUser.Name = newUser.FirstName + "" + userInfo.LastName;
                        if (!string.IsNullOrEmpty(newUser.Token))
                        {
                            newUser.AuthToken = storedUser.AuthToken;
                            UserInfo = newUser;

                            await FetchUserCreditsAsync();

                            UserStoreService.Instance.SaveUserCredentials(newUser);
                        }
                        else
                        {
                            LoggerService.Instance.Log("ERROR: Account.SignInWithTokenAsync: token is null");
                            errorMessage = "There was an error logging in";
                        }
                    }
                    else if (response.ErrorCode == ErrorCodes.AUTH_TOKEN_LOGIN)
                    {
                        //The auth token must be invalid - reset user info and request input
                        storedUser.Token = null;
                        storedUser.AuthToken = null;
                        UserInfo = null;

                        UserStoreService.Instance.SaveUserCredentials(storedUser);
                        errorMessage = response.ErrorMessageClean;
                    }
                    else
                    {
                        errorMessage = response.ErrorMessageClean;
                    }
                else
                    errorMessage = "There was an error logging in";

                if (!string.IsNullOrEmpty(errorMessage))
                    LoggerService.Instance.Log("ERROR: Account.SignInWithTokenAsync: " + errorMessage);
                else
                    LoggerService.Instance.Log("Account.SignInWithTokenAsync: success");

                OnPropertyChanged("SignedIn");
                if (fireEvents)
                    await Fire_OnSignInComplete(SignedIn, errorMessage, false, SignInAErrorType.LoginError);

                return errorMessage;
            }
            catch (ThrottleException ex)
            {
                return ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SignInAsync(string userName, string userPassword)
        {
            try
            {
                LoggerService.Instance.Log("Account.SignInAsync...");
                IsLoading = true;
                UserInfo = null;

                var errorMessage = string.Empty;

                var response = await LoginClient.LoginWithCredentials(userName, userPassword);
                if (response != null)
                    if (response.Success)
                    {
                        var newUser = response.Data.ToObject<User>();
                        if (!string.IsNullOrEmpty(newUser.Token))
                        {
                            UserInfo = newUser;
                            newUser.Password = userPassword;

                            await FetchUserCreditsAsync();

                            UserStoreService.Instance.SaveUserCredentials(UserInfo);
                        }
                        else
                        {
                            LoggerService.Instance.Log("ERROR: Account.SignInAsync: token is null");
                            errorMessage = "There was an error logging in";
                        }
                    }
                    else
                    {
                        errorMessage = response.ErrorMessageClean;
                    }
                else
                    errorMessage = "There was an error logging in";

                if (!string.IsNullOrEmpty(errorMessage))
                    LoggerService.Instance.Log("ERROR: Account.SignInAsync: " + errorMessage);
                else
                    LoggerService.Instance.Log("Account.SignInAsync: success");

                OnPropertyChanged("SignedIn");
                await Fire_OnSignInComplete(SignedIn, errorMessage, false, SignInAErrorType.LoginError);
            }
            catch (ThrottleException ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<Tuple<bool, string>> SignOutAsync()
        {
            try
            {
                LoggerService.Instance.Log("Account.SignOutAsync...");
                IsLoading = true;
                var error = string.Empty;
                var success = false;

                if (SignedIn)
                {
                    var email = string.Empty;
                    if (!string.IsNullOrEmpty(UserInfo.Email))
                        email = UserInfo.Email.ToLower();

                    var name = string.Empty;
                    if (!string.IsNullOrEmpty(UserInfo.Name))
                        name = UserInfo.Name.ToLower();

#if _IOS_

                    var notificationToken = RemoteNotificationsService.Instance.GetTokenValue();
					if (!string.IsNullOrEmpty(notificationToken))
					{
						var notificationType = RemoteNotificationsService.Instance.GetTokenType();
						await NotificationClient.RemoveDevice(notificationType, notificationToken);
					}
#endif
                    var response = await LogoutClient.Logout();
                    if (response != null)
                        if (response.Success)
                        {
                            SharedSettingsService.Instance.SetBoolValue("AutoDownloadDBCreated" + email, false);
                            await (Application.Current.MainPage.BindingContext as Cloud).Logout();
                            OnPropertyChanged("SignedIn");
                            success = true;
                        }
                        else
                        {
                            error = response.ErrorMessageClean;
                        }
                    else
                        error = "There was an error logging out";
                }
                else
                {
                    error = "Please log in before attempting to log out";
                }

                if (!string.IsNullOrEmpty(error))
                    LoggerService.Instance.Log("ERROR: Account.SignOutAsync: " + error);
                else
                    LoggerService.Instance.Log("Account.SignOutAsync: success");

                OnPropertyChanged("SignedIn");
                return Tuple.Create(success, error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task Logout()
        {
            if (Channels != null)
                foreach (var channel in channels)
                    (channel as ChannelEx).LoginInfo = new ChannelLoginInfo();

            UserInfo = null;
            UserStoreService.Instance.DeleteUserCredentials();

            await base.Logout();
        }

        public async Task SignUpAsync(string userName, string userPassword, string email)
        {
            try
            {
                LoggerService.Instance.Log("Account.SignUpAsync...");
                IsLoading = true;
                var error = string.Empty;
                var success = false;

                if (!SignedIn)
                {
                    var response = await SignupClient.SignupWithCredentials(userName, email, userPassword);
                    if (response != null)
                    {
                        if (response.Success)
                        {
                            var newUser = response.Data.ToObject<User>();
                            newUser.Name = newUser.FirstName + " " + newUser.LastName;
                            UserStoreService.Instance.SaveUserCredentials(newUser);

                            UserInfo = newUser;
                            success = true;

                            await FetchUserCreditsAsync();
                        }
                        else if (response.ErrorCode == ErrorCodes.DATA_ERROR && !userName.Contains(" "))
                        {
                            error = "Please enter your First and Last Name";
                        }
                        else if (!string.IsNullOrEmpty(response.ErrorMessageClean))
                        {
                            error = response.ErrorMessageClean;
                        }
                    }
                    else
                    {
                        error = "There was an error signing up";
                    }
                }
                else
                {
                    error = "Please log out before attempting to sign up";
                }

                if (!string.IsNullOrEmpty(error))
                {
                    LoggerService.Instance.Log("ERROR: Account.SignUpAsync: " + error);
                }
                else
                {
                    LoggerService.Instance.Log("Account.SignUpAsync: success");
                    FacebookToolsService.Instance.LogCustomEvent("AccountCreated");
                }

                OnPropertyChanged("SignedIn");
                await Fire_OnSignInComplete(success, error, true, SignInAErrorType.AccountCreationError);
            }
            catch (ThrottleException ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SignUpWithFacebookAsync(string facebookId, string facebookToken)
        {
            LoggerService.Instance.Log("Account.SignUpWithFacebookAsync...");

            var error = string.Empty;
            var success = false;

            if (!SignedIn)
            {
                var response = await SignupClient.SignupWithFacebook(facebookId, facebookToken);
                if (response != null)
                    if (response.Success)
                    {
                        var newUser = response.Data.ToObject<User>();
                        newUser.Name = newUser.FirstName + " " + newUser.LastName;
                        UserInfo = newUser;
                        UserStoreService.Instance.SaveUserCredentials(UserInfo);
                        success = true;

                        await FetchUserCreditsAsync();
                    }
                    else
                    {
                        error = response.ErrorMessageClean;
                    }
                else
                    error = "There was an error signing up with Facebook";
            }
            else
            {
                error = "Please log out before attempting to sign up";
            }

            if (!string.IsNullOrEmpty(error))
                LoggerService.Instance.Log("ERROR: Account.SignUpWithFacebookAsync: " + error);
            else
                LoggerService.Instance.Log("Account.SignUpWithFacebookAsync: success");

            OnPropertyChanged("SignedIn");
            await Fire_OnSignInComplete(success, error, true, SignInAErrorType.AccountCreationError);
        }

        public async Task SignInWithFacebookIdAsync(string facebookId, string facebookToken)
        {
            LoggerService.Instance.Log("Account.SignInWithFacebookIdAsync...");

            var error = string.Empty;
            var success = false;

            var response = await LoginClient.LoginWithFacebook(facebookId, facebookToken);
            if (response != null)
                if (response.Success)
                {
                    var newUser = response.Data.ToObject<User>();
                    newUser.Name = newUser.FirstName + " " + newUser.LastName;
                    if (!string.IsNullOrEmpty(newUser.Token))
                    {
                        UserInfo = newUser;
                        await FetchUserCreditsAsync();
                        success = true;

                        UserStoreService.Instance.SaveUserCredentials(UserInfo);
                    }
                    else
                    {
                        LoggerService.Instance.Log("ERROR: Account.SignInWithFacebookIdAsync: token is null");
                        error = "There was an error signing in with Facebook";
                    }
                }
                else
                {
                    error = response.ErrorMessageClean;
                }
            else
                error = "There was an error signing in with Facebook";

            if (!string.IsNullOrEmpty(error))
                LoggerService.Instance.Log("ERROR: Account.SignInWithFacebookIdAsync: " + error);
            else
                LoggerService.Instance.Log("Account.SignInWithFacebookIdAsync: success");

            OnPropertyChanged("SignedIn");
            await Fire_OnSignInComplete(success, error, false, SignInAErrorType.LoginError);
        }

        public async Task SignWithFacebook(bool signIn)
        {
            try
            {
                IsLoading = true;
                if (!SignedIn)
                {
                    var loginPage = new FacebookLogin();
                    loginPage.OnFacebookLoginCompleted += async (sender, args) =>
                    {
                        if (args.IsAuthenticated && Register != null)
                            Register.ForceHideView = true;

                        await Application.Current.MainPage.Navigation.PopModalAsync();

                        if (args.IsAuthenticated && Register != null)
                        {
                            Register.ForceHideView = false;
                            Register.IsLoading = true;
                        }

                        if (args.IsAuthenticated)
                            if (signIn)
                                await SignInWithFacebookIdAsync(args.Id, args.Token);
                            else
                                await SignUpWithFacebookAsync(args.Id, args.Token);
                        else
                            await Fire_OnSignInComplete(false, string.Empty, false, SignInAErrorType.LoginError);
                    };

                    await Application.Current.MainPage.Navigation.PushModalAsync(loginPage);
                }
                else
                {
                    await Fire_OnSignInComplete(false, "Please log out before attempting to log in", false,
                        SignInAErrorType.LoginError);
                }
            }
            catch (ThrottleException ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ResetPassword(string email)
        {
            try
            {
                IsLoading = true;
                var response = await PasswordClient.ResetRequest(email);
                if (response != null)
                    if (response.Success)
                    {
                        await Application.Current.MainPage.DisplayAlert("Reset Password",
                            "An email has been sent to your account. Please follow the instructions to reset your password.",
                            "OK");
                        OnResetPasswordSuccess?.Invoke(this, email);
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Reset Password", response.ErrorMessageClean,
                            "OK");
                    }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task loadCableProviders(ChannelEx channel)
        {
            if (channel != null && channel.CredentialsType == ChannelCredentialsType.UsernamePasswordServiceProvider)
            {
                if (channel.LoginInfo != null && channel.LoginInfo.CableProviders != null &&
                    channel.LoginInfo.CableProviders.Any())
                {
                    if (!string.IsNullOrEmpty(channel.LoginInfo.ProviderCode))
                    {
                        var selectedCableProvider =
                            channel.LoginInfo.CableProviders.FirstOrDefault(
                                c => c.Code == channel.LoginInfo.ProviderCode);
                        if (selectedCableProvider != null)
                            channel.LoginInfo.SelectedCableProviderIndex =
                                channel.LoginInfo.CableProviders.IndexOf(selectedCableProvider);
                    }

                    return;
                }

                try
                {
                    IsLoading = true;
                    await mediaContentViewModel.LoadCableProviders(channel);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        public override async Task NetworkStatusChanged(NetworkStatus newStatus)
        {
            if (CurrentNetworkStatus == NetworkStatus.NotReachable && newStatus != NetworkStatus.NotReachable &&
                !SignedIn)
            {
                var stored = LoadUserFromCredentials();
                if (stored != null)
                    await SignInWithTokenAsync(stored);
            }

            await base.NetworkStatusChanged(newStatus);
        }

        private async Task submitSupportRequest(LibraryItem libraryItem)
        {
            var request = new SupportRequest();

            App.KeyboardTrackingEnabled = false;
            request.Disappearing += (o, args) => { App.KeyboardTrackingEnabled = true; };


            request.ToolbarItems.Add(new ToolbarItem
            {
                Text = "Submit",
                Command = new Command(async () =>
                {
                    await Application.Current.MainPage.Navigation.PopAsync();
                    var result = await SendSupportRequestAsync(request.SupportRequestContents, libraryItem?.ID);

                    //XXX: But what happens if the internet isn't connected?
                    var isConnected = await RestService.Instance.GetIsConnected();
                    if (isConnected)
                        await Application.Current.MainPage.DisplayAlert("Support Request",
                            !result.Item1 ? result.Item2 : "Successfully submitted support ticket.", "OK");
                })
            });

            await Application.Current.MainPage.Navigation.PushAsync(request);
        }

        public void RefreshSelectedChannel()
        {
            OnPropertyChanged("SelectedChannel");
        }

        public async Task<bool> PerformChannelLogin(IContentItem item)
        {
            if (mediaContentViewModel != null)
                return await mediaContentViewModel.PerformChannelLogin(item);

            return false;
        }
    }
}