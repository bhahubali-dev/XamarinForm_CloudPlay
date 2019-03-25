using PlayOnCloud;
using PlayOnCloud.iOS.Controls;
using PlayOnCloud.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using PlayOnCloud.iOS.Extensions;

[assembly: ExportRenderer(typeof(CustomListView), typeof(CustomListViewRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class CustomListViewRenderer : ListViewRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
				return;

			if (e.NewElement != null)
			{
				var tableView = Control as UITableView;
				if ((e.NewElement as CustomListView).CanEditRow)
				{
					ListViewDataSourceWrapper listViewDataSourceWrapper = new ListViewDataSourceWrapper(this.GetFieldValue<UITableViewSource>(typeof(ListViewRenderer), "_dataSource"));
					listViewDataSourceWrapper.OnMoveRow += ListViewDataSourceWrapper_OnMoveRow;
					tableView.Source = listViewDataSourceWrapper;
				}

				tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			}
		}

		private void ListViewDataSourceWrapper_OnMoveRow(int sourceIndex, int destinationIndex)
		{
			if (Element != null)
				(Element as CustomListView).MoveRow(sourceIndex, destinationIndex);
		}
	}
}
