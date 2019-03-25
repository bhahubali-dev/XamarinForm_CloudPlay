using System;
using PlayOnCloud.ViewModel;

namespace PlayOnCloud
{
    public partial class ChannelLoginHelp : PopupBase
    {
        public ChannelLoginHelp()
        {
            InitializeComponent();
        }

        public async void ContactSupportClick(object sender, EventArgs args)
        {
            Close();
            await MessagingCenterManager.CloseAllPopups();
            if (BindingContext is MediaContent)
                (BindingContext as MediaContent).Account.SubmitSupportRequest.Execute(null);
        }

        public void GotBackClick(object sender, EventArgs args)
        {
            Close();
        }
    }
}