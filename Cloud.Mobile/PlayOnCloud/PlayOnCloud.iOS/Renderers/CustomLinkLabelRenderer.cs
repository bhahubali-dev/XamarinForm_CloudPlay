using EvanRobertson.TTTAttributedLabel;
using Foundation;
using PlayOnCloud;
using PlayOnCloud.iOS.Renderers;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CustomLinkLabel), typeof(CustomLinkLabelRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class CustomLinkLabelRenderer : LabelRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);

			if (Element == null)
				return;

			if (e.NewElement != null)
			{
				var attributedLabel = new TTTAttributedLabel();
				setupText(attributedLabel, e.NewElement as CustomLinkLabel);
				SetNativeControl(attributedLabel);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if ((e.PropertyName == CustomLinkLabel.TextProperty.PropertyName) ||
				(e.PropertyName == CustomLinkLabel.LinkColorProperty.PropertyName) ||
				(e.PropertyName == CustomLinkLabel.UrlProperty.PropertyName) ||
				(e.PropertyName == CustomLinkLabel.UrlTextProperty.PropertyName))
				setupText(Control as TTTAttributedLabel, Element as CustomLinkLabel);
		}

		private void setupText(TTTAttributedLabel attributedLabel, CustomLinkLabel label)
		{
			if ((attributedLabel != null) && (label != null))
			{
				NSMutableParagraphStyle paragraphStyle = new NSMutableParagraphStyle();
				paragraphStyle.LineBreakMode = UILineBreakMode.WordWrap;

				if (label.LineSpacing > 0)
					paragraphStyle.LineHeightMultiple = (float)label.LineSpacing;

				UIElementsHelper.SetCustomLabelParagraphStyle(paragraphStyle, label);

				string text = string.Empty;
				if (!string.IsNullOrEmpty(label.Text))
					text = label.Text;

				var attString = new NSMutableAttributedString(text, new UIStringAttributes
				{
					ForegroundColor = label.TextColor.ToUIColor(),
					Font = UIFont.SystemFontOfSize((nfloat)label.FontSize),
					ParagraphStyle = paragraphStyle,
					KerningAdjustment = null
				});

				attributedLabel.LineBreakMode = UILineBreakMode.WordWrap;
				attributedLabel.Lines = 0;
				attributedLabel.TextAlignment = paragraphStyle.Alignment;

				if (!string.IsNullOrEmpty(label.UrlText) && !string.IsNullOrEmpty(label.Url) && !string.IsNullOrEmpty(label.Text) && label.Text.Contains(label.UrlText))
				{
					attributedLabel.AddLinkToURL(new NSUrl(label.Url), new NSRange(label.Text.IndexOf(label.UrlText), label.UrlText.Length));
					attributedLabel.Delegate = new LabelDelegate();
					attString.AddAttribute(UIStringAttributeKey.ForegroundColor, label.LinkColor.ToUIColor(), new NSRange(label.Text.IndexOf(label.UrlText), label.UrlText.Length));
				}

				attributedLabel.AttributedText = attString;
			}
		}
	}

	class LabelDelegate : TTTAttributedLabelDelegate
	{
		public override void DidSelectLinkWithURL(TTTAttributedLabel label, NSUrl url)
		{
			UIApplication.SharedApplication.OpenUrl(url);
		}
	}
}
