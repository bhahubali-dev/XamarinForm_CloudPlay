namespace PlayOnCloud
{
	public interface IDeviceInfo
	{
		Xamarin.Forms.Size GetFullScreenSize();

		Xamarin.Forms.Size GetScreenSize();

		Model.DeviceOrientation GetScreenOrientation();

		double GetScreenScale();
	}
}