using System.Windows.Input;
using UIKit;

namespace PlayOnCloud.iOS.Controls
{
	public class FormsUIRefreshControl : UIRefreshControl
	{

		private bool isRefreshing;

		public ICommand RefreshCommand { get; set; }

		public FormsUIRefreshControl()
		{
		    ValueChanged += (sender, e) =>
		    {
		        var command = RefreshCommand;
		        if (command != null)
		            command.Execute(null);

		        IsRefreshing = false;
		    };
		}

		public bool IsRefreshing
		{
			get { return isRefreshing; }
			set
			{
				isRefreshing = value;
				if (isRefreshing)
					BeginRefreshing();
				else
					EndRefreshing();
			}
		}
	}
}