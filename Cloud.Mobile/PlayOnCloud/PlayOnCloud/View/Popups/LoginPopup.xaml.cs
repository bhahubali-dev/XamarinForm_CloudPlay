using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class LoginPopup : PopupBase
	{
		public LoginPopup()
		{
			InitializeComponent();
		}

		private void Background_OnTapped(object sender, EventArgs e)
		{
			Close();
		}

		private void Button_OnClicked(object sender, EventArgs e)
		{
			Close();
		}
	}
}
