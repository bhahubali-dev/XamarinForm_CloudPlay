using System.ComponentModel;
using PlayOnCloud;
using PlayOnCloud.Droid.Renderers;
using PlayOnCloud.Droid.Tools;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomLabel), typeof(CustomLabelRenderer))]

namespace PlayOnCloud.Droid.Renderers
{
    public class CustomLabelRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            var label = Element as CustomLabel;
            if (Control != null && label != null)
                if (label.LinesCount > -1)
                {
                    Control.SetMaxLines(label.LinesCount);
                    Control.SetLines(label.LinesCount);
                    label.LineBreakMode = LineBreakMode.TailTruncation;
                }

            highlightSubstring();
            setLineSpacing();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Label.TextProperty.PropertyName ||
                e.PropertyName == CustomLabel.BoldTextProperty.PropertyName)
            {
                highlightSubstring();
                setLineSpacing();
            }
        }

        private void setLineSpacing()
        {
            var label = Element as CustomLabel;
            if (Control != null && label != null && label.LineSpacing > 0)
            {
                //NSAttributedString controlText = UIElementsHelper.GetUILabelAttributedText(Control);
                //if (controlText != null)
                //{
                //    var labelString = new NSMutableAttributedString(controlText);
                //    var paragraphStyle = new NSMutableParagraphStyle {LineHeightMultiple = (nfloat) label.LineSpacing};

                //    // UIElementsHelper.SetCustomLabelParagraphStyle(paragraphStyle, label);

                //    var style = UIStringAttributeKey.ParagraphStyle;
                //    var range = new NSRange(0, labelString.Length);

                //    labelString.AddAttribute(style, paragraphStyle, range);
                //    Control.AttributedText = labelString;
                //}
            }
        }

        private void highlightSubstring()
        {
            var label = Element as CustomLabel;
            UIElementsHelper.HighlightSubstring((Element as CustomLabel)?.BoldText, Element as CustomLabel);
            // UIElementsHelper.HighlightText(label, label.BoldText, Color.Green, Color.Brown);
        }
    }
}