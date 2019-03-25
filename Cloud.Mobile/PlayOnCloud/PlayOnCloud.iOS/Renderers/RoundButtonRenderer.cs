using PlayOnCloud;
using PlayOnCloud.iOS.Renderers;
using PlayOnCloud.iOS.Tools;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RoundButton), typeof(RoundButtonRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class RoundButtonRenderer : ButtonRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if ((Control != null) && (e.NewElement != null))
			{
				Control.Tag = ((RoundButton)e.NewElement).Tag;
				Control.TouchUpInside += (sender, ev) =>
				{
					var responder = InputTools.FindFirstResponder(UIApplication.SharedApplication.KeyWindow.RootViewController.View);
					if (responder != null)
						responder.ResignFirstResponder();
				};
			}
		}
	}
}
