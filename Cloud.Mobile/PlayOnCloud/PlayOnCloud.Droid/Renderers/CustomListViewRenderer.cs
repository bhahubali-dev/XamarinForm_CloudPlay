using System.ComponentModel;
using Android.Graphics.Drawables;
using PlayOnCloud;
using PlayOnCloud.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;

[assembly: ExportRenderer(typeof(CustomListView), typeof(CustomListViewRenderer))]

namespace PlayOnCloud.Droid.Renderers
{
    public class CustomListViewRenderer :  ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            SetBackgroundResource(Resource.Drawable.ViewCellBackground);
            base.OnElementChanged(e);

            if (Control == null)
                return;

            if (e.NewElement != null)
            {
                var listView = Control;
                listView.Divider = new ColorDrawable(Color.Gray);
                listView.DividerHeight = 1;
                listView.Selected = false;
                listView.SetSelector(Resource.Drawable.ViewCellBackground);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var listView = Control;
            listView.Selected = false;
            listView.SetSelector(Resource.Drawable.ViewCellBackground);
           

            var ss = e.PropertyName;
        }

        private void ListViewDataSourceWrapper_OnMoveRow(int sourceIndex, int destinationIndex)
        {
            if (Element != null)
                (Element as CustomListView).MoveRow(sourceIndex, destinationIndex);
        }
    }
}