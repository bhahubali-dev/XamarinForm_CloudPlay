using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class DetailsPage : ContentPage
	{
		public DetailsPage()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
			BackButtonHelper.BackButtonPressed += (sender, args) => Navigation.RemovePage(this);
			discoverButton.OnClicked += async (s, a) => await navigate();
			libraryButton.OnClicked += async (s, a) => await navigate();
			queueButton.OnClicked += async (s, a) => await navigate();
			notificationsButton.OnClicked += async (s, a) => await navigate();
			accountButton.OnClicked += async (s, a) => await navigate();
		}

		public void InsertChild(View child)
		{
			if (child is ContentItemDetailsView)
				(child as ContentItemDetailsView).IsInDetailsPage = true;

			mainGrid.Children.Insert(0, child);
			if (child is LibraryItemDetails)
			{
				(child as LibraryItemDetails).OnCloseRequest += DetailsPage_OnCloseRequest;
				((child as LibraryItemDetails).BindingContext as PlayOnCloud.ViewModel.Library).OnItemDeleted += DetailsPage_OnItemDeleted;
			}
		}

		private void DetailsPage_OnItemDeleted(object sender, EventArgs e)
		{
			if ((sender is PlayOnCloud.ViewModel.Library) && ((sender as PlayOnCloud.ViewModel.Library).SelectedItem == null))
				Navigation.RemovePage(this);
		}

		private void DetailsPage_OnCloseRequest(object sender, EventArgs e)
		{
			Navigation.RemovePage(this);
		}

		public void GotBackClick(object sender, EventArgs args)
		{
			Navigation.RemovePage(this);
		}

		private async Task navigate()
		{
			while ((Application.Current.MainPage as NavigationPage).CurrentPage is DetailsPage)
				await Application.Current.MainPage.Navigation.PopAsync();
		}
	}
}
