using System;
using PlayOnCloud.ViewModel;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public partial class ChannelLogin : ContentView
    {
        public ChannelLogin()
        {
            InitializeComponent();
        }

        public event EventHandler<bool> OnLoginComplete;

        public async void DoLogin(object sender, EventArgs args)
        {
            if (BindingContext is MediaContent)
            {
                var context = BindingContext as MediaContent;
                context.ChannelLogin.Execute(context.SelectedChannel);
                OnLoginComplete?.Invoke(this, true);
            }
            else if (BindingContext is ViewModel.Account)
            {
                var context = BindingContext as ViewModel.Account;
                var success = await context.PerformChannelLogin(context.SelectedChannel);
                OnLoginComplete?.Invoke(this, success);
            }
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            var url = ChannelUrl.Text;
            Device.OpenUri(new Uri(url));
        }
    }
}