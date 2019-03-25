using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class ContentItemDetailsView : ContentView
	{
		public ContentItemDetailsView()
		{
			InitializeComponent();
		}

		public static readonly BindableProperty IsInDetailsPageProperty =
			BindableProperty.Create("IsInDetailsPage", typeof(bool), typeof(ContentItemDetailsView), false);

		public bool IsInDetailsPage
		{
			get { return (bool)GetValue(IsInDetailsPageProperty); }
			set { SetValue(IsInDetailsPageProperty, value); }
		}
	}
}
