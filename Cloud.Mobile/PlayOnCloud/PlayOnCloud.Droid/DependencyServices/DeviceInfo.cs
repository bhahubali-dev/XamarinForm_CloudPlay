using System;
using Xamarin.Forms;
using PlayOnCloud.Model;
using PlayOnCloud.Droid;
using Android.Content.Res;

[assembly: Dependency(typeof(DeviceInfo))]
namespace PlayOnCloud.Droid
{
	public class DeviceInfo : IDeviceInfo
	{
		public Size GetFullScreenSize()
		{
            int intHeight = (int)(Application.Current.MainPage.Height * GetScreenScale());
            int intWidth = (int)(Application.Current.MainPage.Width * GetScreenScale());
            return new Size(intWidth, intHeight);
        }

		public DeviceOrientation GetScreenOrientation()
		{
            return (Application.Current.MainPage.Width < Application.Current.MainPage.Height) ? DeviceOrientation.Portrait : DeviceOrientation.Landscape;
           
		}

		public double GetScreenScale()
		{
            return Application.Current.MainPage.Scale;
        }

		public Size GetScreenSize()
		{
            int intHeight = (int)(Application.Current.MainPage.Height);
            int intWidth = (int)(Application.Current.MainPage.Width);
            return new Size(intWidth, intHeight);
        }
	}
}