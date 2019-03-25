using System;
using System.Net;
using Android.Content;
using Android.Net;
using PlayOnCloud.Droid;
using Xamarin.Forms;
using Application = Android.App.Application;

[assembly: Dependency(typeof(ReachabilityHelper))]
namespace PlayOnCloud.Droid
{
	public class ReachabilityHelper : IReachabilityHelper
	{
        public event EventHandler<NetworkStatus> NetworkStatusChanged;

        public ReachabilityHelper()
        {
            Reachability.ReachabilityChanged += Reachability_ReachabilityChanged;
        }

        public NetworkStatus InternetConnectionStatus
        {
            get { return InternetConnectionStatusCheck(); }
        }

        private void Reachability_ReachabilityChanged(object sender, EventArgs e)
        {
            var networkStatusChanged = NetworkStatusChanged;
            if (networkStatusChanged != null)
                networkStatusChanged(this, InternetConnectionStatusCheck());
        }

        public bool IsConnected { get; set; }
        public void CheckNetworkConnection()
        {
            var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;

            if (activeNetworkInfo != null && activeNetworkInfo.IsConnectedOrConnecting)
                IsConnected = true;
            else
                IsConnected = false;
        }

        public NetworkStatus InternetConnectionStatusCheck()
        {
            var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;

            if (activeNetworkInfo != null && activeNetworkInfo.IsConnectedOrConnecting)
            {
                IsConnected = true;
                if (activeNetworkInfo.TypeName == "WIFI")
                    return NetworkStatus.ReachableViaWiFiNetwork;
                else if (activeNetworkInfo.TypeName == "MOBILE")
                    return NetworkStatus.ReachableViaCarrierDataNetwork;
            }
            else
            {
                IsConnected = false;
                return NetworkStatus.NotReachable;
            }
            return NetworkStatus.NotReachable;
        }
    }
}