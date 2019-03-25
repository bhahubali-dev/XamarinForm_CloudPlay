using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace PlayOnCloud
{
	public partial class LegalDisclaimer : PopupBase
	{
		public LegalDisclaimer()
		{
			InitializeComponent();
		}

		public void GotItClick(object sender, EventArgs args)
		{
			Result = true;
			Close();
		}

		public void CancelClick(object sender, EventArgs args)
		{
			Close();
		}
	}
}
