using System;
using PlayOnCloud.ViewModel;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public partial class Discover : SlidingContentView
    {
        public Discover()
        {
            InitializeComponent();
        }

        private void Item_OnTapped(object sender, EventArgs e)
        {
            (BindingContext as Cloud).MediaContent.ListViewItemSelected.Execute(null);
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://playon.tv/privacy"));
        }
    }
}