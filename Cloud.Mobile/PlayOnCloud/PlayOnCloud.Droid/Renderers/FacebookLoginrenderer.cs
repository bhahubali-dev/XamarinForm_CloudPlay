using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Auth;
using Android.Util;
using Java.IO;

[assembly: Xamarin.Forms.ExportRenderer(typeof(PlayOnCloud.FacebookLogin), typeof(PlayOnCloud.Droid.Renderers.FacebookLoginrenderer))]
namespace PlayOnCloud.Droid.Renderers
{
	class FacebookLoginrenderer : PageRenderer
	{
		private bool isShown;

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			base.OnElementChanged(e);

			var activity = this.Context as Activity;
			if (!isShown && (Element != null))
			{
				isShown = true;
				FacebookLogin facebookLogin = (FacebookLogin)Element;

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
								var response = t.Result.GetResponseStream();
								JsonReader reader = new JsonReader(new InputStreamReader(response, "UTF-8"));

								reader.BeginObject();
								while (reader.HasNext)
								{
									var name = reader.NextName();
									if (name.Equals("id"))
									{
										id = reader.NextString();
										break;
									}
									else
										reader.SkipValue();
								}
								reader.EndObject();
							}
						});
					}

					facebookLogin.LoginCompleted(eventArgs.IsAuthenticated, id, token);
				};

				activity.StartActivity(auth.GetUI(activity));
			}
		}
	}
}