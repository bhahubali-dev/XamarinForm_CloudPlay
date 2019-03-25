using StoreKit;
using System;

namespace PlayOnCloud.iOS
{
	internal class CustomPaymentObserver : SKPaymentTransactionObserver
	{
		private InAppPurchaseManager theManager;

		public CustomPaymentObserver(InAppPurchaseManager manager)
		{
			theManager = manager;
		}

		// called when the transaction status is updated
		public override async void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
		{
			try
			{
				Logger.Log("UpdatedTransactions thread: " + System.Threading.Thread.CurrentThread.Name + " ID:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
				foreach (SKPaymentTransaction transaction in transactions)
				{
					string logMessage = "UpdatedTransaction: State: " + transaction.TransactionState.ToString();
					if (transaction.TransactionReceipt != null)
						logMessage += ". Transaction: " + transaction.TransactionReceipt.ToString();

					Logger.Log(logMessage);

					string identifier = transaction.TransactionIdentifier;
					if (!string.IsNullOrEmpty(identifier))
						Logger.Log("UpdatedTransaction: Transaction Identifier: " + identifier);

					switch (transaction.TransactionState)
					{
						case SKPaymentTransactionState.Purchased:
							await theManager.CompleteTransaction(transaction);
							break;

						case SKPaymentTransactionState.Failed:
							theManager.FailedTransaction(transaction);
							break;

						default:
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: UpdatedTransactions: " + ex);
			}
		}

		public override void RestoreCompletedTransactionsFinished(SKPaymentQueue queue)
		{
			Logger.Log("Consumable product purchases do not get Restored");
			throw new NotImplementedException("Consumable product purchases do not get Restored");
		}
	}
}
