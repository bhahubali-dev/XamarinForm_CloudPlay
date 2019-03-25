using System;
using System.Net;
using Xamarin.Forms;
using PlayOnCloud.iOS;

[assembly: Dependency(typeof(ReachabilityHelper))]
namespace PlayOnCloud.iOS
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
			get { return Reachability.InternetConnectionStatus(); }
		}

		private void Reachability_ReachabilityChanged(object sender, EventArgs e)
		{
			var networkStatusChanged = NetworkStatusChanged;
			if (networkStatusChanged != null)
				networkStatusChanged(this, Reachability.InternetConnectionStatus());
		}
	}
}