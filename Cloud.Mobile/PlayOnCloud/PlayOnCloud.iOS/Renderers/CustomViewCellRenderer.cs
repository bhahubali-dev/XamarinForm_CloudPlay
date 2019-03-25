using Foundation;
using PlayOnCloud;
using PlayOnCloud.iOS.Renderers;
using System;
using System.Drawing;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomViewCell), typeof(CustomViewCellRenderer))]
namespace PlayOnCloud.iOS.Renderers
{
	public class CustomViewCellRenderer : ViewCellRenderer
	{
		private UISwipeGestureRecognizer swipeLeft;
		private UISwipeGestureRecognizer swipeRight;

		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var cell = base.GetCell(item, reusableCell, tv);
			UIView bgViewColor = new UIView() { BackgroundColor = UIColor.FromRGBA(255, 255, 255, 15) };
			bgViewColor.Layer.MasksToBounds = true;
			cell.SelectedBackgroundView = bgViewColor;
			if ((item is CustomViewCell) && (item as CustomViewCell).CanMove)
				AddGestures(item as ViewCell, cell, tv);

			//XXX: Disable this until we get cleat idea what to do with it
			//AddSwipeGestures(item as ViewCell, cell, tv);
			return cell;
		}

		private void AddGestures(ViewCell cell, UITableViewCell tableViewCell, UITableView tableView)
		{
			tableViewCell.AddGestureRecognizer(LongPressGestureRecognizer.CreateGesture(tableView, cell));
		}

		private void AddSwipeGestures(ViewCell cell, UITableViewCell tableViewCell, UITableView tableView)
		{
			swipeRight = new UISwipeGestureRecognizer(() =>
			{
				if (!(cell as CustomViewCell).Checked)
				{
					UIView.Animate(0.2, 0.0, UIViewAnimationOptions.TransitionNone,
					() =>
					{
						tableViewCell.Frame = new RectangleF(
							new PointF((float)(tableViewCell.Frame.Location.X + 26),
							(float)tableViewCell.Frame.Location.Y),
							(SizeF)tableViewCell.Frame.Size);
					},
					() =>
					{
						tableViewCell.Frame = new RectangleF(
							new PointF((float)(tableViewCell.Frame.Location.X - 26),
							(float)tableViewCell.Frame.Location.Y),
							(SizeF)tableViewCell.Frame.Size);
						(cell as CustomViewCell).Checked = true;
					});
				}
			});

			swipeRight.Direction = UISwipeGestureRecognizerDirection.Right;
			swipeLeft = new UISwipeGestureRecognizer(() => (cell as CustomViewCell).Checked = false) { Direction = UISwipeGestureRecognizerDirection.Left, Delegate = new SwipeRecogniserDelegate(cell as CustomViewCell) };

			tableViewCell.AddGestureRecognizer(swipeRight);
			tableViewCell.AddGestureRecognizer(swipeLeft);
		}

		private class SwipeRecogniserDelegate : UIGestureRecognizerDelegate
		{
			private CustomViewCell customViewCell;
			public SwipeRecogniserDelegate(CustomViewCell customViewCell)
			{
				this.customViewCell = customViewCell;
			}

			public override bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
			{
				if ((((UISwipeGestureRecognizer)recognizer).Direction == UISwipeGestureRecognizerDirection.Left) && (customViewCell != null))
					customViewCell.Checked = false;

				return true;
			}

			public override bool ShouldBegin(UIGestureRecognizer recognizer)
			{
				var swipeGesture = ((UISwipeGestureRecognizer)recognizer);
				return true;
			}
		}

		private class LongPressGestureRecognizer : UILongPressGestureRecognizer
		{
			public static LongPressGestureRecognizer CreateGesture(UITableView tableView, ViewCell cell)
			{
				return new LongPressGestureRecognizer(OnRecognizing)
				{
					TableView = new WeakReference<UITableView>(tableView),
					ViewCell = new WeakReference<ViewCell>(cell),
				};
			}

			private static void OnRecognizing(UILongPressGestureRecognizer r)
			{
				LongPressGestureRecognizer recognizer = r as LongPressGestureRecognizer;
				UITableView tableView;
				recognizer.TableView.TryGetTarget(out tableView);
				ViewCell cell;
				recognizer.ViewCell.TryGetTarget(out cell);
				if ((tableView == null) || (cell == null))
					return;

				OnRecognizing(recognizer, tableView, cell);
			}

			private static void OnRecognizing(LongPressGestureRecognizer recognizer, UITableView tableView, ViewCell cell)
			{
				NSIndexPath indexPath = tableView.IndexPathForRowAtPoint(recognizer.LocationInView(tableView));
				switch (recognizer.State)
				{
					case UIGestureRecognizerState.Began:
						if (indexPath != null)
						{
							// Remember the source row
							recognizer.sourceIndexPath = indexPath;
							recognizer.destinationIndexPath = indexPath;
							cell.View.BackgroundColor = Color.Gray;
						}
						break;

					case UIGestureRecognizerState.Changed:
						if ((recognizer.destinationIndexPath != null) && (indexPath != null) && (recognizer.destinationIndexPath != indexPath))
						{
							// Dragged to a new row location, so show it to the user with animation
							tableView.MoveRow(recognizer.destinationIndexPath, indexPath);
							recognizer.destinationIndexPath = indexPath;
						}
						break;

					case UIGestureRecognizerState.Cancelled:
					case UIGestureRecognizerState.Failed:
						recognizer.sourceIndexPath = null;
						cell.View.BackgroundColor = Color.Transparent;
						break;

					case UIGestureRecognizerState.Recognized:
						// Move the data source finally
						if ((recognizer.sourceIndexPath != null) && (recognizer.destinationIndexPath != null) && (recognizer.sourceIndexPath != recognizer.destinationIndexPath))
						{
							// Reset the move because otherwise the underneath control will get out of sync with
							// the Xamarin.Forms element. The next line will drive the real change from ItemsSource
							tableView.MoveRow(recognizer.destinationIndexPath, recognizer.sourceIndexPath);
							tableView.Source.MoveRow(tableView, recognizer.sourceIndexPath, recognizer.destinationIndexPath);
						}

						recognizer.sourceIndexPath = null;
						recognizer.destinationIndexPath = null;
						cell.View.BackgroundColor = Color.Transparent;
						break;
				}
			}

			private WeakReference<UITableView> TableView { get; set; }

			private WeakReference<ViewCell> ViewCell { get; set; }

			private NSIndexPath sourceIndexPath, destinationIndexPath;

			private LongPressGestureRecognizer(Action<UILongPressGestureRecognizer> action)
				: base(action)
			{
			}
		}
	}
}
