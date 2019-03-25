using System;
using PlayOnCloud.Model;
using PlayOnCloud.ViewModel;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public partial class AccountChannel : ContentView
    {
        public AccountChannel()
        {
            InitializeComponent();
        }

        private void Channel_OnTapped(object sender, EventArgs e)
        {
            (accountChannelsView.BindingContext as Cloud).Account.SelectedChannel =
                (IContentItem) ((TappedEventArgs) e).Parameter;
            MessagingCenterManager.ShowPopup(new ChannelLoginPopup
            {
                BindingContext = (accountChannelsView.BindingContext as Cloud).Account
            });
        }
    }
}