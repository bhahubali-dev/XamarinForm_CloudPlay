using System;
using System.Net;
using System.Net.Http;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public enum NetworkStatus
	{
		NotReachable,
		ReachableViaCarrierDataNetwork,
		ReachableViaWiFiNetwork
	}

	public interface IReachabilityHelper
	{
		NetworkStatus InternetConnectionStatus { get; }

		event EventHandler<NetworkStatus> NetworkStatusChanged;
	}

	public static class ReachabilityHelperService
	{
		private static volatile IReachabilityHelper reachabilityHelper;
		private static object syncRoot = new object();

		public static IReachabilityHelper Instance
		{
			get
			{
				if (reachabilityHelper == null)
					lock (syncRoot)
						if (reachabilityHelper == null)
							reachabilityHelper = DependencyService.Get<IReachabilityHelper>();

				return reachabilityHelper;
			}
		}

		public static event EventHandler<NetworkStatus> NetworkStatusChanged
		{
			add
			{
				Instance.NetworkStatusChanged += value;
			}
			remove
			{
				Instance.NetworkStatusChanged -= value;
			}
		}
	}
}
