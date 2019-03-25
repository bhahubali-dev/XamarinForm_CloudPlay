using System;
using PlayOnCloud.ViewModel;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public partial class Queue : SlidingContentView
    {
        public Queue()
        {
            InitializeComponent();

            var cloudViewModel = Application.Current.BindingContext as Cloud;
            var tapGestureRecognizer = new TapGestureRecognizer {NumberOfTapsRequired = 1};
            tapGestureRecognizer.Command = cloudViewModel.Queue.SelectActiveRecording;
            gridActiveRecording.GestureRecognizers.Add(tapGestureRecognizer);
        }

        private void OnListBoxItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
                queueListView.ScrollTo(e.SelectedItem, ScrollToPosition.MakeVisible, true);
        }

        private void OnAutoDownloadTapped(object sender, EventArgs args)
        {
            var cloudViewModel = Application.Current.BindingContext as Cloud;
            if (cloudViewModel != null)
                cloudViewModel.Library.AutoDownload = !cloudViewModel.Library.AutoDownload;
        }

        private void Item_OnTapped(object sender, EventArgs e)
        {
            (BindingContext as Cloud).Queue.ListViewItemSelected.Execute(null);
        }
    }
}