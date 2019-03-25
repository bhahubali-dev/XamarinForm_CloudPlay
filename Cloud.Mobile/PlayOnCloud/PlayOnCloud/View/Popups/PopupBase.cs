using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public abstract class PopupBase : ContentView
	{
	    public event EventHandler CloseRequest = delegate { };

	    public bool Result { get; set; }

	    public void Close()
	    {
	        Parent = null;
	        RaiseCloseRequest();
	    }

	    private void RaiseCloseRequest()
	    {
	        CloseRequest(this, EventArgs.Empty);
	    }


	}
}
