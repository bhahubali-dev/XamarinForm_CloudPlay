using PlayOnCloud;
using PlayOnCloud.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RoundFrame), typeof(RoundFrameRenderer))]
namespace PlayOnCloud.iOS
{
	public class RoundFrameRenderer : FrameRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);

			if ((NativeView != null) && (e.NewElement != null))
			{
				var newElement = e.NewElement as RoundFrame;
				NativeView.Layer.CornerRadius = newElement.BorderRadius;
				NativeView.Layer.BorderWidth = newElement.BorderWidth;
				NativeView.Layer.BorderColor = newElement.BorderColor.ToCGColor();
				NativeView.Layer.ShadowColor = newElement.ShadowColor.ToCGColor();
				NativeView.Layer.ShadowRadius = newElement.ShadowRadius;
				NativeView.Tag = newElement.Tag;
			}
		}
	}
}
