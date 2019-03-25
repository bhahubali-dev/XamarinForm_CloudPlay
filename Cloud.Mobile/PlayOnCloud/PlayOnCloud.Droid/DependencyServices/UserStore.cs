using System;
using System.Linq;
using Xamarin.Auth;
using Xamarin.Forms;
using Android.Util;
using Plugin.CurrentActivity;
using PlayOnCloud.Droid;
using PlayOnCloud.Model;

[assembly: Dependency(typeof(UserStore))]
namespace PlayOnCloud.Droid
{
	public class UserStore : IUserStore
	{
		private const string service = "PlayOnCloudServiceID";

		public User LoadUserCredentials()
		{
			try
			{
				var account = AccountStore.Create(CrossCurrentActivity.Current.Activity).FindAccountsForService(service).FirstOrDefault();
				if (account != null)
				{
					var user = new User();
					user.Email = account.Properties["Email"];
                    user.Name = account.Properties["Name"];
                    if (account.Properties.ContainsKey("AuthToken"))
						user.AuthToken = account.Properties["AuthToken"];

					if (account.Properties.ContainsKey("Token"))
						user.Token = account.Properties["Token"];

					if (account.Properties.ContainsKey("Expires"))
					{
						long expires;
						if (long.TryParse(account.Properties["Expires"], out expires))
							user.Expires = expires;
					}

					return user;
				}
			}
			catch (Exception ex)
			{
				Log.Error(nameof(UserStore), "ERROR: LoadUserCredentials: " + ex);
			}

			return null;
		}

		public bool SaveUserCredentials(User user)
		{
			if (!string.IsNullOrEmpty(user?.Email))
			{
				try
				{
					//Make sure we kill all other accounts
					DeleteUserCredentials();

					//Save account in key chain
					var account = new Xamarin.Auth.Account();
					account.Properties.Add("Email", user.Email);
                    account.Properties.Add("Name", user.Name);
                    if (!string.IsNullOrEmpty(user.Token))
						account.Properties.Add("Token", user.Token);

					if (!string.IsNullOrEmpty(user.AuthToken))
						account.Properties.Add("AuthToken", user.AuthToken);

					if (user.Expires > 0)
						account.Properties.Add("Expires", user.Expires.ToString());

					AccountStore.Create(CrossCurrentActivity.Current.Activity).Save(account, service);
					return true;
				}
				catch (Exception ex)
				{
					Log.Error(nameof(UserStore), "ERROR: SaveUserCredentials: " + ex);
				}
			}

			return false;
		}

		public bool DeleteUserCredentials()
		{
			try
			{
				var accounts = AccountStore.Create(CrossCurrentActivity.Current.Activity).FindAccountsForService(service);
				if (accounts == null)
					return true;

				foreach (var account in accounts)
					AccountStore.Create(CrossCurrentActivity.Current.Activity).Delete(account, service);

				return true;
			}
			catch (Exception ex)
			{
				Log.Error(nameof(UserStore), "ERROR: DeleteUserCredentials: " + ex);
			}

			return false;
		}

		public bool StoreRecordInKeychain(string key, string value)
		{
			try
			{
				var account = AccountStore.Create(CrossCurrentActivity.Current.Activity).FindAccountsForService(service).FirstOrDefault();
				if (account != null)
					account.Properties[key] = value;
				else
				{
					//Save account in key chain
					account = new Xamarin.Auth.Account();
					account.Properties.Add(key, value);
					AccountStore.Create(CrossCurrentActivity.Current.Activity).Save(account, service);
				}

				return true;
			}
			catch (Exception ex)
			{
				Log.Error(nameof(UserStore), "ERROR: StoreRecordInKeychain: " + ex);
			}

			return false;
		}

		public string GetRecordFromKeychain(string key)
		{
			try
			{
				var account = AccountStore.Create(CrossCurrentActivity.Current.Activity).FindAccountsForService(service).FirstOrDefault();
				if ((account != null) && account.Properties.ContainsKey(key))
					return account.Properties[key];
			}
			catch (Exception ex)
			{
				Log.Error(nameof(UserStore), "ERROR: GetRecordFromKeychain: " + ex);
			}

			return null;
		}
	}
}