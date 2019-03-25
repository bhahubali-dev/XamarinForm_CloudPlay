using PlayOnCloud;
using PlayOnCloud.iOS.Renderers;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.ComponentModel;
using UIKit;
using CoreAnimation;
using Foundation;

[assembly: ExportRenderer(typeof(CustomActivityIndicator), typeof(CustomActivityIndicatorRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class CustomActivityIndicatorRenderer : ViewRenderer<CustomActivityIndicator, UIImageView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<CustomActivityIndicator> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				UIImageView imageView = new UIImageView();
				imageView.Image = UIImage.FromFile("loadingAnimation.png");
				SetNativeControl(imageView);

				if (e.NewElement.IsRunning)
					startAnimation();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if ((e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName) && ((Element != null) && (Control != null)))
			{
				if (Element.IsRunning)
					startAnimation();
				else
					stopAnimation();
			}
		}

		private void startAnimation()
		{
			CABasicAnimation rotationAnimation = CABasicAnimation.FromKeyPath("transform.rotation");
			rotationAnimation.To = NSNumber.FromDouble(Math.PI * 2); // full rotation (in radians)
			rotationAnimation.RepeatCount = int.MaxValue; // repeat forever
			rotationAnimation.Duration = 1;
			rotationAnimation.RemovedOnCompletion = false;
			// Give the added animation a key for referencing it later (to remove, in this case).
			Control.Layer.AddAnimation(rotationAnimation, "rotationAnimation");
		}

		private void stopAnimation()
		{
			Control.Layer.RemoveAnimation("rotationAnimation");
		}
	}
}
