using System;
using System.Linq;
using Xamarin.Forms;
using PlayOnCloud.Model;
using System.Collections.Generic;

namespace PlayOnCloud
{
	public partial class Notifications : SlidingContentView
	{
		private StackLayout currentSelectedElement;
		private DeviceOrientation lastDeviceOrientation;
		private bool isLastDeviceOrientationInit;

		public Notifications()
		{
			InitializeComponent();
		}

		private void OnListBoxItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			// XXX: Fixes 'notificationsItemsView.ScrollTo' doesn't make visible the whole selected list item
			Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					if (e.SelectedItem != null)
						notificationsItemsView.ScrollTo(e.SelectedItem, ScrollToPosition.MakeVisible, true);
				});

				return false;
			});
		}

		private void Item_OnTapped(object sender, EventArgs e)
		{
			DeviceOrientation deviceOrientation = (BindingContext as ViewModel.Cloud).DeviceOrientation;
			StackLayout prevSelectedElement = currentSelectedElement;

			if ((prevSelectedElement != null) && (prevSelectedElement.Children != null))
			{
				var details = prevSelectedElement.Children.FirstOrDefault(c => c.GetType() == typeof(NotificationItemAccordionDetails));
				if (details != null)
				{
					if (!details.IsVisible)
						prevSelectedElement = null;

					details.IsVisible = false;
					details.HeightRequest = 0;
				}
			}

			CustomViewCell currentCell = sender as CustomViewCell;
			if (currentCell != null)
			{
				currentSelectedElement = currentCell.FindByName<StackLayout>("notificationItem");
				if ((currentSelectedElement != null) && !currentSelectedElement.Equals(prevSelectedElement))
				{
					var details = currentSelectedElement.Children.FirstOrDefault(c => c.GetType() == typeof(NotificationItemAccordionDetails));
					if (details != null)
					{
						details.IsVisible = (deviceOrientation == DeviceOrientation.Landscape) ? false : !details.IsVisible;
						details.HeightRequest = (details.IsVisible) ? getSelectedStackLayoutHeight() : 0;
					}
					else
					{
						double heightRequest = getSelectedStackLayoutHeight();
						currentSelectedElement.Children.Add(new NotificationItemAccordionDetails() { BindingContext = BindingContext, HeightRequest = heightRequest, IsVisible = (heightRequest > 0) });
					}
				}
			}
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			ViewModel.Cloud cloud = (BindingContext as ViewModel.Cloud);

			if (!isLastDeviceOrientationInit && (cloud != null))
			{
				lastDeviceOrientation = cloud.DeviceOrientation;
				isLastDeviceOrientationInit = true;
			}

			if ((cloud != null) && (lastDeviceOrientation != cloud.DeviceOrientation))
			{
				lastDeviceOrientation = cloud.DeviceOrientation;
				invalidateNotificationsList();
				currentSelectedElement = null;
			}
		}

		private void invalidateNotificationsList()
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				object slectedItem = notificationsItemsView.SelectedItem;
				notificationsItemsView.SelectedItem = null;

				ICollection<Notification> notifications = notificationsItemsView.ItemsSource as ICollection<Notification>;
				notificationsItemsView.ItemsSource = new List<Cell>();
				notificationsItemsView.ItemsSource = notifications;

				notificationsItemsView.SelectedItem = slectedItem;
			});
		}

		private double getSelectedStackLayoutHeight()
		{
			if ((BindingContext is ViewModel.Cloud) && ((BindingContext as ViewModel.Cloud).DeviceOrientation == DeviceOrientation.Landscape))
				return 0;

			// XXX: Fixes wrong StackLayout height when we select messages with different type sequentially and if we use bindings in NotificationItemAccordionDetails.xaml

			if ((currentSelectedElement != null) && (currentSelectedElement.Parent != null))
			{
				Notification notification = currentSelectedElement.Parent.BindingContext as Notification;
				if ((notification.Type == NotificationType.FailedRecording) || (notification.Type == NotificationType.BrowsingError))
					return (Device.Idiom == TargetIdiom.Tablet) ? 400 : 330;
				else if (notification.Type == NotificationType.DownloadExpiring)
					return (Device.Idiom == TargetIdiom.Tablet) ? 330 : 230;

				return (Device.Idiom == TargetIdiom.Tablet) ? 390 : 310;
			}

			return 0;
		}
	}
}
