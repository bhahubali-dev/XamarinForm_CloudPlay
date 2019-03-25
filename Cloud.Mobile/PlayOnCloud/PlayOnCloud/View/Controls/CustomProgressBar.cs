using Xamarin.Forms;

namespace PlayOnCloud
{
    public class CustomProgressBar : ProgressBar
    {
        public static readonly BindableProperty ProgressTintColorProperty =
            BindableProperty.Create("ProgressTintColor", typeof(Color), typeof(CustomProgressBar),
                Color.FromHex("#3bb4ff"));

        public static readonly BindableProperty CustomScaleProperty = BindableProperty.Create("CustomScale",
            typeof(double), typeof(CustomProgressBar), 1d,
            BindingMode.TwoWay, null, onCustomScaleChanged);

        public Color ProgressTintColor
        {
            get { return (Color) GetValue(ProgressTintColorProperty); }
            set { SetValue(ProgressTintColorProperty, value); }
        }

        public double CustomScale
        {
            get { return (double) GetValue(CustomScaleProperty); }
            set { SetValue(CustomScaleProperty, value); }
        }

        private static void onCustomScaleChanged(BindableObject sender, object oldValue, object newValue)
        {
            var progressBar = sender as CustomProgressBar;
            if (progressBar != null && oldValue != newValue)
                if (newValue is double)
                    progressBar.ScaleTo((double) newValue);
        }
    }
}