using System;
using Android.App;
using Android.Graphics.Drawables;
using Android.Views;
using Xamarin.Forms;
using Color = Android.Graphics.Color;

namespace PlayOnCloud.Droid.Popups
{
    internal class PopupDialogContainer
    {
        private readonly Dialog _dialog;
        private readonly PopupBase _popup;
        private readonly PopupArguments _popupArguments;

        public PopupDialogContainer(PopupArguments popupArguments)
        {
            _popupArguments = popupArguments;
            _popup = popupArguments.Popup;

            _dialog = new Dialog(Forms.Context);
            _dialog.RequestWindowFeature((int) WindowFeatures.NoTitle);

            _popup.CloseRequest += OnCloseRequest;
        }

        public void Show()
        {
            var viewGroup = FormsViewHelper.ConvertFormsToNative(_popup, DisplayHelper.GetSize());
            _dialog.SetContentView(viewGroup);

            // to show dialog container 'fullscreen'
            var lp = new WindowManagerLayoutParams();
            lp.CopyFrom(_dialog.Window.Attributes);
            lp.Width = ViewGroup.LayoutParams.MatchParent;
            lp.Height = ViewGroup.LayoutParams.MatchParent;
            _dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            _dialog.Window.SetSoftInputMode(SoftInput.StateVisible);
            _dialog.Show();
            _dialog.Window.Attributes = lp;
        }

        public void Close()
        {
            _dialog.Dismiss();
            _popup.CloseRequest -= OnCloseRequest;
            _popupArguments.SetResult(true);
        }

        private void OnCloseRequest(object sender, EventArgs e)
        {
            Close();
        }
    }
}