using System;
using System.Collections.Generic;
using Xamarin.Forms;
using PlayOnCloud.Droid;
using PlayOnCloud.Model;

[assembly: Dependency(typeof(InAppPurchase))]
namespace PlayOnCloud.Droid
{
	class InAppPurchase : IInAppPurchase
	{
		public event EventHandler<string> OnProductPurchased;
		public event EventHandler<List<Product>> OnProductsRetrieved;
		public event EventHandler<string> OnRequestFailed;
		public event EventHandler<string> OnTransactionFailed;
		public event EventHandler<string> OnValidationFailed;

		public void PurchaseProduct(Product product, ContentItemDetails item)
		{
			//throw new NotImplementedException();
		}

		public void RequestProductData(List<string> productIDs)
		{
			//throw new NotImplementedException();
		}

		public bool CanMakePayments
		{
			get
			{
				//throw new NotImplementedException();
				return false;
			}
		}
	}
}