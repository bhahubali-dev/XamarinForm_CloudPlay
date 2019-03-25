using PlayOnCloud.Model;
using PlayOnCloud.ViewModel;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public partial class Main : ContentPage
    {
        public Main()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = Application.Current.BindingContext;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (BindingContext is Cloud)
                if (width > height)
                    (BindingContext as Cloud).DeviceOrientation = DeviceOrientation.Landscape;
                else
                    (BindingContext as Cloud).DeviceOrientation = DeviceOrientation.Portrait;
        }
    }
}