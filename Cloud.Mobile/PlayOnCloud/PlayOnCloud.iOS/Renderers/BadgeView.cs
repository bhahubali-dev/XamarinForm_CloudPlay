using System;
using System.Drawing;
using CoreGraphics;
using Foundation;
using UIKit;

namespace PlayOnCloud.iOS
{
	/// <summary>
	/// A Badge view.
	/// </summary>
	public class BadgeView : UIView
	{
		private int val;

		public BadgeView(CGRect frame)
			: base(frame)
		{
			InitState();
		}

		public BadgeView(NSCoder coder)
			: base(coder)
		{
			InitState();
		}

		public string TextFormat { get; set; }

		public PointF AdjustOffset { get; set; }

		public int Value
		{
			get { return val; }
			set
			{
				if (val != value)
				{
					val = value;
					Hidden = (val == 0);

					var size = BadgeSize;
					Frame = new CGRect(Frame.X, Frame.Y, size.Width, size.Height);
					SetNeedsDisplay();
				}
			}
		}

		public bool Shadow { get; set; }

		public SizeF ShadowOffset { get; set; }

		public UIColor ShadowColor { get; set; }

		public UIFont Font { get; set; }

		public UIColor FillColor { get; set; }

		public UIColor SubFillColor { get; set; }

		public UIColor StrokeColor { get; set; }

		public float StrokeWidth { get; set; }

		public UIColor TextColor { get; set; }

		public UITextAlignment Alignment { get; set; }

		public CGSize BadgeSize
		{
			get
			{
				var numberString = Value.ToString(TextFormat);
				CGSize numberSize = new NSString(numberString).StringSize(Font);

				using (CGPath badgePath = NewBadgePathForTextSize(numberSize))
				{
					CGRect badgeRect = badgePath.PathBoundingBox;

					badgeRect.X = 0;
					badgeRect.Y = 0;
					badgeRect.Size = new SizeF((float)Math.Ceiling(badgeRect.Size.Width), (float)Math.Ceiling(badgeRect.Size.Height));

					return badgeRect.Size;
				}
			}
		}

		public int Pad { get; set; }

		public override void Draw(CGRect rect)
		{
			CGRect viewBounds = Bounds;
			CGContext curContext = UIGraphics.GetCurrentContext();
			var numberString = Value.ToString(TextFormat);

			CGSize numberSize = new NSString(numberString).StringSize(Font);
			using (CGPath badgePath = NewBadgePathForTextSize(numberSize))
			{
				CGRect badgeRect = badgePath.PathBoundingBox;

				badgeRect.X = 0;
				badgeRect.Y = 0;
				badgeRect.Size = new SizeF((float)Math.Ceiling(badgeRect.Size.Width), (float)Math.Ceiling(badgeRect.Size.Height));

				curContext.SaveState();

				curContext.SetLineWidth(StrokeWidth);
				curContext.SetStrokeColor(StrokeColor.CGColor);
				curContext.SetFillColor(SubFillColor.CGColor);

				curContext.AddEllipseInRect(badgeRect);
				curContext.DrawPath(CGPathDrawingMode.Fill);

				curContext.SetFillColor(FillColor.CGColor);

				// Line stroke straddles the path, so we need to account for the outer portion
				badgeRect.Size = new CGSize(badgeRect.Size.Width + (float)Math.Ceiling(StrokeWidth / 2), badgeRect.Size.Height + (float)Math.Ceiling(StrokeWidth / 2));

				CGPoint ctm = new PointF(0f, 0f);

				curContext.TranslateCTM(ctm.X, ctm.Y);

				curContext.BeginPath();
				curContext.AddPath(badgePath);
				curContext.ClosePath();
				curContext.DrawPath(CGPathDrawingMode.EOFill);
				curContext.RestoreState();

				curContext.SaveState();
				curContext.SetFillColor(TextColor.CGColor);

				CGPoint textPt = new CGPoint(ctm.X + ((badgeRect.Size.Width - numberSize.Width) / 2) + AdjustOffset.X,
					ctm.Y + ((badgeRect.Size.Height - numberSize.Height) / 2) + AdjustOffset.Y);

				curContext.RestoreState();
			}
		}

		private void InitState()
		{
			var iosVersion = new Version(UIDevice.CurrentDevice.SystemVersion);

			Opaque = false;
			Pad = 0;
			Font = UIFont.BoldSystemFontOfSize(16);
			Shadow = true;
			ShadowOffset = new SizeF(0, 3);
			ShadowColor = UIColor.Black.ColorWithAlpha(0.5f);
			Alignment = UITextAlignment.Center;
			FillColor = UIColor.Red;
			StrokeColor = iosVersion.Major < 7 ? UIColor.White : UIColor.Clear;
			StrokeWidth = iosVersion.Major < 7 ? 2.0f : 0.0f;
			TextColor = UIColor.White;
			AdjustOffset = new PointF(0, 0);
			TextFormat = "d";

			BackgroundColor = UIColor.Clear;
		}

		private CGPath NewBadgePathForTextSize(CGSize size)
		{
			nfloat arcRadius = (nfloat)Math.Ceiling((size.Height + Pad) / 2.0f);

			nfloat badgeWidthAdjustment = size.Width - (size.Height / 2.0f);
			nfloat badgeWidth = 2.0f * arcRadius;
			var m_pi_2 = (float)(Math.PI / 2);

			if (badgeWidthAdjustment > 0.0)
				badgeWidth += badgeWidthAdjustment;

			CGPath badgePath = new CGPath();

			//Add main circle
			badgePath.MoveToPoint(arcRadius, 0.0f);
			badgePath.AddArc(arcRadius, arcRadius, arcRadius, 3.0f * m_pi_2, m_pi_2, true);
			badgePath.AddLineToPoint(badgeWidth - arcRadius, 2.0f * arcRadius);
			badgePath.AddArc(badgeWidth - arcRadius, arcRadius, arcRadius, m_pi_2, 3.0f * m_pi_2, true);
			badgePath.AddLineToPoint(arcRadius, 0.0f);

			//Add sub circle
			badgePath.MoveToPoint(arcRadius, 0.0f);
			badgePath.AddArc(arcRadius, arcRadius, arcRadius / 2, m_pi_2, 3.0f * m_pi_2, false);
			badgePath.AddLineToPoint(badgeWidth - arcRadius, 2.0f * arcRadius);
			badgePath.AddArc(badgeWidth - arcRadius, arcRadius, arcRadius / 2, 3.0f * m_pi_2, m_pi_2, false);
			badgePath.AddLineToPoint(arcRadius, 0.0f);

			return badgePath;
		}
	}
}
