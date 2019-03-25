using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Foundation;
using UIKit;
using PlayOnCloud.iOS;
using PlayOnCloud.iOS.Tools;

[assembly: Dependency(typeof(KeyboardHelper))]
namespace PlayOnCloud.iOS
{
	public class KeyboardHelper : IKeyboardHelper
	{
		public KeyboardHelper()
		{
			NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
			NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
		}

		public event EventHandler<KeyboardHelperEventArgs> KeyboardChanged;
		public event EventHandler<float> KeyboardShown;

		private void OnKeyboardNotification(NSNotification notification)
		{
			float neededOffset = 0;
			var visible = notification.Name == UIKeyboard.WillShowNotification;
			var keyboardFrame = visible ? UIKeyboard.FrameEndFromNotification(notification) : UIKeyboard.FrameBeginFromNotification(notification);

			if (visible)
				KeyboardShown?.Invoke(this, (float)keyboardFrame.Height);

			var parentView = UIApplication.SharedApplication.KeyWindow.RootViewController.View;
			bool isSubview = false;
			var responder = InputTools.FindFirstResponder(parentView);
			if ((responder == null) && (UIApplication.SharedApplication.KeyWindow.Subviews != null))
			{
				foreach (var subview in UIApplication.SharedApplication.KeyWindow.Subviews)
				{
					responder = InputTools.FindFirstResponder(subview);
					if (responder != null)
					{
						parentView = subview;
						isSubview = true;
						break;
					}
				}
			}

			if (responder != null)
			{
				var offsetFromBottom = getOffsetFromBottom(responder);
				if (responder.Tag > 0)
				{
					var views = getTaggedViews(parentView, responder.Tag);
					if (views.Any())
					{
						var offsets = new List<nfloat>();
						foreach (var view in views)
							offsets.Add(getOffsetFromBottom(view));

						offsetFromBottom = offsets.Min();
					}
				}

				if (keyboardFrame.Height > offsetFromBottom)
					neededOffset = (float)(keyboardFrame.Height - offsetFromBottom + responder.Frame.Height);
			}

			if ((KeyboardChanged != null) && ((neededOffset > 0) || !visible))
			{
				if (isSubview)
					parentView.Transform = CoreGraphics.CGAffineTransform.MakeTranslation(0, -neededOffset);
				else
					KeyboardChanged(this, new KeyboardHelperEventArgs(visible, (float)keyboardFrame.Height, neededOffset));
			}
		}

		private List<UIView> getTaggedViews(UIView root, nint tag)
		{
			List<UIView> result = new List<UIView>();
			if ((root.Tag == tag) && !root.Hidden)
				result.Add(root);

			foreach (var subview in root.Subviews)
				result.AddRange(getTaggedViews(subview, tag));

			return result;
		}

		private nfloat getOffsetFromBottom(UIView view)
		{
			if (view != null)
			{
				var point = view.Superview.ConvertPointToView(view.Frame.Location, null);
				var bottom = point.Y + view.Frame.Height;
				return UIApplication.SharedApplication.KeyWindow.RootViewController.View.Frame.Height - bottom;
			}

			return 0;
		}
	}
}
