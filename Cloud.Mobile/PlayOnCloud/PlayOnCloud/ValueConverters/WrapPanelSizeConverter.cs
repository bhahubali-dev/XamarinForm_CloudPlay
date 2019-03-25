using System;
using System.Globalization;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public class WrapPanelSizeConverter : IValueConverter
    {
        private IDeviceInfo deviceInfo;
        private double phoneSize = 90;
        private double tabletSize = 200;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (deviceInfo == null)
                {
                    deviceInfo = DependencyService.Get<IDeviceInfo>();
                    if (deviceInfo != null)
                        phoneSize = (deviceInfo.GetScreenSize().Width - 60) / 3;
                }
            }
            catch
            {
            }

            switch (Device.Idiom)
            {
                case TargetIdiom.Tablet:
                    return phoneSize;

                case TargetIdiom.Phone:
                    return phoneSize;

                default:
                    return 90;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}