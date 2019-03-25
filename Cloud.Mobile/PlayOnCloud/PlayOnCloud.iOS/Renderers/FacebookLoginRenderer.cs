#define USE_NATIVE_FACEBOOK_LOGIN

using System;
using Foundation;
using PlayOnCloud;
using PlayOnCloud.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Facebook.LoginKit;
using Accounts;
using System.Threading;
using Social;

[assembly: ExportRenderer(typeof(FacebookLogin), typeof(FacebookLoginRenderer))]
namespace PlayOnCloud.iOS
{
	class FacebookLoginRenderer : PageRenderer
	{
		private bool isShown;

		private string parseFacebookUserIdFromSettings(ACAccount account)
		{
			SLRequest sl = SLRequest.Create(SLServiceKind.Facebook, SLRequestMethod.Get, new NSUrl("https://graph.facebook.com/me"), null);
			sl.Account = account;

			AutoResetEvent completedEvent = new AutoResetEvent(false);
			var id = string.Empty;

			sl.PerformRequest((data, response, error) =>
			{
				if (error == null)
				{
					NSError parseError;
					NSDictionary jsonDict = (NSDictionary)NSJsonSerialization.Deserialize(data, 0, out parseError);
					if (jsonDict != null)
					{
						NSObject obj = jsonDict.ValueForKey(new NSString("id"));
						id = obj?.ToString();
					}
				}

				completedEvent.Set();
			});

			completedEvent.WaitOne();
			return id;
		}

		private Tuple<string, string> verifyFacebookFromSerttings()
		{
			Tuple<string, string> signedInResult = Tuple.Create<string, string>(string.Empty, string.Empty);
			AutoResetEvent taskCompleted = new AutoResetEvent(false);

			var options = new AccountStoreOptions() { FacebookAppId = "1626108114272416" };
			options.SetPermissions(ACFacebookAudience.Friends, new[] { "public_profile", "email" });

			ACAccountStore accountStore = new ACAccountStore();
			var accountType = accountStore.FindAccountType(ACAccountType.Facebook);

			var facebookAccounts = accountStore.FindAccounts(accountType);
			if ((facebookAccounts == null) || (facebookAccounts.Length == 0))
			{
				accountStore.RequestAccess(accountType, options, (granted, error2) =>
				{
					if (granted)
						facebookAccounts = accountStore.FindAccounts(accountType);

					taskCompleted.Set();
				});

				taskCompleted.WaitOne();
			}

			if ((facebookAccounts != null) && (facebookAccounts.Length > 0))
			{
				var facebookAccount = facebookAccounts[0];
				accountStore.RenewCredentials(facebookAccount, (result, error1) =>
				{
					if (result == ACAccountCredentialRenewResult.Renewed)
					{
						var id = parseFacebookUserIdFromSettings(facebookAccount);
						if (!string.IsNullOrEmpty(id))
							signedInResult = Tuple.Create<string, string>(facebookAccount.Credential.OAuthToken, id);
					}

					taskCompleted.Set();
				});
			}
			else
				taskCompleted.Set();

			taskCompleted.WaitOne();
			return signedInResult;
		}

		public override async void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			if (!isShown && (Element != null))
			{
				isShown = true;
				FacebookLogin facebookLogin = (FacebookLogin)Element;

#if USE_NATIVE_FACEBOOK_LOGIN

				try
				{
					var signedIn = verifyFacebookFromSerttings();
					if (string.IsNullOrEmpty(signedIn.Item1) || string.IsNullOrEmpty(signedIn.Item2))
					{
						LoginManager manager = new LoginManager()
						{
							LoginBehavior = LoginBehavior.Native
						};

						manager.Init();
						LoginManagerLoginResult result = await manager.LogInWithReadPermissionsAsync(new string[] { "public_profile", "email" }, null);

						var id = string.Empty;
						var token = string.Empty;
						var isAuthenticated = !result.IsCancelled && (result.Token != null);

						if (isAuthenticated)
						{
							token = result.Token.TokenString;
							id = result.Token.UserID;
						}

						facebookLogin.LoginCompleted(isAuthenticated, id, token);
					}
					else
						facebookLogin.LoginCompleted(true, signedIn.Item2, signedIn.Item1);
				}
				catch
				{
					facebookLogin.LoginCompleted(false, string.Empty, string.Empty);
				}
#else
				var auth = new OAuth2Authenticator(
					clientId: facebookLogin.FacebookSettings.ClientId,
					scope: facebookLogin.FacebookSettings.Scope,
					authorizeUrl: new Uri(facebookLogin.FacebookSettings.AuthorizeUrl),
					redirectUrl: new Uri(facebookLogin.FacebookSettings.RedirectUrl));

				auth.Completed += async (sender, eventArgs) =>
				{
					string id = string.Empty, token = String.Empty;
					if (eventArgs.IsAuthenticated)
					{
						token = eventArgs.Account.Properties["access_token"];

						var request = new OAuth2Request("GET", new Uri("https://graph.facebook.com/me?fields=name"), null, eventArgs.Account);
						await request.GetResponseAsync().ContinueWith(t =>
						{
							if (!t.IsFaulted)
							{
								string response = t.Result.GetResponseText();
								NSData responseData = NSData.FromString(response, NSStringEncoding.UTF8);

								NSError error;
								NSDictionary jsonDict = (NSDictionary)NSJsonSerialization.Deserialize(responseData, 0, out error);

								if (jsonDict != null)
								{
									NSObject obj = jsonDict.ValueForKey(new NSString("id"));
									id = obj?.ToString();
								}
							}
						});
					}
					
					facebookLogin.LoginCompleted(eventArgs.IsAuthenticated, id, token);
				};

				PresentViewController(auth.GetUI(), true, null);
#endif
			}
		}
	}
}
