using System.ComponentModel;
using Android.Views.Animations;
using Android.Widget;
using PlayOnCloud;
using PlayOnCloud.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AProgressBar = Android.Widget.ProgressBar;

[assembly: ExportRenderer(typeof(CustomActivityIndicator), typeof(CustomActivityIndicatorRenderer))]

namespace PlayOnCloud.Droid.Renderers
{
    internal class CustomActivityIndicatorRenderer : ViewRenderer<CustomActivityIndicator, ImageView>
        //: ViewRenderer<ActivityIndicator, AProgressBar>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<CustomActivityIndicator> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                var imageView = new ImageView(Context);
                imageView.SetImageResource(Resource.Drawable.loadingAnimation);
                imageView.Focusable = true;
                SetNativeControl(imageView);

                if (e.NewElement.IsRunning)
                    startAnimation();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName)
                if (Element.IsRunning)
                    startAnimation();
                else
                    stopAnimation();
        }

        private void startAnimation()
        {

            var rotate = new RotateAnimation(0, 360, Dimension.RelativeToSelf, 0.5f, Dimension.RelativeToSelf, 0.5f)
            {
                Duration = 1000,
                RepeatMode = RepeatMode.Restart,
                RepeatCount = int.MaxValue
            };

            Control.StartAnimation(rotate);
            
        }

        private void stopAnimation()
        {
            Control.Animation = null;
        }


        //public CustomActivityIndicatorRenderer()
        //{
        //    AutoPackage = false;

        //}

        //protected override AProgressBar CreateNativeControl()
        //{
        //    return new AProgressBar(Context) {Indeterminate = true};
        //}

        //protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
        //{
        //    base.OnElementChanged(e);

        //    var progressBar = Control;
        //    if (progressBar == null)
        //    {
        //        progressBar = CreateNativeControl();
        //        SetNativeControl(progressBar);
        //    }

        //    UpdateColor();
        //    UpdateVisibility();
        //}

        //protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    base.OnElementPropertyChanged(sender, e);

        //    if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName)
        //        UpdateVisibility();
        //    else if (e.PropertyName == ActivityIndicator.ColorProperty.PropertyName)
        //        UpdateColor();
        //}

        //private void UpdateColor()
        //{
        //    var color = Element.Color;

        //    //if (!color..IsDefault)
        //    //    Control.IndeterminateDrawable.SetColorFilter(color.ToAndroid(), PorterDuff.Mode.SrcIn);
        //    //else
        //    Control.IndeterminateDrawable.ClearColorFilter();
        //}

        //private void UpdateVisibility()
        //{
        //    Control.Visibility = Element.IsRunning ? ViewStates.Visible : ViewStates.Invisible;
        //}
    }
}