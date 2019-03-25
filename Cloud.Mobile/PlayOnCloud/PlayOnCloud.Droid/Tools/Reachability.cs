using System;
using System.Net;
using Android.App;
using Android.Content;
using Android.Net;
using PlayOnCloud.Droid;
using Xamarin.Forms.PlatformConfiguration;

namespace PlayOnCloud
{
    public static class Reachability
    {
        public static string HostName = "www.google.com";


        public static event EventHandler ReachabilityChanged;

       

        public static NetworkStatus RemoteHostStatus()
        {

            return InternetConnectionStatusCheck();
        }

        public static NetworkStatus InternetConnectionStatus()
        {
            

            return InternetConnectionStatusCheck();
        }

        public static NetworkStatus LocalWifiConnectionStatus()
        {
           

            return InternetConnectionStatusCheck();

        }

        public  static NetworkStatus InternetConnectionStatusCheck()
        {
            var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;

            if (activeNetworkInfo != null && activeNetworkInfo.IsConnectedOrConnecting)
            {
                
                if (activeNetworkInfo.TypeName == "WIFI")
                    return NetworkStatus.ReachableViaWiFiNetwork;
                else if (activeNetworkInfo.TypeName == "MOBILE")
                    return NetworkStatus.ReachableViaCarrierDataNetwork;
            }
            else
            {
               
                return NetworkStatus.NotReachable;
            }
            return NetworkStatus.NotReachable;
        }
    }
}