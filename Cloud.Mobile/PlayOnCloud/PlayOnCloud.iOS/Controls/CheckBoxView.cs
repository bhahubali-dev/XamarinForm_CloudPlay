using CoreGraphics;
using Foundation;
using UIKit;

namespace PlayOnCloud.iOS.Controls
{
	[Register("CheckBoxView")]
	public class CheckBoxView : UIButton
	{
		public CheckBoxView()
		{
			Initialize();
		}

		public CheckBoxView(CGRect bounds)
			: base(bounds)
		{
			Initialize();
		}

		public bool Checked
		{
			set { Selected = value; }
			get { return Selected; }
		}

		void Initialize()
		{
			TouchUpInside += (sender, args) => Selected = !Selected;
		}
	}
}
