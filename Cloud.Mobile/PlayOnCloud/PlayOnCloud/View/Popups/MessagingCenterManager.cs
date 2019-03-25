using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	internal static class MessagingCenterManager
	{
		public static Task<bool> ShowPopup(PopupBase popup)
		{
			var popupArguments = new PopupArguments(popup);
			MessagingCenter.Send(Application.Current.MainPage, Messages.DisplayPopupMessage, popupArguments);
			return popupArguments.Result.Task;
		}

		public static Task<bool> CloseAllPopups()
		{
			var popupArguments = new PopupArguments(null);
			MessagingCenter.Send(Application.Current.MainPage, Messages.CloseAllPopupsMessage, popupArguments);
			return popupArguments.Result.Task;
		}
	}
}
