using Foundation;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace PlayOnCloud.iOS
{
	public class PopupDialogContainer
	{
		PopupArguments popupArguments;
		PopupBase popup;
		UIView view;
		NSObject orientationChangeObserver;
		IVisualElementRenderer renderer;

		public PopupDialogContainer(PopupArguments popupArguments)
		{
			this.popupArguments = popupArguments;
			popup = popupArguments.Popup;
			popup.CloseRequest += OnCloseRequest;
			orientationChangeObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification, OrientationChanged);
		}

		public void Show()
		{
			var parentWindow = UIApplication.SharedApplication.KeyWindow;
			view = FormsViewHelper.ConvertFormsToNative(popup, parentWindow.Bounds, out renderer);
			parentWindow.AddSubview(view);
		}

		public void Close()
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver(orientationChangeObserver);

			if (view != null)
				view.RemoveFromSuperview();

			popup.CloseRequest -= OnCloseRequest;
			popupArguments.SetResult(popup.Result);
		}

		void OnCloseRequest(object sender, System.EventArgs e)
		{
			Close();
		}

		void OrientationChanged(NSNotification notification)
		{
			if (renderer != null)
				renderer.Element.Layout(UIApplication.SharedApplication.KeyWindow.Bounds.ToRectangle());
		}
	}
}
