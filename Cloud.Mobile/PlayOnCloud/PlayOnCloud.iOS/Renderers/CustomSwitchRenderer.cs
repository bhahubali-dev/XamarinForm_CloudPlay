using CoreGraphics;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace PlayOnCloud.iOS.Renderers
{
	public class CustomSwitchRenderer : ViewRenderer<CustomSwitch, UISwitch>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<CustomSwitch> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				//XXX: scale not working
				UISwitch uiSwitch = new UISwitch();
				uiSwitch.Transform = new CGAffineTransform();
				uiSwitch.Transform.Scale(0.2f, 0.2f);
				SetNativeControl(uiSwitch);
			}
		}
	}
}
