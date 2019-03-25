using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UIKit;

namespace PlayOnCloud.iOS.Controls
{
	public enum ITSegmentOrganizeMode : int
	{
		ITSegmentOrganizeModeHorizontal = 0,
		ITSegmentOrganizeModeVertical
	};

	[Register("ITSegmentedControl"), DesignTimeVisible(true)]
	public class ITSegmentedControl : UIControl
	{
		private int indexOfSelectedSegment;
		private int numberOfSegments;
		private ITSegmentOrganizeMode organizeMode;
		private double verticalMargin;
		private double horizontalMargin;
		private UIColor separatorColor;
		private double separatorWidth;
		private UIColor selectedColor;
		private UIColor normalColor;
		private UIColor selectedTextColor;
		private UIColor normalTextColor;
		private UIFont titleFont;
		private List<ITSegment> segments;

		public ITSegmentedControl()
			: base()
		{
			setDefaultValues();
		}

		public ITSegmentedControl(NSCoder coder)
			: base(coder)
		{
			setDefaultValues();
		}

		public ITSegmentedControl(CGRect frame)
			: base(frame)
		{
			setDefaultValues();
		}

		public ITSegmentedControl(CGRect frame, UIColor separatorColor, double separatorWidth)
			: this(frame)
		{
			this.separatorColor = separatorColor;
			this.separatorWidth = separatorWidth;
		}

		private void setDefaultValues()
		{
			indexOfSelectedSegment = 0;
			numberOfSegments = 0;
			organizeMode = ITSegmentOrganizeMode.ITSegmentOrganizeModeHorizontal;
			verticalMargin = 5.0;
			horizontalMargin = 15.0;
			separatorColor = UIColor.LightGray;
			separatorWidth = 1.0;
			selectedColor = UIColor.DarkGray;
			normalColor = UIColor.White;
			selectedTextColor = UIColor.White;
			normalTextColor = UIColor.DarkGray;
			titleFont = UIFont.SystemFontOfSize((nfloat)17.0);
			segments = new List<ITSegment>();
			BackgroundColor = UIColor.Clear;
			Layer.MasksToBounds = true;
			Layer.CornerRadius = 10;
			Layer.BorderWidth = 1;
		}

		public int IndexOfSelectedSegment
		{
			get { return indexOfSelectedSegment; }
			set { indexOfSelectedSegment = value; }
		}

		public int NumberOfSegments
		{
			get { return numberOfSegments; }
			set { numberOfSegments = value; }
		}

		public ITSegmentOrganizeMode OrganizeMode
		{
			get { return organizeMode; }
			set
			{
				organizeMode = value;
				SetNeedsDisplay();
			}
		}

		public double VerticalMargin
		{
			get { return verticalMargin; }
			set
			{
				verticalMargin = value;
				foreach (ITSegment segment in segments)
					segment.VerticalMargin = verticalMargin;
			}
		}

		public double HorizontalMargin
		{
			get { return horizontalMargin; }
			set
			{
				horizontalMargin = value;
				foreach (ITSegment segment in segments)
					segment.HorizontalMargin = horizontalMargin;
			}
		}

		public UIColor SeparatorColor
		{
			get { return separatorColor; }
			set
			{
				separatorColor = value;
				SetNeedsDisplay();
			}
		}

		public double SeparatorWidth
		{
			get { return separatorWidth; }
			set
			{
				separatorWidth = value;
				foreach (ITSegment segment in segments)
					segment.SeparatorWidth = separatorWidth;

				updateFrameForSegments();
			}
		}

		public UIColor SelectedColor
		{
			get { return selectedColor; }
			set
			{
				selectedColor = value;
				foreach (ITSegment segment in segments)
					segment.SelectedColor = selectedColor;
			}
		}

		public UIColor NormalColor
		{
			get { return normalColor; }
			set
			{
				normalColor = value;
				foreach (ITSegment segment in segments)
					segment.NormalColor = normalColor;
			}
		}

		public UIColor SelectedTextColor
		{
			get { return selectedTextColor; }
			set
			{
				selectedTextColor = value;
				foreach (ITSegment segment in segments)
					segment.SelectedTextColor = selectedTextColor;
			}
		}

		public UIColor NormalTextColor
		{
			get { return normalTextColor; }
			set
			{
				normalTextColor = value;
				foreach (ITSegment segment in segments)
					segment.NormalTextColor = normalTextColor;
			}
		}

		public UIFont TitleFont
		{
			get { return titleFont; }
			set
			{
				titleFont = value;
				foreach (ITSegment segment in segments)
					segment.TitleFont = titleFont;

				SetNeedsDisplay();
			}
		}

		public override CGRect Frame
		{
			get { return base.Frame; }
			set
			{
				base.Frame = value;
				updateFrameForSegments();
			}
		}

		public void AddSegmentWithTitle(string title, UIImage selectedImage, UIImage normalImage)
		{
			ITSegment segment = new ITSegment(separatorWidth, verticalMargin, selectedColor, normalColor, selectedTextColor, normalTextColor, titleFont);
			segment.Index = segments.Count;
			segments.Add(segment);
			updateFrameForSegments();

			segment.Parent = this;
			segment.Title = title;
			segment.SelectedImage = selectedImage;
			segment.NormalImage = normalImage;

			AddSubview(segment);
			numberOfSegments = segments.Count;
		}

		private void updateFrameForSegments()
		{
			if ((segments != null) && (segments.Count != 0))
			{
				int count = segments.Count;
				if (count > 1)
				{
					if (organizeMode == ITSegmentOrganizeMode.ITSegmentOrganizeModeHorizontal)
					{
						double segmentWidth = (Frame.Size.Width - separatorWidth * (count - 1)) / count;
						double originX = 0.0;
						foreach (ITSegment segment in segments)
						{
							segment.Frame = new CGRect(originX, 0.0, segmentWidth, Frame.Size.Height);
							originX += segmentWidth + separatorWidth;
						}
					}
					else
					{
						double segmentHeight = (Frame.Size.Height - separatorWidth * (count - 1)) / count;
						double originY = 0.0;
						foreach (ITSegment segment in segments)
						{
							segment.Frame = new CGRect(0.0, originY, Frame.Size.Width, segmentHeight);
							originY += segmentHeight + separatorWidth;
						}
					}
				}
				else
				{
					ITSegment segment = segments[0];
					segment.Frame = new CGRect(0.0, 0.0, Frame.Size.Width, Frame.Size.Height);
				}
			}

			SetNeedsDisplay();
		}

		private void setIndexOfSelectedSegment(int indexOfSelectedSegment)
		{
			SelectSegmentAtIndex(indexOfSelectedSegment);
		}

		public void SelectSegment(ITSegment segment)
		{
			if (indexOfSelectedSegment != -1)
			{
				ITSegment previousSelectedSegment = segments[indexOfSelectedSegment];
				previousSelectedSegment.Selected = false;
			}

			indexOfSelectedSegment = segment.Index;
			segment.Selected = true;
			SendActionForControlEvents(UIControlEvent.ValueChanged);
		}

		public void SelectSegmentAtIndex(int index)
		{
			SelectSegment(segments[index]);
		}

		public override void DrawRect(CGRect area, UIViewPrintFormatter formatter)
		{
			CGContext context = UIGraphics.GetCurrentContext();
			drawSeparatorWithContext(context);
		}

		private void drawSeparatorWithContext(CGContext context)
		{
			context.SaveState();

			if (segments.Count > 1)
			{
				CGPath path = new CGPath();
				ITSegment segment = segments[0];

				if (organizeMode == ITSegmentOrganizeMode.ITSegmentOrganizeModeHorizontal)
				{
					double originX = (double)Frame.Size.Width + separatorWidth / 2.0;
					for (int index = 1; index < segments.Count; ++index)
					{
						segment = segments[index];

						path.MoveToPoint((nfloat)originX, (nfloat)0.0);
						path.AddLineToPoint((nfloat)originX, Frame.Size.Height);
						originX += Frame.Size.Width + separatorWidth;
					}
				}
				else
				{
					double originY = Frame.Size.Height + separatorWidth / 2.0;
					for (int index = 1; index < segments.Count; ++index)
					{
						segment = segments[index];
						path.MoveToPoint((nfloat)0.0, (nfloat)originY);
						path.AddLineToPoint(Frame.Size.Width, (nfloat)originY);
						originY += Frame.Size.Height + separatorWidth;
					}
				}

				context.AddPath(path);
				context.SetStrokeColor(separatorColor.CGColor);
				context.SetLineWidth((nfloat)separatorWidth);
				context.DrawPath(CGPathDrawingMode.Stroke);
			}

			context.RestoreState();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			InvalidateIntrinsicContentSize();
			return IntrinsicContentSize;
		}

		public override CGSize IntrinsicContentSize
		{
			get
			{
				CGSize rs = CGSize.Empty;
				foreach (ITSegment s in segments)
				{
					CGSize stf = s.SizeThatFits(rs);
					rs.Width = (nfloat)Math.Max(rs.Width, stf.Width);
					rs.Height = (nfloat)Math.Max(rs.Height, stf.Height);
				}

				rs.Width = (rs.Width + (nfloat)separatorWidth) * segments.Count - (nfloat)separatorWidth;
				return rs;
			}
		}
	}
}
