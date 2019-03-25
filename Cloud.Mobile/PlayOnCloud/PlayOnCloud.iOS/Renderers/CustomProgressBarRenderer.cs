using CoreGraphics;
using PlayOnCloud;
using PlayOnCloud.iOS.Renderers;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomProgressBar), typeof(CustomProgressBarRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class CustomProgressBarRenderer : ProgressBarRenderer
	{
		public CustomProgressBarRenderer()
		{
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			try
			{
				CustomProgressBar customProgressBar = Element as CustomProgressBar;
				if (customProgressBar != null)
				{
					float scaleCoef = (float)customProgressBar.CustomScale;
					if ((scaleCoef > 1f) && ((Transform == null) || (Transform.yy != scaleCoef)))
						Transform = CGAffineTransform.MakeScale(1.0f, scaleCoef);
				}
			}
			catch
			{
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement == null)
				return;

			if (Control != null)
				UpdateBarColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == CustomProgressBar.ProgressTintColorProperty.PropertyName)
				UpdateBarColor();
		}

		private void UpdateBarColor()
		{
			var element = Element as CustomProgressBar;
			Control.ProgressTintColor = element.ProgressTintColor.ToUIColor();
		}
	}
}
