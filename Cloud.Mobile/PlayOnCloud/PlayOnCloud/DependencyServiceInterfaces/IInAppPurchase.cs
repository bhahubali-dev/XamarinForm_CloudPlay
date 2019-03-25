using System;
using System.Collections.Generic;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	public interface IInAppPurchase
	{
		bool CanMakePayments { get; }

		void RequestProductData(List<string> productIDs);

		void PurchaseProduct(Product product, ContentItemDetails item);

		event EventHandler<List<Product>> OnProductsRetrieved;

		event EventHandler<string> OnProductPurchased;

		event EventHandler<string> OnTransactionFailed;

		event EventHandler<string> OnRequestFailed;

		event EventHandler<string> OnValidationFailed;
	}
}
