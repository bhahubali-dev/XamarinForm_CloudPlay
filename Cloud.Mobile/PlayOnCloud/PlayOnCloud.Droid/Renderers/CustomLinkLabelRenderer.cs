using System;
using System.ComponentModel;
using Android.Text;
using Android.Text.Style;
using Android.Text.Util;
using Android.Util;
using Android.Views;
using Android.Widget;
using PlayOnCloud;
using PlayOnCloud.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomLinkLabel), typeof(CustomLinkLabelRenderer))]
namespace PlayOnCloud.Droid.Renderers
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
              // setupText(e.NewElement as CustomLinkLabel);
                //String udata = "Underlined Text";
                //SpannableString content = new SpannableString(udata);
                //content.SetSpan(new UnderlineSpan(), 0, udata.Length, 0);
              // SetNativeControl();

            }
            //var view = (CustomLinkLabel)Element;
            //if (view == null) return;

            //TextView textView = new TextView(Forms.Context);
            //textView.LayoutParameters = new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
            //textView.SetTextColor(view.TextColor.ToAndroid());

            //// Setting the auto link mask to capture all types of link-able data
            //textView.AutoLinkMask = MatchOptions.All;
            //// Make sure to set text after setting the mask
            //textView.Text = view.Text;
            //textView.SetTextSize(ComplexUnitType.Dip, (float)view.FontSize);

            //// overriding Xamarin Forms Label and replace with our native control
            //SetNativeControl(textView);
        }

       
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if ((e.PropertyName == CustomLinkLabel.TextProperty.PropertyName) ||
                (e.PropertyName == CustomLinkLabel.LinkColorProperty.PropertyName) ||
                (e.PropertyName == CustomLinkLabel.UrlProperty.PropertyName) ||
                (e.PropertyName == CustomLinkLabel.UrlTextProperty.PropertyName))
            {
                //setupText(Element as CustomLinkLabel);

            }
        }

        private void setupText(CustomLinkLabel label)
        {
            SpannableStringBuilder content = new SpannableStringBuilder(label.UrlText);
            content.SetSpan(new UnderlineSpan(),0,label.UrlText.Length,0 );

            //foreach (Span span in label.LinkColor)
            //{
            //    content.Append(span.Text);
            //    ForegroundColorSpan foregroundColorSpan = new ForegroundColorSpan(span.ForegroundColor.ToAndroid(Color.Fuchsia));
            //    content.SetSpan(foregroundColorSpan, content.Length() - span.Text.Length, content.Length(), SpanTypes.ExclusiveExclusive);
            //}

            // Control.TextSize = 39;
            Control.SetText(content, TextView.BufferType.Normal);


        }

    }
}