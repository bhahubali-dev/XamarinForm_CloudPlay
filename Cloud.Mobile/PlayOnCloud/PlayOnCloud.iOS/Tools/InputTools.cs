using UIKit;

namespace PlayOnCloud.iOS.Tools
{
	internal static class InputTools
	{
		internal static UIView FindFirstResponder(UIView view)
		{
			if (view.IsFirstResponder)
				return view;

			foreach (var subview in view.Subviews)
			{
				var responder = FindFirstResponder(subview);
				if (responder != null)
					return responder;
			}

			return null;
		}
	}
}
