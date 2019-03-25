using Xamarin.Forms;

namespace PlayOnCloud
{
	public enum CustomTextAlignment : long
	{
		Left = 0,
		Center = 1,
		Right = 2,
		Justified = 3,
		Natural = 4
	}

	public abstract class CustomLabelBase : Label
	{
		public static readonly BindableProperty LineSpacingProperty =
			BindableProperty.Create("LineSpacing", typeof(double), typeof(CustomLabel), -1.0d);

		public static readonly BindableProperty ParagraphStyleAlignmentProperty =
			BindableProperty.Create("ParagraphStyleAlignment", typeof(CustomTextAlignment), typeof(CustomLinkLabel), CustomTextAlignment.Left);

		public double LineSpacing
		{
			get { return (double)base.GetValue(LineSpacingProperty); }
			set { base.SetValue(LineSpacingProperty, value); }
		}

		public CustomTextAlignment ParagraphStyleAlignment
		{
			get { return (CustomTextAlignment)base.GetValue(ParagraphStyleAlignmentProperty); }
			set { base.SetValue(ParagraphStyleAlignmentProperty, value); }
		}
	}
}
