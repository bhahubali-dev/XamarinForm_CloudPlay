using Xamarin.Forms;

namespace PlayOnCloud
{
	public class CustomLinkLabel : CustomLabelBase
	{
		public static readonly BindableProperty UrlTextProperty =
			BindableProperty.Create("UrlText", typeof(string), typeof(CustomLinkLabel), string.Empty);

		public static readonly BindableProperty UrlProperty =
			BindableProperty.Create("Url", typeof(string), typeof(CustomLinkLabel), string.Empty);

		public static readonly BindableProperty LinkColorProperty =
			BindableProperty.Create("LinkColor", typeof(Color), typeof(CustomLinkLabel), Color.FromHex("#04a3ff"));

		public string UrlText
		{
			get { return (string)base.GetValue(UrlTextProperty); }
			set { base.SetValue(UrlTextProperty, value); }
		}

		public string Url
		{
			get { return (string)base.GetValue(UrlProperty); }
			set { base.SetValue(UrlProperty, value); }
		}

		public Color LinkColor
		{
			get { return (Color)base.GetValue(LinkColorProperty); }
			set { base.SetValue(LinkColorProperty, value); }
		}
	}
}
