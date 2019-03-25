using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class OfflinePopup : PopupBase
	{
		public OfflinePopup()
		{
			InitializeComponent();
		}

		private void CloseClicked(object sender, EventArgs args)
		{
			Close();
		}
	}
}
