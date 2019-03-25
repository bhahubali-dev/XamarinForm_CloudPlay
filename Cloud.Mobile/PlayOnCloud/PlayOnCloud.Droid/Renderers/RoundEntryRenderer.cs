using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using PlayOnCloud;
using PlayOnCloud.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(RoundEntry), typeof(RoundEntryRenderer))]

namespace PlayOnCloud.Droid.Renderers
{
    internal class RoundEntryRenderer : GeneralEntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null && e.NewElement != null)
            {
                var roundEntry = (RoundEntry) e.NewElement;

                SetBorder(roundEntry);
                SetFont(roundEntry);
                SetTextAlignment(roundEntry);
                SetPlaceholderTextColor(roundEntry);
            }
        }

        private void SetTextAlignment(RoundEntry view)
        {
            switch (view.XAlign)
            {
                case Xamarin.Forms.TextAlignment.Center:
                    Control.Gravity = GravityFlags.Center;
                    break;
                case Xamarin.Forms.TextAlignment.End:
                    Control.Gravity = GravityFlags.End;
                    break;
                case Xamarin.Forms.TextAlignment.Start:
                    Control.Gravity = GravityFlags.Start;
                    break;
            }
        }

        private void SetFont(RoundEntry view)
        {
            Control.SetTextSize(ComplexUnitType.Sp, (float) view.FontSize);
        }

        private void SetBorder(RoundEntry view)
        {
            if (view.BorderRadius > 0)
            {
                var backColor = view.BackgroundColor.ToAndroid();
                view.BackgroundColor = Color.Transparent;

                var gd = new GradientDrawable();
                gd.SetColor(backColor);
                gd.SetCornerRadius(view.BorderRadius);
                gd.SetStroke(1, view.BorderColor.ToAndroid());

                Control.SetBackgroundDrawable(gd);
            }
        }

        private void SetPlaceholderTextColor(RoundEntry view)
        {
            if (string.IsNullOrEmpty(view.Placeholder) == false && view.PlaceholderTextColor != Color.Default)
            {
                Control.Hint = view.Placeholder;
                Control.SetHintTextColor(view.PlaceholderTextColor.ToAndroid());
            }
        }
    }
}