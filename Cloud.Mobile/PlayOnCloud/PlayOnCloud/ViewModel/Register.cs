using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud.ViewModel
{
	public class Register : ViewModelBase
	{
		private RegisterViewMode view = RegisterViewMode.None;
		private RegisterViewType type = RegisterViewType.Launch;

		private bool forceHideView;

		public Register()
		{
			SwitchView = new Command<RegisterViewMode>((p) =>
			{
				bool isMainPageNavigated = false;
				foreach (var page in Application.Current.MainPage?.Navigation?.NavigationStack)
				{
					if (page is Main)
					{
						isMainPageNavigated = true;
						break;
					}
				}

				bool isFromAccount = ((Application.Current.MainPage.BindingContext as ViewModel.Cloud).SelectedItem == CloudItem.Account);
				bool isFromResetPassword = ((Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).Register.View == RegisterViewMode.ResetPassword);
				bool isFromLaunch = ((Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).Register.Type == RegisterViewType.Launch);

				if (isMainPageNavigated && isFromAccount && isFromLaunch && !isFromResetPassword && (p == RegisterViewMode.None))
					Application.Current.MainPage.Navigation.PopAsync(true);
				else
				{
					if (isFromResetPassword)
						p = RegisterViewMode.Login;

					View = p;
				}
			});
		}

		public ICommand SwitchView { protected set; get; }

		public RegisterViewMode View
		{
			get { return view; }
			set { SetField(ref view, value); }
		}

		public RegisterViewType Type
		{
			get { return type; }
			set { SetField(ref type, value); }
		}

		public override Task OnBackButtonPressed()
		{
			View = RegisterViewMode.None;
			return base.OnBackButtonPressed();
		}

		public bool ForceHideView
		{
			get { return forceHideView; }
			set { SetField(ref forceHideView, value); }
		}
	}
}
