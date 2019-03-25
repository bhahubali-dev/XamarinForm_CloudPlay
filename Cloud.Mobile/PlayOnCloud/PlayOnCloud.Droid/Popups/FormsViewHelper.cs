using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using View = Xamarin.Forms.View;

namespace PlayOnCloud.Droid.Popups
{
    //internal static class FormsViewHelper
    //{
    //   public static ViewGroup ConvertFormsToNative(Xamarin.Forms.View view, Rectangle size)
    //    {
    //        var vRenderer = RendererFactory.GetRenderer(view);
    //        var viewGroup = vRenderer.ViewGroup;
    //        vRenderer.Tracker.UpdateLayout();
    //        var layoutParams = new ViewGroup.LayoutParams((int)size.Width, (int)size.Height);
    //        viewGroup.LayoutParameters = layoutParams;
    //        view.Layout(size);
    //        viewGroup.Layout(0, 0, (int)view.WidthRequest, (int)view.HeightRequest);
    //        return viewGroup;
    //    }
    //}

    internal static class FormsViewHelper
    {
        // Code taken from 
        // http://www.michaelridland.com/xamarin/creating-native-view-xamarin-forms-viewpage/
        public static ViewGroup ConvertFormsToNative(View view, Rectangle size)
        {
            var renderer = Platform.CreateRenderer(view);
            var viewGroup = renderer.ViewGroup;
            renderer.Tracker.UpdateLayout();
            var layoutParams = new ViewGroup.LayoutParams((int) size.Width, (int) size.Height);
            viewGroup.LayoutParameters = layoutParams;
            view.Layout(size);
            viewGroup.Layout(0, 0, (int) view.WidthRequest, (int) view.HeightRequest);
            return viewGroup;
        }
    }
}