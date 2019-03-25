using Foundation;
using System;
using UIKit;

namespace PlayOnCloud.iOS.Controls
{
	public class ListViewDataSourceWrapper : UITableViewSource
	{
		private readonly UITableViewSource underlyingTableSource;
		public delegate void MoveRowDelegate(int sourceIndex, int destinationIndex);
		public event MoveRowDelegate OnMoveRow;

		public ListViewDataSourceWrapper(UITableViewSource underlyingTableSource)
		{
			this.underlyingTableSource = underlyingTableSource;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = GetCellInternal(tableView, indexPath);
			cell.SelectedBackgroundView = new UIView() { BackgroundColor = UIColor.FromRGBA(255, 255, 255, 15) };
			return cell;
		}

		public override nint RowsInSection(UITableView tableView, nint section)
		{
			return underlyingTableSource.RowsInSection(tableView, section);
		}

		public override nfloat GetHeightForHeader(UITableView tableView, nint section)
		{
			return underlyingTableSource.GetHeightForHeader(tableView, section);
		}

		public override UIView GetViewForHeader(UITableView tableView, nint section)
		{
			return underlyingTableSource.GetViewForHeader(tableView, section);
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			return underlyingTableSource.NumberOfSections(tableView);
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			underlyingTableSource.RowSelected(tableView, indexPath);
			tableView.ReloadRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Automatic);
		}

		public override string[] SectionIndexTitles(UITableView tableView)
		{
			return underlyingTableSource.SectionIndexTitles(tableView);
		}

		public override string TitleForHeader(UITableView tableView, nint section)
		{
			return underlyingTableSource.TitleForHeader(tableView, section);
		}

		private UITableViewCell GetCellInternal(UITableView tableView, NSIndexPath indexPath)
		{
			return underlyingTableSource.GetCell(tableView, indexPath);
		}

		public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
		{
			return true;
		}

		public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
		{
			return UITableViewCellEditingStyle.None;
		}

		public override bool ShouldIndentWhileEditing(UITableView tableView, NSIndexPath indexPath)
		{
			return false;
		}

		public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
		{
			return true;
		}

		public override void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
		{
			// Don't call the base method, which is the key
			// TODO: do things to actually reorder in data model, such as raise a reorder event, etc.
			OnMoveRow?.Invoke(sourceIndexPath.Row, destinationIndexPath.Row);
		}
	}
}
