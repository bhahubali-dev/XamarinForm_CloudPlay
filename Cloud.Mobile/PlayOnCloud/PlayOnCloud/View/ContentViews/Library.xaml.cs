using System;
using PlayOnCloud.ViewModel;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public partial class Library : SlidingContentView
    {
        
        public Library()
        {
            InitializeComponent();
        }


        private void OnListBoxItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                cloudItemsView.ScrollTo(e.SelectedItem, ScrollToPosition.MakeVisible, true);
               
            }
                
        }

        private void Item_OnTapped(object sender, EventArgs e)
        {
            (BindingContext as Cloud).Library.ListViewItemSelected.Execute(null);
        }

        private void Imgbtn2_OnTapped(object sender, EventArgs e)
        {
            (BindingContext as Cloud).Library.SelectedView = 1;
            ImgCloud.Source = "cloud_deactive.png";
            ImgDevice.Source = "Device_active.png";
        }

        private void ImgBtn1_OnTapped(object sender, EventArgs e)
        {
            //cloud_deactive.png device_deactive.png
            //Cloud_active.png Device_active.png
            ImgCloud.Source = "Cloud_active.png";
            ImgDevice.Source = "device_deactive.png";
            (BindingContext as Cloud).Library.SelectedView = 0;
        }
    }
}