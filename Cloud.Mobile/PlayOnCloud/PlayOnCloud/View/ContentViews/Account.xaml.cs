using System;
using System.Threading;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class Account : ContentView
	{
        private SemaphoreSlim isLoggedOutSemaphore = new SemaphoreSlim(1, 1);
        public Account()
		{
			InitializeComponent();
		}

		private void Channel_OnTapped(object sender, EventArgs e)
		{
			//(accountChannelsView.BindingContext as ViewModel.Cloud).Account.SelectedChannel = (Model.IContentItem)((TappedEventArgs)e).Parameter;
			//MessagingCenterManager.ShowPopup(new ChannelLoginPopup() { BindingContext = (accountChannelsView.BindingContext as ViewModel.Cloud).Account });
		}
        private async void SignOut_OnClicked(object sender, EventArgs e)
        {
            try
            {
                await isLoggedOutSemaphore.WaitAsync();

                var account = BindingContext as ViewModel.Account;
                if (account != null)
                {
                    if (account.SignedIn)
                    {
                        Tuple<bool, string> signOutResult = await account.SignOutAsync();
                        if (!signOutResult.Item1)
                            await Application.Current.MainPage.DisplayAlert("Error Signing Out", signOutResult.Item2, "OK");
                    }
                    else
                        LoggerService.Instance.Log("WARNING: AccountDetails.SignOut_OnClicked: Do not try to logout again ('account.SignedIn' is already false)");
                }
            }
            catch (Exception ex)
            {
                LoggerService.Instance.Log("ERROR: AccountDetails.SignOut_OnClicked: " + ex.Message);
            }
            finally
            {
                isLoggedOutSemaphore.Release();
            }
        }

        private async void SignIn_Clicked(object sender, EventArgs e)
        {
            if ((BindingContext as ViewModel.Account) != null)
            {
                (Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).Register.View = Model.RegisterViewMode.Login;
                (Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).Register.Type = Model.RegisterViewType.Launch;

                await Navigation.PushAsync(new Register() { BindingContext = Application.Current.BindingContext });
            }
        }

        private async void OnClick_ChannelSettings(object sender, EventArgs e)
        {
            await Application.Current.MainPage.DisplayAlert("Message", "This is call Channel list.", "OK", "Cancel");
            //  await Navigation.PushAsync(new AccountChannelList());
        }
    }
}
