using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public interface IFacebookTools
	{
		void ActivateApp();
		void LogPurchase();
		void LogCustomEvent(string eventName);
	}

	public class FacebookToolsService
	{
		private static volatile IFacebookTools instance;
		private static readonly object syncRoot = new object();

		public static IFacebookTools Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<IFacebookTools>();

				return instance;
			}
		}
	}
}
