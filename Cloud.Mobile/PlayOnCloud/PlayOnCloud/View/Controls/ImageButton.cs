using System;
using System.Linq.Expressions;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public enum ImageOrientation : long
	{
		ImageToLeft = 0,
		ImageOnTop = 1,
		ImageToRight = 2,
		ImageOnBottom = 3,
		ImageCentered = 4
	}

	public class ImageButton : Button
	{
		private const int MinWidth = 50;
		private const int MinHeight = 50;
		private const double DefaultUncheckedAlpha = 0.3;

		public int RenderWidth => ImageWidthRequest <= 0 ? MinWidth : ImageWidthRequest;
		public int RenderHeight => ImageHeightRequest <= 0 ? MinHeight : ImageHeightRequest;

		public static readonly BindableProperty SourceProperty = BindableProperty.Create(
			"Source", typeof(ImageSource), typeof(ImageButton),
			null,
			BindingMode.OneWay,
			null,
			(bindable, oldvalue, newvalue) => ((VisualElement)bindable).ToString());

		public static readonly BindableProperty DisabledSourceProperty = BindableProperty.Create(
			"DisabledSource", typeof(ImageSource), typeof(ImageButton),
			null,
			BindingMode.OneWay,
			null,
			(bindable, oldvalue, newvalue) => ((VisualElement)bindable).ToString());

		public static readonly BindableProperty ImageWidthRequestProperty =
			BindableProperty.Create(nameof(ImageWidthRequest), typeof(int), typeof(ImageButton), default(int));

		public static readonly BindableProperty BadgeValueProperty =
			BindableProperty.Create(nameof(BadgeValue), typeof(int), typeof(ImageButton), default(int));

		public static readonly BindableProperty BadgeColorProperty =
			BindableProperty.Create(nameof(BadgeColor), typeof(Color), typeof(ImageButton), default(Color));

		public static readonly BindableProperty BadgeSubColorProperty =
			BindableProperty.Create(nameof(BadgeSubColor), typeof(Color), typeof(ImageButton), default(Color));

		public static readonly BindableProperty ImageHeightRequestProperty =
			BindableProperty.Create(nameof(ImageHeightRequest), typeof(int), typeof(ImageButton), default(int));

		public static readonly BindableProperty OrientationProperty =
			BindableProperty.Create(nameof(Orientation), typeof(ImageOrientation), typeof(ImageButton), ImageOrientation.ImageToLeft);

		public static readonly BindableProperty CheckedProperty =
			BindableProperty.Create(nameof(Checked), typeof(bool), typeof(ImageButton), false);

		public static readonly BindableProperty SupportCheckedStateProperty =
			BindableProperty.Create(nameof(SupportCheckedState), typeof(bool), typeof(ImageButton), false);

		public static readonly BindableProperty UncheckedAlphaProperty =
			BindableProperty.Create(nameof(UncheckedAlpha), typeof(double), typeof(ImageButton), DefaultUncheckedAlpha);

		public static readonly BindableProperty ShadowOffsetProperty =
			BindableProperty.Create(nameof(ShadowOffset), typeof(double), typeof(ImageButton), -1d);

		public static readonly BindableProperty ShadowOpacityProperty =
			BindableProperty.Create(nameof(ShadowOpacity), typeof(float), typeof(ImageButton), -1f);

		public static readonly BindableProperty ShadowColorProperty =
			BindableProperty.Create(nameof(ShadowColor), typeof(Color), typeof(ImageButton), default(Color));

		public static readonly BindableProperty ShadowRadiusProperty =
			BindableProperty.Create(nameof(ShadowRadius), typeof(float), typeof(ImageButton), -1f);

		public static readonly BindableProperty HorizontalContentAlignmentProperty =
			BindableProperty.Create(nameof(HorizontalContentAlignment), typeof(TextAlignment), typeof(ImageButton), TextAlignment.Center);

		public static readonly BindableProperty ContentMarginProperty =
			BindableProperty.Create(nameof(ContentMargin), typeof(Thickness), typeof(ImageButton), default(Thickness));

		public static readonly BindableProperty BoldTextProperty =
			BindableProperty.Create(nameof(BoldText), typeof(string), typeof(ImageButton), string.Empty);

		[TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource Source
		{
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		[TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource DisabledSource
		{
			get { return (ImageSource)GetValue(DisabledSourceProperty); }
			set { SetValue(DisabledSourceProperty, value); }
		}

		public ImageOrientation Orientation
		{
			get { return (ImageOrientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		public int ImageHeightRequest
		{
			get { return (int)GetValue(ImageHeightRequestProperty); }
			set { SetValue(ImageHeightRequestProperty, value); }
		}

		public int ImageWidthRequest
		{
			get { return (int)GetValue(ImageWidthRequestProperty); }
			set { SetValue(ImageWidthRequestProperty, value); }
		}

		public bool Checked
		{
			get { return (bool)GetValue(CheckedProperty); }
			set { SetValue(CheckedProperty, value); }
		}

		public bool SupportCheckedState
		{
			get { return (bool)GetValue(SupportCheckedStateProperty); }
			set { SetValue(SupportCheckedStateProperty, value); }
		}

		public double UncheckedAlpha
		{
			get { return (double)GetValue(UncheckedAlphaProperty); }
			set { SetValue(UncheckedAlphaProperty, value); }
		}

		public int BadgeValue
		{
			get { return (int)GetValue(BadgeValueProperty); }
			set { SetValue(BadgeValueProperty, value); }
		}

		public Color BadgeColor
		{
			get { return (Color)GetValue(BadgeColorProperty); }
			set { SetValue(BadgeColorProperty, value); }
		}

		public Color BadgeSubColor
		{
			get { return (Color)GetValue(BadgeSubColorProperty); }
			set { SetValue(BadgeSubColorProperty, value); }
		}

		public double ShadowOffset
		{
			get { return (double)GetValue(ShadowOffsetProperty); }
			set { SetValue(ShadowOffsetProperty, value); }
		}

		public float ShadowOpacity
		{
			get { return (float)GetValue(ShadowOpacityProperty); }
			set { SetValue(ShadowOpacityProperty, value); }
		}

		public Color ShadowColor
		{
			get { return (Color)GetValue(ShadowColorProperty); }
			set { SetValue(ShadowColorProperty, value); }
		}

		public float ShadowRadius
		{
			get { return (float)GetValue(ShadowRadiusProperty); }
			set { SetValue(ShadowRadiusProperty, value); }
		}

		public TextAlignment HorizontalContentAlignment
		{
			get { return (TextAlignment)GetValue(HorizontalContentAlignmentProperty); }
			set { SetValue(HorizontalContentAlignmentProperty, value); }
		}

		public Thickness ContentMargin
		{
			get { return (Thickness)GetValue(ContentMarginProperty); }
			set { SetValue(ContentMarginProperty, value); }
		}

		public string BoldText
		{
			get { return (string)base.GetValue(BoldTextProperty); }
			set { base.SetValue(BoldTextProperty, value); }
		}
	}
}