using Foundation;
using UIKit;

namespace PlayOnCloud.iOS.Controls
{
	[Register("CustomTextField")]
	public class CustomTextField : UITextField
	{
		public string ResponderName { get; set; }

		public string NextResponderName { get; set; }
	}
}
