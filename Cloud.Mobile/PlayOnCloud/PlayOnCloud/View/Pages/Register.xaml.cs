using System;
using System.Linq;
using System.Threading.Tasks;
using PlayOnCloud.Model;
using PlayOnCloud.ViewModel;
using Xamarin.Forms;
//using Android.Content;

namespace PlayOnCloud
{
    public partial class Register : ContentPage
    {
        public Register()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            var account = ((Cloud) Application.Current.BindingContext).Account;
            account.OnSignInComplete += Account_OnSignInComplete;
            account.OnResetPasswordSuccess += Account_OnResetPasswordSuccess;
        }

        public bool SkipMainPageInitialization { get; set; }

        private async void Account_OnSignInComplete(object sender, SignInArgs e)
        {
            if (!e.IsSignedIn)
            {
                var isInternetConnected = await RestService.Instance.GetIsConnected();
                if (isInternetConnected)
                {
                    if (e.SignedInError.Length > 0)
                        await Application.Current.MainPage.DisplayAlert(
                            (e.ErrorType == SignInAErrorType.AccountCreationError ? "Account Creation " : "Login ") +
                            "Error", e.SignedInError, "OK");

                    var registerModel = ((Cloud) Application.Current.BindingContext).Register;
                    if (registerModel.IsLoading)
                        registerModel.IsLoading = false;

                    return;
                }
            }

            await loadMainPage();
        }

        private void Account_OnResetPasswordSuccess(object sender, string email)
        {
            ((Cloud) Application.Current.BindingContext).Register.View = RegisterViewMode.Login;
        }

        public async void SkipRegister(object sender, EventArgs args)
        {
            if (((Cloud) Application.Current.BindingContext).Register.Type == RegisterViewType.Queue)
                ((Cloud) Application.Current.BindingContext).Register.View = RegisterViewMode.Login;
            else
                await loadMainPage();
        }

        private async Task loadMainPage()
        {
            var registerModel = ((Cloud) Application.Current.BindingContext).Register;
            if (!registerModel.IsLoading)
                registerModel.IsLoading = true;

            var account = ((Cloud) Application.Current.BindingContext).Account;
            ((Cloud) Application.Current.BindingContext).SelectedItem = CloudItem.Content;
            account.OnSignInComplete -= Account_OnSignInComplete;

            Main mainPage = null;

            var queryMainPage = Navigation.NavigationStack.Where(page => typeof(Main) == page.GetType());
            if (queryMainPage.Any())
            {
                if (!SkipMainPageInitialization)
                    await (Application.Current.BindingContext as Cloud).MediaContent.Init();

                await Navigation.PopAsync();
            }
            else
            {
                // make it async as it is slow
                await Task.Run(() => { mainPage = new Main(); });

                if (!(Application.Current.BindingContext as Cloud).MediaContent.Initialized)
                    await (Application.Current.BindingContext as Cloud).MediaContent.Init();

                await Navigation.PushAsync(mainPage);
            }

            registerModel.IsLoading = false;
            Navigation.RemovePage(this);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (BindingContext is Cloud)
                if (width > height)
                    (BindingContext as Cloud).DeviceOrientation = DeviceOrientation.Landscape;
                else
                    (BindingContext as Cloud).DeviceOrientation = DeviceOrientation.Portrait;
        }

        public void SignUpWithFacebookClick(object sender, EventArgs args)
        {
            signUpForm?.SignUpWithFacebookClick(sender, args);
        }

        public void OnBackPressed()
        {
            if ((BindingContext as Cloud).Register.Type == RegisterViewType.Queue)
                if ((Application.Current.MainPage as NavigationPage).CurrentPage is Register &&
                    Application.Current.MainPage.Navigation.NavigationStack.Count > 2)
                    Application.Current.MainPage.Navigation.PopAsync();
        }


        private void PanGestureRecognizer_OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            Device.OpenUri(new Uri("https://www.playon.tv/cloud-tos"));           
        }
    }
}