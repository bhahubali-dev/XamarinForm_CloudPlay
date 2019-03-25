using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace PlayOnCloud
{
	public class FacebookLoginArgs : EventArgs
	{
		public bool IsAuthenticated { get; set; }
		public string Id { get; set; }
		public string Token { get; set; }
	}

	public partial class FacebookLogin : ContentPage
	{
		public OAuthSettings FacebookSettings { get; private set; }

		public FacebookLogin()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);

			FacebookSettings = new OAuthSettings("1626108114272416",
				"public_profile, email",
				"https://m.facebook.com/dialog/oauth/",
				"https://www.playon.tv");

			//FacebookSettings = new OAuthSettings("1701661513390493",
			//	"public_profile, email",
			//	"https://m.facebook.com/dialog/oauth/",
			//	"https://www.facebook.com/connect/login_success.html");
		}

		public event EventHandler<FacebookLoginArgs> OnFacebookLoginCompleted;


		public void LoginCompleted(bool isAuthenticated, string id, string token)
		{
			if (OnFacebookLoginCompleted != null)
			{
				var args = new FacebookLoginArgs
				{
					IsAuthenticated = isAuthenticated,
					Id = id,
					Token = token
				};

				OnFacebookLoginCompleted(this, args);
			}
		}
	}
}
