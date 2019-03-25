using Android.App;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Point = Android.Graphics.Point;

namespace PlayOnCloud.Droid.Popups
{
    internal static class DisplayHelper
    {
        public static Rectangle GetSize()
        {
            var display = (Forms.Context as Activity).WindowManager.DefaultDisplay;
            var size = new Point();
            display.GetSize(size);

            var width = (int) Forms.Context.FromPixels(size.X);
            var height = (int) Forms.Context.FromPixels(size.Y);
            return new Rectangle(0, 0, width, height);
        }
    }
}