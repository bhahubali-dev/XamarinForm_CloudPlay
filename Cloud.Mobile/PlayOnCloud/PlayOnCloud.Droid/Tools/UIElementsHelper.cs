using Xamarin.Forms;

namespace PlayOnCloud.Droid.Tools
{
    public class UIElementsHelper
    {
        public static void HighlightSubstring(string boldText, Label label)
        {
            //if (string.IsNullOrWhiteSpace(boldText) || label == null || string.IsNullOrWhiteSpace(label.Text))
            //    return;

            //if (!label.Text.Contains(boldText) ||
            //    !label.s.RespondsToSelector(new ObjCRuntime.Selector("setAttributedText:")))
            //    return;

            //var index = label.Text.IndexOf(boldText, StringComparison.OrdinalIgnoreCase);
            //if (index >= 0)
            //{
            //    AttributedString controlText = GetUILabelAttributedText(label);
            //    if (controlText != null)
            //    {
            //        var attributedText = new NSMutableAttributedString(controlText);
            //        var range = new NSRange(index, boldText.Length);
            //        attributedText.SetAttributes(
            //            new UIStringAttributes {Font = UIFont.BoldSystemFontOfSize(label.Font.PointSize)}, range);
            //        label.AttributedText = attributedText;
            //    }
            //}
        }

        //public static void HighlightText(Label textView, string searchText, Color foregroundColor,
        //    Color? backgroundColor = null)
        //{
        //    var highlightMarker = new HighlightMarker.HighlightMarker(textView.Text, searchText);
        //    var spannableStringBuilder = new SpannableStringBuilder(textView.Text);

        //    foreach (var current in highlightMarker)
        //    {
        //        var fromIndex = current.FromIndex;
        //        var endIndex = fromIndex + current.Length;
        //        var isHighlighted = current.IsHighlighted;

        //        if (isHighlighted)
        //        {
        //            spannableStringBuilder.SetSpan(new ForegroundColorSpan(foregroundColor), fromIndex, endIndex,
        //                SpanTypes.ExclusiveExclusive);

        //            if (backgroundColor.HasValue)
        //                spannableStringBuilder.SetSpan(new BackgroundColorSpan(backgroundColor.Value), fromIndex,
        //                    endIndex, SpanTypes.ExclusiveExclusive);
        //        }
        //    }

        //    textView.FormattedText = spannableStringBuilder.ToString();
        //}

        //{//public static void SetCustomLabelParagraphStyle(NSMutableParagraphStyle paragraphStyle, CustomLabelBase label)

        //    if ((label != null) && (paragraphStyle != null))
        //    {
        //        if (label.ParagraphStyleAlignment == CustomTextAlignment.Center)
        //            paragraphStyle.Alignment = Android.Views.TextAlignment.Center;
        //        else if (label.ParagraphStyleAlignment == CustomTextAlignment.Justified)
        //            paragraphStyle.Alignment = Android.Views.TextAlignment.Justified;
        //        else if (label.ParagraphStyleAlignment == CustomTextAlignment.Left)
        //            paragraphStyle.Alignment = Android.Views.TextAlignment;
        //        else if (label.ParagraphStyleAlignment == CustomTextAlignment.Right)
        //            paragraphStyle.Alignment = Android.Views.TextAlignment.Right;
        //        else
        //            paragraphStyle.Alignment = UITextAlignment.Natural;
        //    }
        //}

        //public static FontAttributesd GetUILabelAttributedText(Label label)
        //{
        //    if (label != null)
        //    {
        //        if (label.FontAttributes != null)
        //            return label.FontAttributes;
        //        else if (label.Text != null)
        //            return new NSAttributedString(label.Text);
        //    }

        //    return null;
        //}
    }
}