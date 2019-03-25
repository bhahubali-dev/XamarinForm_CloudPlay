using System;
using System.Globalization;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class SplashScreenBackgroundConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				string param = parameter?.ToString();
				if ((Device.OS == TargetPlatform.iOS) && !string.IsNullOrWhiteSpace(param))
				{
					string[] imageData = param.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

					if (imageData?.Length > 0)
					{
						string imagePath = imageData[0];
						string imageExtension = (imageData.Length > 1) ? imageData[1] : "png";

						IDeviceInfo deviceInfo = DependencyService.Get<IDeviceInfo>();
						if (deviceInfo != null)
						{
							Size fullScreen = deviceInfo.GetFullScreenSize();
							double screenHeight = fullScreen.Height;

							if (Device.Idiom == TargetIdiom.Phone)
							{
								if (screenHeight >= 2208)
									return imagePath + "-736h@3x." + imageExtension;
								else if (screenHeight >= 1334)
									return imagePath + "-667h@2x." + imageExtension;
								else if (screenHeight >= 1136)
									return imagePath + "-568h@2x." + imageExtension;
								else
									return imagePath + "@2x." + imageExtension;
							}
							else if (Device.Idiom == TargetIdiom.Tablet)
							{
								if (deviceInfo.GetScreenOrientation() == Model.DeviceOrientation.Portrait)
								{
									if (screenHeight >= 2048)
										return imagePath + "-Portrait@2x." + imageExtension;
									else
										return imagePath + "-Portrait." + imageExtension;
								}
								else
								{
									if (fullScreen.Width >= 2208)
										return imagePath + "-Landscape@3x." + imageExtension;
									else if (fullScreen.Width >= 2048)
										return imagePath + "-Landscape@2x." + imageExtension;
									else
										return imagePath + "-Landscape." + imageExtension;
								}
							}
						}
					}
				}
                else if ((Device.OS == TargetPlatform.Android) && !string.IsNullOrWhiteSpace(param))
				{
                    IDeviceInfo deviceInfo = DependencyService.Get<IDeviceInfo>();
                    if (deviceInfo != null)
                    {
                        Size fullScreen = deviceInfo.GetFullScreenSize();
                        double screenHeight = fullScreen.Height;

                        if (Device.Idiom == TargetIdiom.Phone)
                        {
                            if (deviceInfo.GetScreenOrientation() == Model.DeviceOrientation.Portrait)
                            {
                                return "backgroundimage.jpg";
                            }
                            else
                            {
                                return "backgroundimagelandscape.jpg";
                            }
                        }
                        else if (Device.Idiom == TargetIdiom.Tablet)
                        {
                            if (deviceInfo.GetScreenOrientation() == Model.DeviceOrientation.Portrait)
                            {
                                return "backgroundimage.jpg";
                            }
                            else
                            {
                                return "backgroundimagelandscape.jpg";
                            }
                        }
                    }
                    return "backgroundimage.jpg";

				}
			}
			catch
			{
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
