using PlayOnCloud;
using PlayOnCloud.iOS.Controls;
using PlayOnCloud.iOS.Renderers;
using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SegmentedControl), typeof(SegmentedControlRenderer))]

namespace PlayOnCloud.iOS.Renderers
{
	public class SegmentedControlRenderer : ViewRenderer<SegmentedControl, ITSegmentedControl>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<SegmentedControl> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				ITSegmentedControl segmentedControl = new ITSegmentedControl();

				for (var i = 0; i < e.NewElement.Children.Count; i++)
				{
					UIImage image = null;
					if (!string.IsNullOrEmpty(e.NewElement.Children[i].NormalImage))
						image = UIImage.FromFile(e.NewElement.Children[i].NormalImage);

					UIImage selectedImage = null;
					if (!string.IsNullOrEmpty(e.NewElement.Children[i].SelectedImage))
						selectedImage = UIImage.FromFile(e.NewElement.Children[i].SelectedImage);

					segmentedControl.AddSegmentWithTitle(e.NewElement.Children[i].Text, selectedImage, image);
				}

				UIFont uiFont = Font.Default.ToUIFont();
				if (uiFont != null)
				{
					UIFont scFont = UIFont.FromName(uiFont.Name, (nfloat)e.NewElement.FontSize);
					segmentedControl.TitleFont = scFont;
				}

				segmentedControl.NormalColor = e.NewElement.NormalColor.ToUIColor();
				segmentedControl.SelectedColor = e.NewElement.SelectedColor.ToUIColor();
				segmentedControl.NormalTextColor = e.NewElement.NormalTextColor.ToUIColor();
				segmentedControl.SelectedTextColor = e.NewElement.SelectedTextColor.ToUIColor();
				segmentedControl.Layer.BorderColor = e.NewElement.BorderColor.ToCGColor();
				segmentedControl.Layer.BorderWidth = e.NewElement.BorderWidth;
				segmentedControl.Layer.CornerRadius = e.NewElement.BorderRadius;

				segmentedControl.ValueChanged += (sender, eventArgs) =>
				{
					e.NewElement.SelectedValue = segmentedControl.IndexOfSelectedSegment;
				};

				segmentedControl.SelectSegmentAtIndex(e.NewElement.SelectedValue);
				SetNativeControl(segmentedControl);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == SegmentedControl.SelectedValueProperty.PropertyName)
				Control.SelectSegmentAtIndex(Element.SelectedValue);
		}
	}
}
