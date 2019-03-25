using System;
using System.Linq;
using Xamarin.Auth;
using Xamarin.Forms;
using Foundation;
using Security;
using PlayOnCloud.iOS;
using PlayOnCloud.Model;

[assembly: Dependency(typeof(UserStore))]
namespace PlayOnCloud.iOS
{
	public class UserStore : IUserStore
	{
		public User LoadUserCredentials()
		{
			try
			{
				var service = NSBundle.MainBundle.InfoDictionary["CFBundleName"].ToString();
				var account = AccountStore.Create().FindAccountsForService(service).FirstOrDefault();
				if (account != null)
				{
					User user = new User();
					user.Email = account.Properties["Email"];

					if (account.Properties.ContainsKey("UserName"))
						user.Name = account.Properties["UserName"];

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
				Logger.Log("ERROR: LoadUserCredentials: " + ex);
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

					if (!string.IsNullOrWhiteSpace(user.Name))
						account.Properties.Add("UserName", user.Name);

					if (!string.IsNullOrEmpty(user.Token))
						account.Properties.Add("Token", user.Token);

					if (!string.IsNullOrEmpty(user.AuthToken))
						account.Properties.Add("AuthToken", user.AuthToken);

					if (user.Expires > 0)
						account.Properties.Add("Expires", user.Expires.ToString());

					var service = NSBundle.MainBundle.InfoDictionary["CFBundleName"].ToString();
					saveXamarinAuthAccountInternal(account, SecAccessible.AfterFirstUnlock);

					return true;
				}
				catch (Exception ex)
				{
					Logger.Log("ERROR: SaveUserCredentials: " + ex);
				}
			}

			return false;
		}

		private void saveXamarinAuthAccountInternal(Xamarin.Auth.Account account, SecAccessible access)
		{
			var accountStore = AccountStore.Create();
			var serviceId = NSBundle.MainBundle.InfoDictionary["CFBundleName"].ToString();

			var statusCode = SecStatusCode.Success;
			var serializedAccount = account.Serialize();
			var data = NSData.FromString(serializedAccount, NSStringEncoding.UTF8);

			// Remove any existing record
			var existing = accountStore.FindAccountsForService(serviceId);
			if (existing.Any())
			{
				var query = new SecRecord(SecKind.GenericPassword)
				{
					Service = serviceId,
					Account = account.Username
				};

				statusCode = SecKeyChain.Remove(query);
				if (statusCode != SecStatusCode.Success)
					throw new Exception("Could not save account to KeyChain: " + statusCode);
			}

			// Add this record
			var record = new SecRecord(SecKind.GenericPassword)
			{
				Service = serviceId,
				Account = account.Username,
				Generic = data,
				Accessible = access
			};

			statusCode = SecKeyChain.Add(record);
			if (statusCode != SecStatusCode.Success)
				throw new Exception("Could not save account to KeyChain: " + statusCode);
		}

		public bool DeleteUserCredentials()
		{
			try
			{
				var service = NSBundle.MainBundle.InfoDictionary["CFBundleName"].ToString();
				var accounts = AccountStore.Create().FindAccountsForService(service);
				if (accounts == null)
					return true;

				foreach (var account in accounts)
					AccountStore.Create().Delete(account, service);

				return true;
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: DeleteUserCredentials: " + ex);
			}

			return false;
		}

		public bool StoreRecordInKeychain(string key, string value)
		{
			return storeRecordInKeychainInternal(key, value, SecAccessible.AfterFirstUnlock);
		}

		private bool storeRecordInKeychainInternal(string key, string value, SecAccessible access)
		{
			try
			{
				var secRecord = new SecRecord(SecKind.GenericPassword)
				{
					ValueData = NSData.FromString(value),
					Account = key,
					Service = "PlayOnCloudServiceID",
					Accessible = access
				};

				var error = SecKeyChain.Add(secRecord);
				if (error == SecStatusCode.DuplicateItem)
				{
					if (SecKeyChain.Remove(secRecord) == SecStatusCode.ItemNotFound)
					{
						var oldSecRecord = new SecRecord(SecKind.GenericPassword)
						{
							ValueData = NSData.FromString(value),
							Account = key,
							Service = "PlayOnCloudServiceID"
						};

						SecKeyChain.Remove(oldSecRecord);
					}

					error = SecKeyChain.Add(secRecord);
				}

				return error == SecStatusCode.Success;
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: StoreRecordInKeychain: " + ex);
			}

			return false;
		}

		public string GetRecordFromKeychain(string key)
		{
			try
			{
				SecStatusCode res;
				var secRecord = new SecRecord(SecKind.GenericPassword)
				{
					Account = key,
					Service = "PlayOnCloudServiceID"
				};

				var match = SecKeyChain.QueryAsRecord(secRecord, out res);
				if (match != null)
					return match.ValueData.ToString();
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: GetRecordFromKeychain: " + ex);
			}

			return null;
		}
	}
}
