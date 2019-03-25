using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayOnCloud
{
	public class PopupArguments
	{
		public PopupBase Popup { get; }
		public TaskCompletionSource<bool> Result { get; }

		public PopupArguments(PopupBase popup)
		{
			Popup = popup;
			Result = new TaskCompletionSource<bool>();
		}

		public void SetResult(bool result)
		{
			this.Result.TrySetResult(result);
		}
	}
}
