using System.ComponentModel;
using PlayOnCloud;
using PlayOnCloud.Droid.Controls;
using PlayOnCloud.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomScrollView), typeof(CustomScrollViewRenderer))]

namespace PlayOnCloud.Droid.Renderers
{
    public class CustomScrollViewRenderer : ScrollViewRenderer
    {
        private FormsUIRefreshControl refreshControl;

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (refreshControl != null)
                return;

            if (Element == null)
                return;

            Element.PropertyChanged += Element_PropertyChanged;
            refreshControl = new FormsUIRefreshControl();

            var customScrollView = Element as CustomScrollView;
            if (customScrollView != null)
                refreshControl.RefreshCommand = customScrollView.RefreshCommand;

            //AlwaysBounceVertical = true;
            // AddSubview(refreshControl);
        }


        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var customScrollView = Element as CustomScrollView;
            if (customScrollView == null)
                return;

            if (e.PropertyName == CustomScrollView.IsRefreshingProperty.PropertyName)
                refreshControl.IsRefreshing = customScrollView.IsRefreshing;
            else if (e.PropertyName == CustomScrollView.RefreshCommandProperty.PropertyName)
                refreshControl.RefreshCommand = customScrollView.RefreshCommand;
            //else if (e.PropertyName == CustomScrollView.DisableBouncesProperty.PropertyName)
            //  Bounces = !customScrollView.DisableBounces;
        }
    }
}