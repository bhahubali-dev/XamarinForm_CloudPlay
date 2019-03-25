using System;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public partial class ChannelLoginPopup : PopupBase
    {
        public ChannelLoginPopup()
        {
            InitializeComponent();
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            Close();
        }

        public void LoginComplete(object sender, bool success)
        {
            if (success)
                Close();
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://playon.tv/privacy"));
        }
    }
}