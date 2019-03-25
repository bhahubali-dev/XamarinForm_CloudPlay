using PlayOnCloud;
using PlayOnCloud.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SearchEntry), typeof(SearchEntryRenderer))]
namespace PlayOnCloud.iOS
{
	public class SearchEntryRenderer : RoundEntryRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{
				UITextField textField = Control;
				textField.KeyboardType = UIKeyboardType.Default;
				textField.ReturnKeyType = UIReturnKeyType.Done;
				textField.EnablesReturnKeyAutomatically = true;
			}
		}
	}
}
