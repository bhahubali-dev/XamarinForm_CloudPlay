using PlayOnCloud;
using PlayOnCloud.iOS.Controls;
using PlayOnCloud.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomScrollView), typeof(CustomScrollViewRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class CustomScrollViewRenderer : ScrollViewRenderer
	{
		FormsUIRefreshControl refreshControl = null;

		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			if (refreshControl != null)
				return;

			if (Element == null)
				return;

			Element.PropertyChanged += Element_PropertyChanged;
			refreshControl = new FormsUIRefreshControl();

			CustomScrollView customScrollView = (Element as CustomScrollView);
			if (customScrollView != null)
			{
				refreshControl.RefreshCommand = customScrollView.RefreshCommand;
				Bounces = !customScrollView.DisableBounces;
			}

			AlwaysBounceVertical = true;
			AddSubview(refreshControl);
		}

		private void Element_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var customScrollView = Element as CustomScrollView;
			if (customScrollView == null)
				return;

			if (e.PropertyName == CustomScrollView.IsRefreshingProperty.PropertyName)
				refreshControl.IsRefreshing = customScrollView.IsRefreshing;
			else if (e.PropertyName == CustomScrollView.RefreshCommandProperty.PropertyName)
				refreshControl.RefreshCommand = customScrollView.RefreshCommand;
			else if (e.PropertyName == CustomScrollView.DisableBouncesProperty.PropertyName)
				Bounces = !customScrollView.DisableBounces;
		}
	}
}
