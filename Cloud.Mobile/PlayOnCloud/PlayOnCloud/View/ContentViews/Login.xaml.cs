using System;
using Xamarin.Forms;
using PlayOnCloud.ViewModel;

namespace PlayOnCloud
{
	public partial class Login : ContentView
	{
		private string initialUserName;
		private string initialPassword;

		public string UserName
		{
			get { return txtEmail.Text; }
			set { txtEmail.Text = value; }
		}

		public string Password
		{
			get { return txtPassword.Text; }
			set { txtPassword.Text = value; }
		}

		public Login()
		{
			InitializeComponent();

			var user = UserStoreService.Instance.LoadUserCredentials();
			if (user != null)
			{
				UserName = initialUserName = user.Email;
				if (!string.IsNullOrEmpty(user.AuthToken))
					initialPassword = Password = "*****";
			}
		}

		public async void SignInClick(object sender, EventArgs args)
		{
			var theAccount = ((Cloud)BindingContext).Account;
			var currentUser = UserStoreService.Instance.LoadUserCredentials();

			var forceInteractiveLogin = (initialUserName != UserName) || (initialPassword != Password);
			if (forceInteractiveLogin || string.IsNullOrEmpty(currentUser?.AuthToken))
			{
				if (string.IsNullOrEmpty(UserName))
				{
					await Application.Current.MainPage.DisplayAlert("Login", "Please enter your email address.", "OK");
					return;
				}

				if (string.IsNullOrEmpty(Password))
				{
					await Application.Current.MainPage.DisplayAlert("Login", "Please enter your password.", "OK");
					return;
				}

				await theAccount.SignInAsync(UserName, Password);
			}
			else
				await theAccount.SignInWithTokenAsync(currentUser);
		}

		public async void SignInWithFacebookClick(object sender, EventArgs args)
		{
			var theAccount = ((Cloud)BindingContext).Account;
			await theAccount.SignWithFacebook(true);
		}
	}
}
