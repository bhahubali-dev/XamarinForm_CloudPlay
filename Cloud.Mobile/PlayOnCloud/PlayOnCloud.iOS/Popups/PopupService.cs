using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace PlayOnCloud.iOS
{
    internal class PopupService
    {
        private static readonly PopupService Instance = new PopupService();

        public static void Init()
        {
            MessagingCenter.Subscribe<Page, PopupArguments>(Instance, Messages.DisplayPopupMessage, Instance.Show);
            MessagingCenter.Subscribe<Page, PopupArguments>(Instance, Messages.CloseAllPopupsMessage, Instance.CloseAll);
        }

        public void Show(Page page, PopupArguments args)
        {
            args.Popup.Parent = page;

            var container = new PopupDialogContainer(args);
            container.Show();
        }

        public void CloseAll(Page page, PopupArguments args)
        {
            var subviews =
                UIApplication.SharedApplication.KeyWindow.Subviews.Where(v => v is IVisualElementRenderer).ToList();
            foreach (var subview in subviews)
                subview.RemoveFromSuperview();

            args.SetResult(true);
        }
    }
}