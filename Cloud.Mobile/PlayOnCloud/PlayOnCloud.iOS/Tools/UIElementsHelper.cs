using Foundation;
using System;
using UIKit;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public static class UIElementsHelper
	{
		public static void HighlightSubstring(string boldText, UILabel label)
		{
			if (string.IsNullOrWhiteSpace(boldText) || (label == null) || string.IsNullOrWhiteSpace(label.Text))
				return;

			if (!label.Text.Contains(boldText) || !label.RespondsToSelector(new ObjCRuntime.Selector("setAttributedText:")))
				return;

			var index = label.Text.IndexOf(boldText, StringComparison.OrdinalIgnoreCase);
			if (index >= 0)
			{
				NSAttributedString controlText = GetUILabelAttributedText(label);
				if (controlText != null)
				{
					var attributedText = new NSMutableAttributedString(controlText);
					var range = new NSRange(index, boldText.Length);
					attributedText.SetAttributes(new UIStringAttributes { Font = UIFont.BoldSystemFontOfSize(label.Font.PointSize) }, range);
					label.AttributedText = attributedText;
				}
			}
		}

		public static void SetCustomLabelParagraphStyle(NSMutableParagraphStyle paragraphStyle, CustomLabelBase label)
		{
			if ((label != null) && (paragraphStyle != null))
			{
				if (label.ParagraphStyleAlignment == CustomTextAlignment.Center)
					paragraphStyle.Alignment = UITextAlignment.Center;
				else if (label.ParagraphStyleAlignment == CustomTextAlignment.Justified)
					paragraphStyle.Alignment = UITextAlignment.Justified;
				else if (label.ParagraphStyleAlignment == CustomTextAlignment.Left)
					paragraphStyle.Alignment = UITextAlignment.Left;
				else if (label.ParagraphStyleAlignment == CustomTextAlignment.Right)
					paragraphStyle.Alignment = UITextAlignment.Right;
				else
					paragraphStyle.Alignment = UITextAlignment.Natural;
			}
		}

		public static NSAttributedString GetUILabelAttributedText(UILabel label)
		{
			if (label != null)
			{
				if (label.AttributedText != null)
					return label.AttributedText;
				else if (label.Text != null)
					return new NSAttributedString(label.Text);
			}

			return null;
		}
	}
}
