using Xamarin.Forms;
using UIKit;
using PlayOnCloud.iOS;
using PlayOnCloud.Model;

[assembly: Dependency(typeof(DeviceInfo))]
namespace PlayOnCloud.iOS
{
	public class DeviceInfo : IDeviceInfo
	{
		public Size GetFullScreenSize()
		{
			return new Size((UIScreen.MainScreen.Bounds.Width * GetScreenScale()), (UIScreen.MainScreen.Bounds.Height * GetScreenScale()));
		}

		public Size GetScreenSize()
		{
			return new Size(UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
		}

		public DeviceOrientation GetScreenOrientation()
		{
			return (UIScreen.MainScreen.Bounds.Width < UIScreen.MainScreen.Bounds.Height) ? DeviceOrientation.Portrait : DeviceOrientation.Landscape;
		}

		public double GetScreenScale()
		{
			return UIScreen.MainScreen.Scale;
		}
	}
}
