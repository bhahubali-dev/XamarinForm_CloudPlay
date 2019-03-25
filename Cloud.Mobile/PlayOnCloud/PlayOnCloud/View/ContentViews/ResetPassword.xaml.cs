using System;
using PlayOnCloud.ViewModel;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public partial class ResetPassword : ContentView
    {
        public ResetPassword()
        {
            InitializeComponent();
        }

        public async void SendClick(object sender, EventArgs args)
        {
            var theAccount = ((Cloud) BindingContext).Account;
            if (string.IsNullOrEmpty(txtEmail.Text))
            {
                await Application.Current.MainPage.DisplayAlert("Reset Password", "Please enter your email address.",
                    "OK");
                return;
            }

            await theAccount.ResetPassword(txtEmail.Text);
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("mailto:support@playon.tv"));
        }
    }
}