using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace PlayOnCloud.iOS
{
	internal static class FormsViewHelper
	{
		public static UIView ConvertFormsToNative(View view, CGRect size, out IVisualElementRenderer renderer)
		{
			renderer = Platform.CreateRenderer(view);
			renderer.NativeView.Frame = size;
			renderer.NativeView.AutoresizingMask = UIViewAutoresizing.All;
			renderer.NativeView.ContentMode = UIViewContentMode.ScaleToFill;
			renderer.Element.Layout(size.ToRectangle());

			var nativeView = renderer.NativeView;
			nativeView.SetNeedsLayout();
			return nativeView;
		}
	}
}
