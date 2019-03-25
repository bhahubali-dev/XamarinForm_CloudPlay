using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class ChannelLoginInfoView : PopupBase
	{
		public ChannelLoginInfoView()
		{
			InitializeComponent();
		}

		private void Button_OnClicked(object sender, EventArgs e)
		{
			Close();
		}
	}
}
