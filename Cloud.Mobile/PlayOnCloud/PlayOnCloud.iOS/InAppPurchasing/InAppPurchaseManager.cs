using Foundation;
using StoreKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace PlayOnCloud.iOS
{
	public class InAppPurchaseManager : SKProductsRequestDelegate
	{
		public static NSString InAppPurchaseManagerProductsFetchedNotification = new NSString("InAppPurchaseManagerProductsFetchedNotification");
		public static NSString InAppPurchaseManagerTransactionFailedNotification = new NSString("InAppPurchaseManagerTransactionFailedNotification");
		public static NSString InAppPurchaseManagerTransactionSucceededNotification = new NSString("InAppPurchaseManagerTransactionSucceededNotification");
		public static NSString InAppPurchaseManagerRequestFailedNotification = new NSString("InAppPurchaseManagerRequestFailedNotification");
		public static NSString InAppPurchaseManagerValidationFailedNotification = new NSString("InAppPurchaseManagerValidationFailedNotification");

		private SKProductsRequest productsRequest;
		private CustomPaymentObserver theObserver;

		public InAppPurchaseManager()
		{
			try
			{
				theObserver = new CustomPaymentObserver(this);

				Logger.Log("Adding Transaction Observer");
				// Call this once upon startup of in-app-purchase activities
				// This also kicks off the TransactionObserver which handles the various communications
				SKPaymentQueue.DefaultQueue.AddTransactionObserver(theObserver);
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: InAppPurchaseManager constructor: " + ex);
			}
		}

		// Verify that the iTunes account can make this purchase for this application
		public bool CanMakePayments()
		{
			return SKPaymentQueue.CanMakePayments;
		}

		// request multiple products at once
		public void RequestProductData(List<string> productIds)
		{
			try
			{
				Logger.Log("RequestProductData...");
				var array = new NSString[productIds.Count];
				for (var i = 0; i < productIds.Count; i++)
				{
					array[i] = new NSString(productIds[i]);
					Logger.Log("RequestProductData: Product ID: " + productIds[i]);
				}

				NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(array);

				//set up product request for in-app purchase
				productsRequest = new SKProductsRequest(productIdentifiers);
				productsRequest.Delegate = this;
				productsRequest.Start();
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: RequestProductData: " + ex);
			}
		}

		public override void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
		{
			try
			{
				Logger.Log("ReceivedResponse...");
				SKProduct[] products = response.Products;

				NSDictionary userInfo = null;
				if (products.Length > 0)
				{
					NSObject[] productIdsArray = new NSObject[response.Products.Length];
					NSObject[] productsArray = new NSObject[response.Products.Length];
					for (int i = 0; i < response.Products.Length; i++)
					{
						productIdsArray[i] = new NSString(response.Products[i].ProductIdentifier);
						productsArray[i] = response.Products[i];
						Logger.Log("ReceivedResponse: Product ID: " + response.Products[i].ProductIdentifier);
					}

					userInfo = NSDictionary.FromObjectsAndKeys(productsArray, productIdsArray);
				}

				NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerProductsFetchedNotification, this, userInfo);
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: ReceivedResponse: " + ex);
			}
		}

		public void PurchaseProduct(SKProduct product)
		{
			try
			{
				if (Reachability.InternetConnectionStatus() == NetworkStatus.NotReachable)
				{
					Logger.Log("ERROR: PurchaseProduct: No open internet connection is available.");
					using (var alert = new UIAlertView("ERROR", "No open internet connection is available.", null, "OK", null))
						alert.Show();

					return;
				}

				if (!CanMakePayments())
				{
					Logger.Log("ERROR: PurchaseProduct: Cannot make payments");
					using (var alert = new UIAlertView("ERROR", "Sorry but you cannot make purchases from the In App Billing store. Please try again later.", null, "OK", null))
						alert.Show();

					return;
				}

				if (product == null)
				{
					Logger.Log("ERROR: PurchaseProduct: Product is null");
					return;
				}

				Logger.Log("PurchaseProduct: Product ID: " + product.ProductIdentifier);

				SKPayment payment = SKPayment.CreateFrom(product);
				SKPaymentQueue.DefaultQueue.AddPayment(payment);
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: PurchaseProduct: " + ex);
			}
		}

		public async Task CompleteTransaction(SKPaymentTransaction transaction)
		{
			try
			{
				Logger.Log("CompleteTransaction thread: " + System.Threading.Thread.CurrentThread.Name + " ID:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
				string logMessage = "CompleteTransaction: ";
				if ((transaction != null) && (transaction.TransactionReceipt != null))
					logMessage += transaction.TransactionReceipt.ToString();

				Logger.Log(logMessage);
				var verifyResult = await InAppPurchaseVerificationController.Instance.VerifyPurchase(transaction);
				if (verifyResult.Success)
					FinishTransaction(transaction, true);
				else
				{
					SKReceiptRefreshRequest refreshRequest = new SKReceiptRefreshRequest();
					refreshRequest.Delegate = this;
					refreshRequest.Start();

					using (var pool = new NSAutoreleasePool())
					{
						string error = verifyResult.Error;
						if (error == null)
							error = string.Empty;

						NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] { new NSString(error) }, new NSObject[] { new NSString("error") });
						NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerValidationFailedNotification, this, userInfo);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: CompleteTransaction: " + ex);
			}
		}

		public void FailedTransaction(SKPaymentTransaction transaction)
		{
			try
			{
				if (transaction.Error.Code == 2)
					// user cancelled
					Logger.Log("User CANCELLED FailedTransaction Code=" + transaction.Error.Code + " " + transaction.Error.LocalizedDescription);
				else
					// error!
					Logger.Log("FailedTransaction Code=" + transaction.Error.Code + " " + transaction.Error.LocalizedDescription);

				FinishTransaction(transaction, false);
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: FailedTransaction: " + ex);
			}
		}

		public void FinishTransaction(SKPaymentTransaction transaction, bool wasSuccessful)
		{
			try
			{
				Logger.Log("FinishTransaction thread: " + System.Threading.Thread.CurrentThread.Name + " ID:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
				Logger.Log("Finishing Transaction...");
				// remove the transaction from the payment queue. THIS IS IMPORTANT - LET'S APPLE KNOW WE'RE DONE !!!!
				SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
				Logger.Log("Transaction Finished!");

				using (var pool = new NSAutoreleasePool())
				{
					NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] { transaction }, new NSObject[] { new NSString("transaction") });
					if (wasSuccessful)
						NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerTransactionSucceededNotification, this, userInfo);
					else
						NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerTransactionFailedNotification, this, userInfo);
				}
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: FinishTransaction: " + ex);
			}
		}

		/// <summary>
		/// Probably could not connect to the App Store (network unavailable?)
		/// </summary>
		public override void RequestFailed(SKRequest request, NSError error)
		{
			try
			{
				using (var pool = new NSAutoreleasePool())
				{
					NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(new NSObject[] { error }, new NSObject[] { new NSString("error") });
					NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerRequestFailedNotification, this, userInfo);
				}
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: RequestFailed: " + ex);
			}
		}
	}
}
