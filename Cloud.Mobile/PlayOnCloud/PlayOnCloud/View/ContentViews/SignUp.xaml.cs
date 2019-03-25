using System;
using Xamarin.Forms;
using PlayOnCloud.ViewModel;

namespace PlayOnCloud
{
	public partial class SignUp : ContentView
	{
		public SignUp()
		{
			InitializeComponent();
		}

		public async void SignUpClick(object sender, EventArgs args)
		{
			var theAccount = ((Cloud)BindingContext).Account;

			if (string.IsNullOrEmpty(txtName.Text))
			{
				await Application.Current.MainPage.DisplayAlert("Sign up", "Please enter your full name.", "OK");
				return;
			}

			if (string.IsNullOrEmpty(txtEmail.Text))
			{
				await Application.Current.MainPage.DisplayAlert("Sign up", "Please enter your email address.", "OK");
				return;
			}

			if (string.IsNullOrEmpty(txtPassword.Text))
			{
				await Application.Current.MainPage.DisplayAlert("Sign up", "Please enter a password.", "OK");
				return;
			}

			if (string.IsNullOrEmpty(txtConfirmPassword.Text))
			{
				await Application.Current.MainPage.DisplayAlert("Sign up", "Please confirm your password.", "OK");
				return;
			}

			if (txtPassword.Text != txtConfirmPassword.Text)
			{
				await Application.Current.MainPage.DisplayAlert("Sign up", "Your passwords do not match.", "OK");
				return;
			}

			await theAccount.SignUpAsync(txtName.Text, txtPassword.Text, txtEmail.Text);
		}

		public async void SignUpWithFacebookClick(object sender, EventArgs args)
		{
			var theAccount = ((Cloud)BindingContext).Account;
			await theAccount.SignWithFacebook(false);
		}
	}
}
