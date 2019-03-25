using CoreGraphics;
using PlayOnCloud;
using PlayOnCloud.iOS;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ImageButton), typeof(ImageButtonRenderer))]
namespace PlayOnCloud.iOS
{
	public partial class ImageButtonRenderer : ButtonRenderer
	{
		private const int ControlPadding = 5;
		private const string iPad = "iPad";

		/// <summary>
		/// Gets the underlying element typed as an <see cref="ImageButton"/>.
		/// </summary>
		protected ImageButton ImageButton => (ImageButton)Element;

		protected override async void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			var imageButton = this.ImageButton;
			if (imageButton != null)
			{
				if ((Control != null) && imageButton.SupportCheckedState)
				{
					SetNativeControl(UIButton.FromType(UIButtonType.Custom));
					Control.SetTitle(Element.Text, UIControlState.Normal);
				}

				setHorizontalAlignment();
				setContentMargin();

				var targetButton = Control;
				if ((targetButton != null) && (imageButton.Source != null))
				{
					targetButton.LineBreakMode = UIKit.UILineBreakMode.Clip;
					targetButton.AdjustsImageWhenHighlighted = false;

					var width = imageButton.RenderWidth;
					var height = imageButton.RenderHeight;
					await SetupImages(imageButton, targetButton, width, height);

					switch (imageButton.Orientation)
					{
						case ImageOrientation.ImageToLeft:
							AlignToLeft(targetButton);
							break;
						case ImageOrientation.ImageToRight:
							AlignToRight(imageButton.ImageWidthRequest, targetButton);
							break;
						case ImageOrientation.ImageOnTop:
							AlignToTop(height, width, targetButton);
							break;
						case ImageOrientation.ImageOnBottom:
							AlignToBottom(height, width, targetButton);
							break;
					}
				}

				highlightSubstring();
			}
		}

		protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == ImageButton.SourceProperty.PropertyName ||
				e.PropertyName == ImageButton.DisabledSourceProperty.PropertyName)
			{
				var sourceButton = Element as ImageButton;
				if ((sourceButton != null) && (sourceButton.Source != null))
				{
					var imageButton = ImageButton;
					var targetButton = Control;

					if ((imageButton != null) && (targetButton != null) && (imageButton.Source != null))
						await SetupImages(imageButton, targetButton, imageButton.RenderWidth, imageButton.RenderHeight);
				}
			}
			else if (e.PropertyName == ImageButton.HorizontalContentAlignmentProperty.PropertyName)
				setHorizontalAlignment();
			else if (e.PropertyName == ImageButton.ContentMarginProperty.PropertyName)
				setContentMargin();
			else if ((e.PropertyName == ImageButton.TextProperty.PropertyName) || (e.PropertyName == ImageButton.BoldTextProperty.PropertyName))
				highlightSubstring();
		}

		private void setHorizontalAlignment()
		{
			var sourceButton = Element as ImageButton;
			if ((sourceButton != null) && (Control != null))
			{
				if (sourceButton.HorizontalContentAlignment == TextAlignment.Start)
					Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
				else if (sourceButton.HorizontalContentAlignment == TextAlignment.End)
					Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
				else
					Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
			}
		}

		private void setContentMargin()
		{
			var sourceButton = Element as ImageButton;
			if ((sourceButton != null) && (sourceButton.ContentMargin != null) && (Control != null))
			{
				Control.TitleEdgeInsets = new UIEdgeInsets((float)sourceButton.ContentMargin.Top,
															(float)sourceButton.ContentMargin.Left,
															(float)sourceButton.ContentMargin.Bottom,
															(float)sourceButton.ContentMargin.Right);
			}
		}

		async Task SetupImages(ImageButton imageButton, UIButton targetButton, int width, int height)
		{
			await SetImageAsync(imageButton.Source, width, height, targetButton, UIControlState.Normal);
			if (imageButton.DisabledSource != null)
				await SetImageAsync(imageButton.DisabledSource ?? imageButton.Source, width, height, targetButton, UIControlState.Disabled);

			if ((Control != null) &&
				(Control.Layer != null) &&
				(imageButton.ShadowOffset > -1) &&
				(imageButton.ShadowOpacity > -1) &&
				(imageButton.ShadowRadius > -1))
			{
				Control.Layer.ShadowColor = imageButton.ShadowColor.ToCGColor();
				Control.Layer.ShadowOffset = new CGSize(imageButton.ShadowOffset, imageButton.ShadowOffset);
				Control.Layer.ShadowOpacity = imageButton.ShadowOpacity;
				Control.Layer.ShadowRadius = imageButton.ShadowRadius;
			}
		}

		private void AlignToLeft(UIButton targetButton)
		{
			targetButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
			targetButton.TitleLabel.TextAlignment = UITextAlignment.Left;

			if (ImageButton.SupportCheckedState && !ImageButton.Checked)
				targetButton.TitleLabel.Alpha = (nfloat)ImageButton.UncheckedAlpha;

			var titleInsets = new UIEdgeInsets(0, ControlPadding, 0, -1 * ControlPadding);
			targetButton.TitleEdgeInsets = titleInsets;
		}

		private void AlignToRight(int widthRequest, UIButton targetButton)
		{
			targetButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
			targetButton.TitleLabel.TextAlignment = UITextAlignment.Right;

			if (ImageButton.SupportCheckedState && !ImageButton.Checked)
				targetButton.TitleLabel.Alpha = (nfloat)ImageButton.UncheckedAlpha;

			var titleInsets = new UIEdgeInsets(0, 0, 0, widthRequest + ControlPadding);

			targetButton.TitleEdgeInsets = titleInsets;
			var imageInsets = new UIEdgeInsets(0, widthRequest, 0, -1 * widthRequest);
			targetButton.ImageEdgeInsets = imageInsets;
		}

		private void AlignToTop(int heightRequest, int widthRequest, UIButton targetButton)
		{
			targetButton.VerticalAlignment = UIControlContentVerticalAlignment.Top;
			targetButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
			targetButton.TitleLabel.TextAlignment = UITextAlignment.Center;
			targetButton.SizeToFit();

			if (ImageButton.SupportCheckedState && !ImageButton.Checked)
				targetButton.TitleLabel.Alpha = (nfloat)ImageButton.UncheckedAlpha;

			var titleWidth = targetButton.TitleLabel.IntrinsicContentSize.Width;
			var titleHeight = targetButton.TitleLabel.IntrinsicContentSize.Height;
			int heightPadding = ((int)ImageButton.HeightRequest - (heightRequest + (int)titleHeight)) / 2;

			UIEdgeInsets titleInsets;
			UIEdgeInsets imageInsets;

			if (UIDevice.CurrentDevice.Model.Contains(iPad))
			{
				titleInsets = new UIEdgeInsets(heightRequest + ControlPadding + heightPadding,
					Convert.ToInt32(-1 * widthRequest / 2),
					-1 * (heightRequest + ControlPadding + heightPadding),
					Convert.ToInt32(widthRequest / 2));

				imageInsets = new UIEdgeInsets(heightPadding,
					Convert.ToInt32(titleWidth / 2),
					-1 * heightPadding,
					-1 * Convert.ToInt32(titleWidth / 2));
			}
			else
			{
				titleInsets = new UIEdgeInsets(heightRequest + ControlPadding + heightPadding,
					Convert.ToInt32(-1 * widthRequest / 2),
					-1 * (heightRequest + ControlPadding + heightPadding),
					Convert.ToInt32(widthRequest / 2));

				imageInsets = new UIEdgeInsets(heightPadding,
					titleWidth / 2,
					-1 * heightPadding,
					-1 * titleWidth / 2);
			}

			targetButton.TitleEdgeInsets = titleInsets;
			targetButton.ImageEdgeInsets = imageInsets;
		}

		private void AlignToBottom(int heightRequest, int widthRequest, UIButton targetButton)
		{
			targetButton.VerticalAlignment = UIControlContentVerticalAlignment.Bottom;
			targetButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
			targetButton.TitleLabel.TextAlignment = UITextAlignment.Center;
			targetButton.SizeToFit();

			if (ImageButton.SupportCheckedState && !ImageButton.Checked)
				targetButton.TitleLabel.Alpha = (nfloat)ImageButton.UncheckedAlpha;

			var titleWidth = targetButton.TitleLabel.IntrinsicContentSize.Width;
			var titleHeight = targetButton.TitleLabel.IntrinsicContentSize.Height;
			int heightPadding = ((int)ImageButton.HeightRequest - (heightRequest + (int)titleHeight)) / 2;

			UIEdgeInsets titleInsets;
			UIEdgeInsets imageInsets;

			if (UIDevice.CurrentDevice.Model.Contains(iPad))
			{
				titleInsets = new UIEdgeInsets(-1 * (heightRequest + ControlPadding + heightPadding),
					Convert.ToInt32(-1 * widthRequest / 2),
					heightRequest + ControlPadding + heightPadding,
					Convert.ToInt32(widthRequest / 2));

				imageInsets = new UIEdgeInsets(-heightPadding,
					Convert.ToInt32(titleWidth / 2),
					heightPadding,
					-1 * Convert.ToInt32(titleWidth / 2));
			}
			else
			{
				titleInsets = new UIEdgeInsets(-1 * (heightRequest + ControlPadding + heightPadding),
				   Convert.ToInt32(-1 * widthRequest / 2),
				   heightRequest + ControlPadding + heightPadding,
				   Convert.ToInt32(widthRequest / 2));

				imageInsets = new UIEdgeInsets(-heightPadding,
					titleWidth / 2,
					heightPadding,
					-1 * titleWidth / 2);
			}

			targetButton.TitleEdgeInsets = titleInsets;
			targetButton.ImageEdgeInsets = imageInsets;
		}

		private async Task SetImageAsync(ImageSource source, int widthRequest, int heightRequest, UIButton targetButton, UIControlState state = UIControlState.Normal)
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
				using (UIImage image = await handler.LoadImageAsync(source, default(System.Threading.CancellationToken), (float)UIScreen.MainScreen.Scale))
				{
					if (image != null)
					{
						UIImage scaled = image;
						if ((heightRequest > 0) && (widthRequest > 0) && ((image.Size.Height != heightRequest) || (image.Size.Width != widthRequest)))
							scaled = scaled.Scale(new CGSize(widthRequest, heightRequest), UIScreen.MainScreen.Scale);

						if (ImageButton.SupportCheckedState && !ImageButton.Checked)
						{
							UIGraphics.BeginImageContextWithOptions(scaled.Size, false, scaled.CurrentScale);
							var context = UIGraphics.GetCurrentContext();
							var area = new CGRect(0, 0, scaled.Size.Width, scaled.Size.Height);
							context.ScaleCTM(1.0f, -1.0f);
							context.TranslateCTM(0, -scaled.Size.Height);
							context.SetBlendMode(CGBlendMode.Multiply);
							context.SetAlpha((nfloat)ImageButton.UncheckedAlpha);
							context.DrawImage(area, scaled.CGImage);

							var newImage = UIGraphics.GetImageFromCurrentImageContext();
							targetButton.SetImage(newImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal), state);
						}
						else
							targetButton.SetImage(scaled.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal), state);
					}
				}
			}
		}

		/// <summary>
		/// Layouts the subviews.
		/// </summary>
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			if (ImageButton.Orientation == ImageOrientation.ImageToRight)
			{
				var imageInsets = new UIEdgeInsets(0, Control.Frame.Size.Width - ControlPadding - ImageButton.ImageWidthRequest, 0, 0);
				Control.ImageEdgeInsets = imageInsets;
			}
		}

		private void highlightSubstring()
		{
			UIElementsHelper.HighlightSubstring((Element as ImageButton)?.BoldText, Control?.TitleLabel);
		}
	}
}