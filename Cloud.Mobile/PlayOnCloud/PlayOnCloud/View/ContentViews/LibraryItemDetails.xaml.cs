using System;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class LibraryItemDetails : ContentView
	{
		public event EventHandler OnCloseRequest;

		public LibraryItemDetails()
		{
			InitializeComponent();
		}

		public void ButtonClick(object sender, EventArgs args)
		{
			Fire_OnCloseRequest();
		}

		private void Fire_OnCloseRequest()
		{
			var onCloseRequest = OnCloseRequest;
			if (onCloseRequest != null)
				onCloseRequest(this, null);
		}
	}
}
