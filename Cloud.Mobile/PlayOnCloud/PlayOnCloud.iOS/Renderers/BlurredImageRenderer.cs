using CoreImage;
using PlayOnCloud;
using PlayOnCloud.iOS.Renderers;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BlurredImage), typeof(BlurredImageRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class BlurredImageRenderer : ImageRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);

			if ((Element != null) && (Element.Source != null) && (Control != null))
				setupImage(Element.Source, Control);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if ((e.PropertyName == Image.SourceProperty.PropertyName) &&
				((Element != null) && (Element.Source != null) && (Control != null)))
				setupImage(Element.Source, Control);
		}

		private void setupImage(ImageSource source, UIImageView target)
		{
			IImageSourceHandler handler = null;
			if (source is UriImageSource)
				handler = new ImageLoaderSourceHandler();
			else if (source is FileImageSource)
				handler = new FileImageSourceHandler();
			else if (source is StreamImageSource)
				handler = new StreamImagesourceHandler();

			if (handler != null)
			{
				UIImage image = target.Image;
				if (image != null)
				{
					using (var context = CIContext.Create())
					using (var inputImage = CIImage.FromCGImage(image.CGImage))
					using (var filter = new CIGaussianBlur() { Image = inputImage, Radius = 15 })
					using (var resultImage = context.CreateCGImage(filter.OutputImage, inputImage.Extent))
						target.Image = new UIImage(resultImage);
				}
			}
		}
	}
}
