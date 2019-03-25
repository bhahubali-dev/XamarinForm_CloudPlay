using System;

namespace PlayOnCloud
{
	public class BackButtonHelper
	{
		public static event EventHandler BackButtonPressed;

		public static void FireBackButtonPressed()
		{
			BackButtonPressed?.Invoke(null, new EventArgs());
		}
	}
}