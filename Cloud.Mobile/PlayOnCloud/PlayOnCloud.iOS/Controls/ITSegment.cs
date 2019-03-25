using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using Foundation;
using System.ComponentModel;

namespace PlayOnCloud.iOS.Controls
{
	[Register("ITSegment"), DesignTimeVisible(true)]
	public class ITSegment : UIControl
	{
		private int index;
		private double verticalMargin;
		private double horizontalMargin;
		private double separatorWidth;
		private UIColor selectedColor;
		private UIColor normalColor;
		private string title;
		private UIColor selectedTextColor;
		private UIColor normalTextColor;
		private UIFont titleFont;
		private UIImage selectedImage;
		private UIImage normalImage;
		private UIColor highlightColor;
		private UIImageView imageView;
		private UILabel label;
		private double labelWidth;
		private double imageHMargin;
		private ITSegmentedControl parent;

		public ITSegment(double separatorWidth, double verticalMargin, UIColor selectedColor, UIColor normalColor, UIColor selectedTextColor, UIColor normalTextColor, UIFont titleFont) : base(CGRect.Empty)
		{
			parent = null;
			index = 0;
			verticalMargin = 5.0;
			horizontalMargin = 15.0;
			imageHMargin = 5.0;
			selectedColor = UIColor.Blue;
			normalColor = UIColor.DarkGray;
			titleFont = UIFont.SystemFontOfSize((nfloat)17.0);
			imageView = new UIImageView();
			imageView.Init();
			label = new UILabel();
			label.Init();
			labelWidth = 0.0;

			this.separatorWidth = separatorWidth;
			this.verticalMargin = verticalMargin;
			this.selectedColor = selectedColor;
			this.normalColor = normalColor;
			this.selectedTextColor = selectedTextColor;
			this.normalTextColor = normalTextColor;
			this.titleFont = titleFont;

			label.BackgroundColor = UIColor.Clear;
			setupUIElements();
		}

		public override bool Selected
		{
			get { return base.Selected; }
			set
			{
				base.Selected = value;
				if (base.Selected)
				{
					BackgroundColor = selectedColor;
					label.TextColor = selectedTextColor;
					imageView.Image = selectedImage;
				}
				else
				{
					BackgroundColor = normalColor;
					label.TextColor = normalTextColor;
					imageView.Image = normalImage;
				}
			}
		}

		public override CGRect Frame
		{
			get { return base.Frame; }
			set
			{
				base.Frame = value;
				resetContentFrame();
			}
		}

		public int Index
		{
			get { return index; }
			set { index = value; }
		}

		public double VerticalMargin
		{
			get { return verticalMargin; }
			set
			{
				verticalMargin = value;
				resetContentFrame();
			}
		}

		public double HorizontalMargin
		{
			get { return horizontalMargin; }
			set { horizontalMargin = value; }
		}

		public double SeparatorWidth
		{
			get { return separatorWidth; }
			set { separatorWidth = value; }
		}

		public UIColor SelectedColor
		{
			get { return selectedColor; }
			set
			{
				selectedColor = value;
				if (Selected)
					BackgroundColor = selectedColor;

				nfloat hue = (nfloat)0.0;
				nfloat sat = (nfloat)0.0;
				nfloat brt = (nfloat)0.0;
				nfloat alp = (nfloat)0.0;
				selectedColor.GetHSBA(out hue, out sat, out brt, out alp);
				highlightColor = UIColor.FromHSBA(hue, sat * (nfloat)1.25, (nfloat)Math.Min((double)(brt * 0.75), (double)1.0), alp);
			}
		}

		public UIColor NormalColor
		{
			get { return normalColor; }
			set
			{
				normalColor = value;
				if (!Selected)
					BackgroundColor = normalColor;
			}
		}

		public string Title
		{
			get { return title; }
			set
			{
				label.Text = title = value;
				adjustLabel();
			}
		}

		public UIColor SelectedTextColor
		{
			get { return selectedTextColor; }
			set
			{
				selectedTextColor = value;
				if (Selected)
					Label.TextColor = selectedTextColor;
			}
		}

		public UIColor NormalTextColor
		{
			get { return normalTextColor; }
			set
			{
				normalTextColor = value;
				if (!Selected)
					Label.TextColor = normalTextColor;
			}
		}

		public UIFont TitleFont
		{
			get { return titleFont; }
			set
			{
				label.Font = titleFont = value;
				adjustLabel();
			}
		}

		public UIImage SelectedImage
		{
			get { return selectedImage; }
			set
			{
				selectedImage = value;
				if (selectedImage != null)
					resetContentFrame();

				if (Selected)
					imageView.Image = selectedImage;
			}
		}

		public UIImage NormalImage
		{
			get { return normalImage; }
			set
			{
				normalImage = value;
				if (normalImage != null)
					resetContentFrame();

				if (!Selected)
					imageView.Image = normalImage;
			}
		}

		public UIColor HighlightColor
		{
			get { return highlightColor; }
			set { highlightColor = value; }
		}

		public UIImageView ImageView
		{
			get { return imageView; }
			set { imageView = value; }
		}

		public UILabel Label
		{
			get { return label; }
			set { label = value; }
		}

		public double LabelWidth
		{
			get { return labelWidth; }
			set { labelWidth = value; }
		}

		public double ImageHMargin
		{
			get { return imageHMargin; }
			set { imageHMargin = value; }
		}

		public ITSegmentedControl Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		private void adjustLabel()
		{
			if (!string.IsNullOrEmpty(label.Text))
				labelWidth = label.SizeThatFits(new CoreGraphics.CGSize(double.MaxValue, this.Frame.Size.Height)).Width;
			else
				labelWidth = 0.0;
		}

		private void resetContentFrame()
		{
			if (imageView == null)
				return;

			double distanceBetween = 0.0;
			CGRect imageViewFrame = new CGRect(0.0, verticalMargin, 0.0, Frame.Size.Height - verticalMargin * 2.0);

			if ((selectedImage != null) || (normalImage != null))
			{
				imageViewFrame.Width = Frame.Size.Height - (nfloat)(verticalMargin * 2.0);
				distanceBetween = imageHMargin;
			}

			imageViewFrame.X = (nfloat)Math.Max((Frame.Size.Width - imageViewFrame.Width - labelWidth) / 2.0 - distanceBetween, 0.0);

			imageView.Frame = imageViewFrame;
			label.Frame = new CGRect(imageViewFrame.X + imageViewFrame.Width + distanceBetween, verticalMargin, labelWidth, Frame.Size.Height - verticalMargin * 2.0);
		}

		private void setupUIElements()
		{
			BackgroundColor = normalColor;

			imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			AddSubview(imageView);

			label.TextAlignment = UITextAlignment.Center;
			label.Font = titleFont;
			label.TextColor = normalTextColor;
			AddSubview(label);
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);
			if (Selected)
				BackgroundColor = highlightColor;

			parent.SelectSegment(this);
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			double w = labelWidth;
			if ((selectedImage != null) || (normalImage != null))
				w += imageHMargin * 2.0;

			w += horizontalMargin * 2.0;
			return new CGSize(w, Frame.Size.Height);
		}
	}
}