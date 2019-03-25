using System.Threading.Tasks;
using PlayOnCloud.Model;
using PlayOnCloud.ViewModel;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class PostLoad : ContentPage
	{
		public PostLoad()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}

		public bool InitiateSignInSequencePerformed { get; set; }

		public async Task InitiateSignInSequence()
		{
			InitiateSignInSequencePerformed = true;

			var cloud = (PlayOnCloud.ViewModel.Cloud)(((Cloud)Application.Current.BindingContext));
			var account = cloud.Account;

			var isInternetConnected = (cloud.CurrentNetworkStatus != NetworkStatus.NotReachable) && await RestService.Instance.CheckIfConnectionLost(true);
			var stored = account.LoadUserFromCredentials();

			if (!account.SignedIn)
			{
				account.OnSignInComplete += Account_OnSignInComplete;
				
				if (stored != null)
				{
					if(!string.IsNullOrEmpty(stored.AuthToken) && isInternetConnected)
						await account.SignInWithTokenAsync(stored);
					else
						await continueToNextPage(true, isInternetConnected);
				}
				else
					await continueToNextPage(false, isInternetConnected);
			}
			else
				await continueToNextPage(stored != null, isInternetConnected);
		}

		private async void Account_OnSignInComplete(object sender, SignInArgs e)
		{
			var cloud = (PlayOnCloud.ViewModel.Cloud)(((Cloud)Application.Current.BindingContext));

			var isInternetConnected = (cloud.CurrentNetworkStatus != NetworkStatus.NotReachable) && await RestService.Instance.GetIsConnected();
			if (!e.IsSignedIn)
			{
				if (isInternetConnected)
					await Application.Current.MainPage.DisplayAlert((((e.ErrorType == SignInAErrorType.AccountCreationError) ? "Account Creation " : "Login ") + "Error"), e.SignedInError, "OK");
				else
				{
					await continueToNextPage(true, false);
					return;
				}
			}
			else
				await continueToNextPage(true, true);
		}

		private async Task continueToNextPage(bool existingUser, bool isInternetConnected)
		{
			var cloud = (PlayOnCloud.ViewModel.Cloud)(((Cloud)Application.Current.BindingContext));

			var account = cloud.Account;
			account.OnSignInComplete -= Account_OnSignInComplete;

			Page nextPage;
			if (isInternetConnected)
			{
				if (account.SignedIn)
				{
					//All went well - continue browsing with full rights
					nextPage = new Main();
					(Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).SelectedItem = CloudItem.Content;
					if (!(Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).MediaContent.Initialized)
						await (Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).MediaContent.Init();
				}
				else
				{
					(Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).Register.Type = Model.RegisterViewType.Launch;

					//Something went wrong - show the user the login within the Register screen with his credentials so he can correct if needed
					(Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).Register.View = existingUser ? RegisterViewMode.Login : RegisterViewMode.None;
					nextPage = new Register() {BindingContext = Application.Current.BindingContext};
				}
			}
			else
			{
				nextPage = new Main();

				cloud.SelectedItem = CloudItem.Library;
				cloud.Library.SelectedView = (int) LibraryViewMode.Device;

				if (!cloud.Library.Initialized)
					await cloud.Library.Init();
			}

			await Navigation.PushAsync(nextPage);
			activityIndicator.IsRunning = false;
		}
	}
}
