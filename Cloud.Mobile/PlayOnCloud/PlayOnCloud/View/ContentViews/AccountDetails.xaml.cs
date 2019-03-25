using System;
using System.Threading;
using PlayOnCloud.Model;
using PlayOnCloud.ViewModel;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public partial class AccountDetails : ContentView
    {
        private readonly SemaphoreSlim isLoggedOutSemaphore = new SemaphoreSlim(1, 1);

        public AccountDetails()
        {
            InitializeComponent();
        }

        private async void SignOut_OnClicked(object sender, EventArgs e)
        {
            try
            {
                await isLoggedOutSemaphore.WaitAsync();

                var account = BindingContext as ViewModel.Account;
                if (account != null)
                    if (account.SignedIn)
                    {
                        var signOutResult = await account.SignOutAsync();
                        if (!signOutResult.Item1)
                            await Application.Current.MainPage.DisplayAlert("Error Signing Out", signOutResult.Item2,
                                "OK");
                    }
                    else
                    {
                        LoggerService.Instance.Log(
                            "WARNING: AccountDetails.SignOut_OnClicked: Do not try to logout again ('account.SignedIn' is already false)");
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
            if (BindingContext as ViewModel.Account != null)
            {
                (Application.Current.BindingContext as Cloud).Register.View = RegisterViewMode.Login;
                (Application.Current.BindingContext as Cloud).Register.Type = RegisterViewType.Launch;

                await Navigation.PushAsync(new Register {BindingContext = Application.Current.BindingContext});
            }
        }

        private async void OnClick_ChannelSettings(object sender, EventArgs e)
        {
            // await Application.Current.MainPage.DisplayAlert("Message", "This is call Channel list.", "OK", "Cancel");
            var details = new DetailsPage {BindingContext = Application.Current.BindingContext};
            details.InsertChild(new AccountChannel());
            await Application.Current.MainPage.Navigation.PushAsync(details);
        }

        private async void DownloadOption_OnClicked(object sender, EventArgs e)
        {
            var details = new DetailsPage {BindingContext = Application.Current.BindingContext};
            details.InsertChild(new DownloadOption());
            await Application.Current.MainPage.Navigation.PushAsync(details);
        }
    }
}