using StoreKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PlayOnCloud.iOS
{
	public class InAppPurchaseVerificationController
	{
		public class VerifyPurchaseResult
		{
			public bool Success { get; set; }

			public string Error { get; set; }
		}

		private static volatile InAppPurchaseVerificationController instance;
		private static object syncRoot = new object();

		public static InAppPurchaseVerificationController Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = new InAppPurchaseVerificationController();

				return instance;
			}
		}

		public async Task<VerifyPurchaseResult> VerifyPurchase(SKPaymentTransaction transaction)
		{
			bool success;
			string error;

			try
			{
				Logger.Log("VerifyPurchase ...");
				var encodedReceipt = EncodeBase64(transaction.TransactionReceipt.ToString());
				var response = await ReceiptClient.Add(encodedReceipt);
				if (response != null)
				{
					if (response.Success)
					{
						Logger.Log("Verification successful");
						error = null;
						success = true;
					}
					else
					{
						Logger.Log("Verification failed: " + response.ErrorCode + ": " + response.ErrorMessageClean);
						error = response.ErrorMessageClean;
						success = false;
					}
				}
				else
				{
					Logger.Log("Verification failed: response is null");
					error = string.Empty;
					success = false;
				}
			}
			catch (Exception e)
			{
				Logger.Log("Verification Exception: " + e.ToString());
				error = e.ToString();
				success = false;
			}

			return new VerifyPurchaseResult() { Success = success, Error = error };
		}

		public string EncodeBase64(string toEncode)
		{
			byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
			string returnValue = Convert.ToBase64String(toEncodeAsBytes);
			return returnValue;
		}
	}
}
