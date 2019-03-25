using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using PlayOnCloud;
using PlayOnCloud.iOS.Renderers;
using PlayOnCloud.iOS.Controls;
using UIKit;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CheckBox), typeof(CheckBoxRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class CheckBoxRenderer : ViewRenderer<CheckBox, CheckBoxView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<CheckBox> e)
		{
			base.OnElementChanged(e);

			if (Element == null)
				return;

			BackgroundColor = Element.BackgroundColor.ToUIColor();
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var checkBox = new CheckBoxView(Bounds);
					checkBox.TouchUpInside += (s, args) => Element.Checked = Control.Checked;
					checkBox.SetImage(UIImage.FromFile(e.NewElement.CheckedImage), UIControlState.Selected);
					checkBox.SetImage(UIImage.FromFile(e.NewElement.UnCheckedImage), UIControlState.Normal);

					SetNativeControl(checkBox);
				}

				Control.Checked = e.NewElement.Checked;
			}

			Control.Frame = Frame;
			Control.Bounds = Bounds;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == CheckBox.CheckedProperty.PropertyName)
				Control.Checked = Element.Checked;

			if (e.PropertyName == CheckBox.CheckedImageProperty.PropertyName)
				Control.SetImage(UIImage.FromFile(Element.CheckedImage), UIControlState.Selected);

			if (e.PropertyName == CheckBox.UnCheckedImageProperty.PropertyName)
				Control.SetImage(UIImage.FromFile(Element.UnCheckedImage), UIControlState.Normal);
		}
	}
}
