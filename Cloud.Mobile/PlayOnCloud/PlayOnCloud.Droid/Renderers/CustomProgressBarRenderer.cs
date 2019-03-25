using System.ComponentModel;
using Android.Content.Res;
using Android.Graphics;
using PlayOnCloud;
using PlayOnCloud.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;

[assembly: ExportRenderer(typeof(CustomProgressBar), typeof(CustomProgressBarRenderer))]

namespace PlayOnCloud.Droid.Renderers
{
    public class CustomProgressBarRenderer : ProgressBarRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
        {
            base.OnElementChanged(e);

            var customProgressBar = Element as CustomProgressBar;
            if (customProgressBar != null)
            {
                var scaleCoef = (float) customProgressBar.CustomScale;
            }
            Control.ProgressTintList = ColorStateList.ValueOf(Color.Rgb(182, 231, 233));
            // //Change the color
            Control.ScaleY = 1; //Changes the height

            if (e.NewElement == null)
                return;

            if (Control != null)
                UpdateBarColor();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == CustomProgressBar.ProgressTintColorProperty.PropertyName)
                UpdateBarColor();
        }

        private void UpdateBarColor()
        {
            var element = Element as CustomProgressBar;
            Control.IndeterminateDrawable.SetColorFilter(element.ProgressTintColor.ToAndroid(), PorterDuff.Mode.SrcIn);
            // Control.ProgressDrawable.SetColorFilter(element.ProgressTintColor.ToAndroid(), PorterDuff.Mode.SrcIn);
        }
    }
}