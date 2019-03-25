using Xamarin.Forms;

namespace PlayOnCloud
{
	public class CustomLabel : CustomLabelBase
	{
		public static readonly BindableProperty LinesCountProperty =
			BindableProperty.Create("LinesCount", typeof(int), typeof(CustomLabel), -1);

		public static readonly BindableProperty BoldTextProperty =
			BindableProperty.Create("BoldText", typeof(string), typeof(CustomLabel), string.Empty);

		public int LinesCount
		{
			get { return (int)base.GetValue(LinesCountProperty); }
			set { base.SetValue(LinesCountProperty, value); }
		}

		public string BoldText
		{
			get { return (string)base.GetValue(BoldTextProperty); }
			set { base.SetValue(BoldTextProperty, value); }
		}
	}
}
