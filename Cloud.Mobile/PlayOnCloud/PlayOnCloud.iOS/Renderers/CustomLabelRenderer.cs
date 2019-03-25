using Foundation;
using ObjCRuntime;
using PlayOnCloud;
using PlayOnCloud.iOS.Renderers;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CustomLabel), typeof(CustomLabelRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class CustomLabelRenderer : LabelRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);

			CustomLabel label = (Element as CustomLabel);
			if ((Control != null) && (label != null))
			{
				if (label.LinesCount > -1)
				{
					Control.Lines = label.LinesCount;
					Control.LineBreakMode = UILineBreakMode.TailTruncation;
				}
			}

			highlightSubstring();
			setLineSpacing();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if ((e.PropertyName == CustomLabel.TextProperty.PropertyName) || (e.PropertyName == CustomLabel.BoldTextProperty.PropertyName))
			{
				highlightSubstring();
				setLineSpacing();
			}
		}

		private void setLineSpacing()
		{
			CustomLabel label = (Element as CustomLabel);
			if ((Control != null) && (label != null) && (label.LineSpacing > 0))
			{
				NSAttributedString controlText = UIElementsHelper.GetUILabelAttributedText(Control);
				if (controlText != null)
				{
					var labelString = new NSMutableAttributedString(controlText);
					var paragraphStyle = new NSMutableParagraphStyle { LineHeightMultiple = (nfloat)label.LineSpacing };

					UIElementsHelper.SetCustomLabelParagraphStyle(paragraphStyle, label);

					var style = UIStringAttributeKey.ParagraphStyle;
					var range = new NSRange(0, labelString.Length);

					labelString.AddAttribute(style, paragraphStyle, range);
					Control.AttributedText = labelString;
				}
			}
		}

		private void highlightSubstring()
		{
			UIElementsHelper.HighlightSubstring((Element as CustomLabel)?.BoldText, Control);
		}
	}
}
