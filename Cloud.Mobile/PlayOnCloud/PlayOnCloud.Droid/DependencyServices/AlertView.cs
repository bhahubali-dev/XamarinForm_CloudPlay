using System;
using Android.App;
using Xamarin.Forms;
using PlayOnCloud.Droid;

[assembly: Dependency(typeof(AlertView))]
namespace PlayOnCloud.Droid
{
	public class AlertView : IAlertView
	{
		public void ShowAlert(string message)
		{
            var ad = new AlertDialog.Builder(Forms.Context);
            ad.SetTitle("");
            ad.SetMessage(message);
            ad.SetPositiveButton("OK", delegate { ad.Dispose(); });
            ad.Show();

        }
    }
}