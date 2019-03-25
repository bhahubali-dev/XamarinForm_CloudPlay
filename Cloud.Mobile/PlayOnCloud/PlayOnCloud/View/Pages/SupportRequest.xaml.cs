using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class SupportRequest : ContentPage
	{
		public SupportRequest()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, true);
           

            KeyboardHelperService.KeyboardShown += (sender, keyboardHeight) =>
			{
				keyboardSeparator.HeightRequest = keyboardHeight;
			};

			KeyboardHelperService.KeyboardChanged += (sender, args) =>
			{
				if (!args.Visible)
					keyboardSeparator.HeightRequest = 0;
			};

			this.Appearing += (o, args) =>
			{
				supportRequestEditor.Focus();
			};
		}

		public string SupportRequestContents => supportRequestEditor.Text;
	}
}
