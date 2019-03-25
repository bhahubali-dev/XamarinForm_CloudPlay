using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud.ViewModel
{
	public class Products : ViewModelBase
	{
		private ContentItemEx lastPurchasedItem;
		private volatile IInAppPurchase inAppPurchase;
		private Product recordingProduct;
		private Cloud cloudViewModel;
		private Account accountViewModel;
		private Queue queueViewModel;
		private object syncRoot = new object();

		public Products(Cloud cloudViewModel, Account accountViewModel, Queue queueViewModel)
		{
			this.cloudViewModel = cloudViewModel;
			this.accountViewModel = accountViewModel;
			this.queueViewModel = queueViewModel;

			PurchaseContentItem = new Command<IContentItem>(
				async (p) => await purchaseContentItem(p),
				(p) => { return (p != null); });
		}

		public Command PurchaseContentItem { protected set; get; }

		public override async Task Init()
		{
			if (inAppPurchase == null)
			{
				lock (syncRoot)
					if (inAppPurchase == null)
					{
						inAppPurchase = DependencyService.Get<IInAppPurchase>();
						inAppPurchase.OnProductsRetrieved += InAppPurchase_OnProductsRetrieved;
						inAppPurchase.OnProductPurchased += InAppPurchase_OnProductPurchased;
						inAppPurchase.OnTransactionFailed += InAppPurchase_OnTransactionFailed;
						inAppPurchase.OnValidationFailed += InAppPurchase_OnValidationFailed;
					}

				if (CurrentNetworkStatus != NetworkStatus.NotReachable)
					await requestProductData();
				else
					return;
			}

			await base.Init();
		}

		public bool CanMakePayments
		{
			get
			{
				if (inAppPurchase != null)
					return inAppPurchase.CanMakePayments;

				return false;
			}
		}

		public Product RecordingProduct
		{
			get { return recordingProduct; }
			set { SetField(ref recordingProduct, value); }
		}

		private void InAppPurchase_OnProductsRetrieved(object sender, List<Product> e)
		{
			if (e != null)
				RecordingProduct = e.FirstOrDefault();

			OnPropertyChanged("CanMakePayments");
		}

		private async void InAppPurchase_OnProductPurchased(object sender, string productIdentifier)
		{
			try
			{
				if (lastPurchasedItem == null)
					return;

				FacebookToolsService.Instance.LogPurchase();
				await queueViewModel.AddToQueue(lastPurchasedItem);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async void InAppPurchase_OnTransactionFailed(object sender, string localizedDescription)
		{
			lastPurchasedItem = null;
			IsLoading = false;
			if (!string.IsNullOrEmpty(localizedDescription))
				await Application.Current.MainPage.DisplayAlert("Error", localizedDescription, "OK");
		}

		private async void InAppPurchase_OnRequestFailed(object sender, string localizedDescription)
		{
			lastPurchasedItem = null;
			IsLoading = false;
			await Application.Current.MainPage.DisplayAlert("Error", localizedDescription, "OK");
		}

		private async void InAppPurchase_OnValidationFailed(object sender, string error)
		{
			IsLoading = false;
			string message = "Unable to validate receipt. Please make sure you are connected to the internet and try again.";
			if (!string.IsNullOrEmpty(error))
				message += " Error: " + error;

			await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
		}

		private async Task purchaseContentItem(IContentItem item)
		{
			if ((item == null) || (inAppPurchase == null) || !(item is ContentItemEx))
				return;

			if ((item as ContentItemEx).InQueue)
			{
				cloudViewModel.SelectedItem = CloudItem.Queue;
				if ((Application.Current.MainPage as NavigationPage).CurrentPage is DetailsPage)
					await Application.Current.MainPage.Navigation.PopAsync();

				return;
			}

			if (!accountViewModel.SignedIn)
			{
				(Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).Register.View = Model.RegisterViewMode.None;
				(Application.Current.BindingContext as PlayOnCloud.ViewModel.Cloud).Register.Type = RegisterViewType.Queue;

				await Application.Current.MainPage.Navigation.PushAsync(new PlayOnCloud.Register() { BindingContext = Application.Current.BindingContext, SkipMainPageInitialization = true });
				return;
			}

			IsLoading = true;
			try
			{
				await accountViewModel.FetchUserCreditsAsync();
				if (accountViewModel.UserInfo.Credits > 0)
				{
					await queueViewModel.AddToQueue(item as ContentItemEx);
					IsLoading = false;
				}
				else if (RecordingProduct != null)
				{
					if (!CanMakePayments)
					{
						IsLoading = false;
						await Application.Current.MainPage.DisplayAlert("Purchasing Disabled", "In-App Purchases are disabled on this device. If you want to buy this recording, you must first enable In-App Purchases under Settings > General > Restrictions, and then try again.", "OK");
						return;
					}

					lastPurchasedItem = item as ContentItemEx;
					inAppPurchase.PurchaseProduct(RecordingProduct, lastPurchasedItem);
				}
			}
			catch
			{
				IsLoading = false;
			}
		}

		private async Task requestProductData()
		{
			try
			{
				var products = await ProductClient.Get();
				if ((products != null) && (products.Entries != null) && products.Entries.Any())
					inAppPurchase.RequestProductData(products.Entries.Select(p => p.ExternalPID).ToList());
			}
			catch
			{
				//XXX: handle error
			}
		}

		public override Task Poll(bool showLoading)
		{
			OnPropertyChanged("CanMakePayments");
			return null;
		}

		public override async Task NetworkStatusChanged(NetworkStatus newStatus)
		{
			if (newStatus != NetworkStatus.NotReachable)
				await Init();

			await base.NetworkStatusChanged(newStatus);
		}
	}
}
