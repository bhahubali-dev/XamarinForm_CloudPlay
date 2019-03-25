using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Foundation;
using StoreKit;
using PlayOnCloud.iOS;
using PlayOnCloud.iOS.Extensions;
using PlayOnCloud.Model;

[assembly: Dependency(typeof(InAppPurchase))]
namespace PlayOnCloud.iOS
{
	public class InAppPurchase : IInAppPurchase
	{
		private NSObject priceObserver;
		private NSObject succeededObserver;
		private NSObject failedObserver;
		private NSObject requestObserver;
		private NSObject validationObserver;
		private InAppPurchaseManager inAppPurchaseManager;
		private List<SKProduct> retrievedProducts;

		public InAppPurchase()
		{
			retrievedProducts = new List<SKProduct>();
			inAppPurchaseManager = new InAppPurchaseManager();

			priceObserver = NSNotificationCenter.DefaultCenter.AddObserver(InAppPurchaseManager.InAppPurchaseManagerProductsFetchedNotification,
				(notification) =>
				{
					try
					{
						Logger.Log("ProductsFetchedNotification...");
						var info = notification.UserInfo;
						if (info == null)
						{
							Fire_ProductsRetrieved(null);
							Logger.Log("WARNING: ProductsFetchedNotification info is null");
							return;
						}

						List<Product> products = new List<Product>();
						foreach (var key in info.Keys)
						{
							var product = (SKProduct)info.ObjectForKey(key);
							retrievedProducts.RemoveAll(p => p.ProductIdentifier == product.ProductIdentifier);
							retrievedProducts.Add(product);

							Logger.Log("ProductsFetchedNotification: Product ID: " + product.ProductIdentifier);

							products.Add(new Product()
							{
								ProductIdentifier = product.ProductIdentifier,
								LocalizedTitle = product.LocalizedTitle,
								LocalizedDescription = product.LocalizedDescription,
								Price = product.Price.DoubleValue,
								LocalizedPrice = product.LocalizedPrice()
							});
						}

						Fire_ProductsRetrieved(products);
					}
					catch (Exception ex)
					{
						Logger.Log("ERROR: ProductsFetchedNotification: " + ex);
					}
				});

			succeededObserver = NSNotificationCenter.DefaultCenter.AddObserver(InAppPurchaseManager.InAppPurchaseManagerTransactionSucceededNotification,
				(notification) =>
				{
					try
					{
						Logger.Log("TransactionSucceededNotification");
						var transaction = getTransactionFromNotification(notification);
						if ((transaction != null) && (transaction.TransactionReceipt != null))
							Logger.Log("Transaction: " + transaction.TransactionReceipt.ToString());

						if ((transaction != null) && (transaction.TransactionState == SKPaymentTransactionState.Purchased) && (transaction.Payment != null))
							Fire_OnProductPurchased(transaction.Payment.ProductIdentifier);
					}
					catch (Exception ex)
					{
						Logger.Log("ERROR: TransactionSucceededNotification: " + ex);
					}
				});

			failedObserver = NSNotificationCenter.DefaultCenter.AddObserver(InAppPurchaseManager.InAppPurchaseManagerTransactionFailedNotification,
				(notification) =>
				{
					try
					{
						Logger.Log("TransactionFailedNotification");
						var transaction = getTransactionFromNotification(notification);
						string error = string.Empty;

						if ((transaction != null) && (transaction.TransactionReceipt != null))
							Logger.Log("Transaction: " + transaction.TransactionReceipt.ToString());

						//if not canceled by user
						if ((transaction != null) && (transaction.Error != null) && (transaction.Error.Code != 2))
							error = transaction.Error.LocalizedDescription;

						Fire_OnTransactionFailed(error);
					}
					catch (Exception ex)
					{
						Logger.Log("ERROR: TransactionFailedNotification: " + ex);
					}
				});

			requestObserver = NSNotificationCenter.DefaultCenter.AddObserver(InAppPurchaseManager.InAppPurchaseManagerRequestFailedNotification,
				(notification) =>
				{
					try
					{
						string error = "Request Failed";
						if (notification.UserInfo != null)
						{
							var errorKey = new NSString("error");
							if (notification.UserInfo.ContainsKey(errorKey))
								error = ((NSError)notification.UserInfo.ObjectForKey(errorKey)).LocalizedDescription;
						}

						Logger.Log("RequestFailedNotification: " + error);
						Fire_OnRequestFailed(error);
					}
					catch (Exception ex)
					{
						Logger.Log("ERROR: RequestFailedNotification: " + ex);
					}
				});

			validationObserver = NSNotificationCenter.DefaultCenter.AddObserver(InAppPurchaseManager.InAppPurchaseManagerValidationFailedNotification,
				(notification) =>
				{
					try
					{
						string error = string.Empty;
						var info = notification.UserInfo;
						if (info != null)
						{
							var transactionKey = new NSString("transaction");
							if (info.ContainsKey(transactionKey))
								error = (NSString)info.ObjectForKey(transactionKey);
						}

						Fire_OnValidationFailed(error);
					}
					catch (Exception ex)
					{
						Logger.Log("ERROR: ValidationFailedNotification: " + ex);
					}
				});
		}

		public event EventHandler<List<Product>> OnProductsRetrieved;

		public event EventHandler<string> OnProductPurchased;

		public event EventHandler<string> OnTransactionFailed;

		public event EventHandler<string> OnRequestFailed;

		public event EventHandler<string> OnValidationFailed;

		public bool CanMakePayments
		{
			get { return inAppPurchaseManager.CanMakePayments(); }
		}

		public void RequestProductData(List<string> productIDs)
		{
			inAppPurchaseManager.RequestProductData(productIDs);
		}

		public void PurchaseProduct(Product product, ContentItemDetails item)
		{
			if (item != null)
				Logger.Log("PurchaseProduct: " + item.Name);
			else
				Logger.Log("WARNING: PurchaseProduct: item is null");

			var retrievedProduct = retrievedProducts.FirstOrDefault(p => p.ProductIdentifier == product.ProductIdentifier);
			if (retrievedProduct != null)
			{
				Logger.Log("PurchaseProduct: retrievedProduct ID: " + retrievedProduct.ProductIdentifier);
				inAppPurchaseManager.PurchaseProduct(retrievedProduct);
			}
			else
				Logger.Log("WARNING: PurchaseProduct: retrievedProduct is null");
		}

		private void Fire_ProductsRetrieved(List<Product> products)
		{
			var productsRetrieved = OnProductsRetrieved;
			if (productsRetrieved != null)
				Device.BeginInvokeOnMainThread(() => productsRetrieved(this, products));
		}

		private void Fire_OnProductPurchased(string productIdentifier)
		{
			var productPurchased = OnProductPurchased;
			if (productPurchased != null)
				Device.BeginInvokeOnMainThread(() => productPurchased(this, productIdentifier));
		}

		private void Fire_OnTransactionFailed(string localizedDescription)
		{
			var transactionFailed = OnTransactionFailed;
			if (transactionFailed != null)
				Device.BeginInvokeOnMainThread(() => transactionFailed(this, localizedDescription));
		}

		private void Fire_OnRequestFailed(string localizedDescription)
		{
			var requestFailed = OnRequestFailed;
			if (requestFailed != null)
				Device.BeginInvokeOnMainThread(() => requestFailed(this, localizedDescription));
		}

		private void Fire_OnValidationFailed(string error)
		{
			var validationFailed = OnValidationFailed;
			if (validationFailed != null)
				Device.BeginInvokeOnMainThread(() => validationFailed(this, error));
		}

		private SKPaymentTransaction getTransactionFromNotification(NSNotification notification)
		{
			var info = notification.UserInfo;
			if (info == null)
				return null;

			var transactionKey = new NSString("transaction");

			NSObject transactionObject;
			if (info.TryGetValue(transactionKey, out transactionObject))
				return transactionObject as SKPaymentTransaction;

			return null;
		}
	}
}
