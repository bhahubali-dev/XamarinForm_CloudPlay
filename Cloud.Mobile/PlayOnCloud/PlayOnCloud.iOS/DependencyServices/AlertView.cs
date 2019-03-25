using Xamarin.Forms;
using UIKit;
using PlayOnCloud.iOS;

[assembly: Dependency(typeof(AlertView))]
namespace PlayOnCloud.iOS
{
	public class AlertView : IAlertView
	{
		public void ShowAlert(string message)
		{
			UIAlertView alert = new UIAlertView();
			alert.Title = "";
			alert.AddButton("OK");
			alert.Message = message;
			alert.AlertViewStyle = UIAlertViewStyle.Default;
			alert.Show();
		}
	}
}