using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using PlayOnCloud;
using PlayOnCloud.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using DroidListView = Android.Widget.ListView;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(CustomViewCell), typeof(CustomViewCellRenderer))]

namespace PlayOnCloud.Droid.Renderers
{
    public class CustomViewCellRenderer : ViewCellRenderer
    {
        private View cell;
        private bool selected;
        private Drawable unselectedBackground;

        protected override View GetCellCore(Cell item, View convertView, ViewGroup parent, Context context)
        {
            cell = base.GetCellCore(item, convertView, parent, context);

            selected = false;

            cell.SetBackgroundResource(Resource.Drawable.ViewCellBackground);
            var listView = parent as DroidListView;

            if (listView != null)
                listView.SetSelector(Resource.Drawable.ViewCellBackground);
            //if ((item is CustomViewCell) && (item as CustomViewCell).CanMove)
            //    AddGestures(item as ViewCell, cell, parent);


            return cell;
        }

        private void AddGestures(ViewCell cell, View convertView, ViewGroup parent)
        {
            var listView = parent as DroidListView;

            if (listView != null)
                listView.SetSelector(Resource.Drawable.ViewCellBackground);
        }

        protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnCellPropertyChanged(sender, e);
            if (Cell == null)
                return;


            if (e.PropertyName == "IsSelected")
            {
                // I had to create a property to track the selection because cellCore.Selected is always false.
                // Toggle selection
                selected = !selected;
                var listView = sender as DroidListView;

                if (listView != null)
                    listView.SetSelector(Resource.Drawable.ViewCellBackground);
                if (selected)
                {
                   
                    cell.Selected = false;
                    var customTextCell = sender as CustomViewCell;
                }
                else
                {
                    cell.Selected = false;
                }
            }
        }
    }
}