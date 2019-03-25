using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using PlayOnCloud.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;
using TextAlignment = Android.Views.TextAlignment;

[assembly: ExportRenderer(typeof(PlayOnCloud.SegmentedControl), typeof(SegmentedControlRenderer))]

namespace PlayOnCloud.Droid.Renderers
{
    public class SegmentedControlRenderer : ViewRenderer<SegmentedControl, RadioGroup>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SegmentedControl> e)
        {
            base.OnElementChanged(e);

            var layoutInflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);

            var g = new RadioGroup(Context);
            g.Orientation = Orientation.Horizontal;
            g.SetMinimumHeight(50);
            g.CheckedChange += (sender, eventArgs) =>
            {
                var rg = (RadioGroup) sender;
                if (rg.CheckedRadioButtonId != -1)
                {
                    var id = rg.CheckedRadioButtonId;
                    var radioButton = rg.FindViewById(id);
                    var radioId = rg.IndexOfChild(radioButton);
                    e.NewElement.SelectedValue = radioId;
                }
            };

            for (var i = 0; i < e.NewElement.Children.Count; i++)
            {
                var o = e.NewElement.Children[i];
                var button = new SegmentedControlButton(Context);
                button.Text = o.Text;
                if (i == 0)
                    button.SetBackgroundColor(Color.Green);
                else if (i == e.NewElement.Children.Count - 1)
                    button.SetBackgroundColor(Color.Yellow);

                g.AddView(button);
            }

            SetNativeControl(g);
        }
    }

    public class SegmentedControlButton : RadioButton
    {
        private int lineHeightSelected;
        private int lineHeightUnselected;

        private Paint linePaint;

        public SegmentedControlButton(Context context) : base(context, null)
        {
            Initialize();
        }

        private void Initialize()
        {
            var lineColor = Color.LightBlue;
            linePaint = new Paint();
            linePaint.Color = lineColor;

            lineHeightUnselected = 0;
            lineHeightSelected = 2;
            SetButtonDrawable(null);
            SetAllCaps(true);
            TextAlignment = TextAlignment.Center;
            SetWidth(100);
            SetHeight(50);
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (linePaint.Color != 0 && (lineHeightSelected > 0 || lineHeightUnselected > 0))
            {
                var lineHeight = Checked ? lineHeightSelected : lineHeightUnselected;

                if (lineHeight > 0)
                {
                    var rect = new Rect(0, Height - lineHeight, Width, Height);
                    canvas.DrawRect(rect, linePaint);
                }
            }
        }
    }
}