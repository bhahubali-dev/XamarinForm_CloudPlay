using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Foundation;
using UIKit;
using PlayOnCloud.iOS;
using PlayOnCloud.Model;

[assembly: Dependency(typeof(ItemDownloader))]
namespace PlayOnCloud.iOS
{
	public class ItemDownloader : IItemDownloader
	{
		public static string DownloaderPath
		{
			get
			{
				var documents = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User);
				return documents.First().Path;
			}
		}

		private NSUrlSession session;
		private const string sessionID = "com.PlayOnCloud.BackgroundLibraryDownloadSession";
		private object syncRoot = new object();

		public event EventHandler<AsyncCompletedEventArgs> DownloadComplete;

		public event EventHandler<DownloadProgress> DownloadProgress;

		public ItemDownloader()
		{
			CreateSession(sessionID);
		}

		public void CreateSession(string sessionID)
		{
			lock (syncRoot)
				if (session == null)
				{
					using (var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(sessionID))
					{
						configuration.HttpShouldSetCookies = true;
						configuration.HttpCookieAcceptPolicy = NSHttpCookieAcceptPolicy.Always;
						configuration.HttpCookieStorage.AcceptPolicy = NSHttpCookieAcceptPolicy.Always;
						configuration.AllowsCellularAccess = false;
						configuration.HttpMaximumConnectionsPerHost = 1;
						configuration.SessionSendsLaunchEvents = true;
						configuration.TimeoutIntervalForRequest = 0;
						session = NSUrlSession.FromConfiguration(
							configuration,
							new DownloadSessionDelegate(this),
							new NSOperationQueue() { MaxConcurrentOperationCount = 1 });
					}

					Logger.Log("INFO: ItemDownloader session created");
				}
		}

		public void ClearSession()
		{
			lock (syncRoot)
				session = null;
		}

		public string GetDownloaderPath()
		{
			return DownloaderPath;
		}

		private async Task<bool> canStoreOnDevice(DownloadItem downloadItem)
		{
			var canStore = true;

			try
			{
				var freeSize = NSFileManager.DefaultManager.GetFileSystemAttributes(Environment.GetFolderPath(Environment.SpecialFolder.Personal)).FreeSize;
				Logger.Log("Available size on device in bytes: " + freeSize);

				if (freeSize > 0)
				{
					var cookieContainer = new CookieContainer();

					using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
					using (var client = new HttpClient(handler) { BaseAddress = new Uri(downloadItem.Url) })
					{
						foreach (var header in downloadItem.Headers)
							cookieContainer.Add(new Uri(downloadItem.Url), new Cookie(header.Key, header.Value));

						var request = new HttpRequestMessage(HttpMethod.Head, downloadItem.Url);
						var resp = await client.SendAsync(request);
						if ((resp != null) && resp.IsSuccessStatusCode && (resp.Content?.Headers?.ContentLength != null))
							canStore = (freeSize > (ulong)resp.Content.Headers.ContentLength);
					}
				}
				else
					canStore = false;
			}
			catch (Exception ex)
			{
				Logger.Log("There was an error checking available storage: " + ex);
			}

			return canStore;
		}

		public async Task<Tuple<bool, string>> Download(DownloadItem item)
		{
			Logger.Log("Adding item for downloading: ID: " + item.Id);
			var downloadItem = await LibraryClient.GetDownload(item.Id);
			Logger.Log("Item download requested from server");

			if (downloadItem != null)
			{
				Logger.Log("Checking available storage");
				var canStore = await canStoreOnDevice(downloadItem);
				Logger.Log("Available storage checked");

				if (!canStore)
					return Tuple.Create(false, downloadItem.Url);

				item.Url = downloadItem.Url;
				item.Headers = downloadItem.Headers;
				using (var url = NSUrl.FromString(item.Url))
				using (var request = new NSMutableUrlRequest(url))
				{
					List<NSHttpCookie> cookies = new List<NSHttpCookie>();
					foreach (var header in item.Headers)
						cookies.Add(new NSHttpCookie(header.Key, header.Value));

					request.Headers = NSHttpCookie.RequestHeaderFieldsWithCookies(cookies.ToArray());
					DownloadSessionMetadata.SaveDownloadItem(item);
					if ((RestService.Instance.User != null) && !string.IsNullOrEmpty(RestService.Instance.User.Email))
						LocalLibraryService.Instance.CreateCloudItemId(item.Id, RestService.Instance.User.Email);

					CreateSession(sessionID);
					lock (syncRoot)
					{
						if (session != null)
						{
							var task = session.CreateDownloadTask(request);
							task.Resume();
						}
						else
							Logger.Log("ERROR: Failed to start downloading because session is NULL");
					}

					Logger.Log("Item added to the download queue: ID: " + item.Id + " FileName: " + item.FileName + " URL:" + item.Url);

					return Tuple.Create(true, item.Url);
				}
			}
			else
				Logger.Log("ERROR: Failed to start downloading of item: ID: " + item.Id + ". FileName: " + item.FileName);

			return Tuple.Create(true, string.Empty);
		}

		public void Cancel(string id)
		{
			Logger.Log("Canceling download item: ID: " + id);
			lock (syncRoot)
				if (session != null)
					session.GetAllTasks((NSUrlSessionTask[] tasks) =>
					{
						var runningTasks = tasks.Where(t => t.State == NSUrlSessionTaskState.Running).ToArray();
						foreach (var runningTask in runningTasks)
						{
							var downloadItem = DownloadSessionMetadata.GetDownloadItem(runningTask.OriginalRequest.Url.ToString());
							if ((downloadItem != null) && (downloadItem.Id == id))
							{
								runningTask.Cancel();
								Logger.Log("Download item " + id + " canceled");
							}
						}
					});
		}

		public void Fire_DownloadProgress(DownloadProgress progress)
		{
			DownloadProgress?.Invoke(this, progress);
		}

		public void Fire_DownloadComplete(AsyncCompletedEventArgs asyncCompletedEventArgs)
		{
			DownloadComplete?.Invoke(this, asyncCompletedEventArgs);
		}

		public bool CanDownloadItems(out string warningMessage, out string title)
		{
			warningMessage = string.Empty;
			title = string.Empty;

			if (Reachability.InternetConnectionStatus() != NetworkStatus.ReachableViaWiFiNetwork)
			{
				Logger.Log("CanDownloadItems: Wifi Required");
				title = "Wifi Required";
				warningMessage = "Your download will start once you are connected to a Wifi network.";

				return false;
			}

			return true;
		}
	}

	public class DownloadSessionDelegate : NSUrlSessionDownloadDelegate
	{
		private ItemDownloader itemDownloader;
		private DateTime lastProgressUpdate = DateTime.UtcNow;

		public DownloadSessionDelegate(ItemDownloader itemDownloader)
		{
			this.itemDownloader = itemDownloader;
		}

		public override void DidWriteData(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
		{
			try
			{
				if ((downloadTask == null) || (downloadTask.OriginalRequest == null) || (downloadTask.OriginalRequest.Url == null))
					return;

				if (DateTime.UtcNow.Subtract(lastProgressUpdate).TotalSeconds < 1)
					return;

				DownloadProgress progress = new DownloadProgress()
				{
					Status = DownloadStatus.Downloading,
					TotalLength = totalBytesExpectedToWrite,
					DownloadedLength = totalBytesWritten,
					Url = downloadTask.OriginalRequest.Url.ToString(),
				};

				calculateTimeRemaining(progress);
				lastProgressUpdate = DateTime.UtcNow;

				if (itemDownloader != null)
					InvokeOnMainThread(() => itemDownloader.Fire_DownloadProgress(progress));
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: DidWriteData: " + ex);
			}
		}

		public override void DidFinishDownloading(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
		{
			try
			{
				Logger.Log("INFO: DidFinishDownloading: " + downloadTask.OriginalRequest.Url.ToString());
				string itemID = string.Empty;
				string destinationPath = ItemDownloader.DownloaderPath + Path.DirectorySeparatorChar + downloadTask.OriginalRequest.Url.LastPathComponent;
				string fileName = downloadTask.Response.SuggestedFilename;
				string title = downloadTask.Response.SuggestedFilename;

				var downloadItem = DownloadSessionMetadata.GetDownloadItem(downloadTask.OriginalRequest.Url.ToString());
				if ((downloadItem != null) && (downloadItem.FileName != null))
				{
					if (string.IsNullOrEmpty(fileName))
						fileName = downloadItem.FileName;

					itemID = downloadItem.Id;
					if (!string.IsNullOrEmpty(downloadItem.FileName))
						title = downloadItem.FileName.Replace(".mp4", string.Empty);

					DownloadSessionMetadata.RemoveDownloadItem(downloadTask.OriginalRequest.Url.ToString());
				}

				string filter = "\\/:*?\"<>|#";
				string filteredFileName = new string(fileName.Where(c => filter.IndexOf(c) < 0).ToArray());
				destinationPath = ItemDownloader.DownloaderPath + Path.DirectorySeparatorChar + filteredFileName;

				NSError error = null;
				Exception exception = null;

				if (NSFileManager.DefaultManager.FileExists(destinationPath))
					NSFileManager.DefaultManager.Remove(destinationPath, out error);

				bool isCustomError = false;
				var success = NSFileManager.DefaultManager.Move(location, NSUrl.FromFilename(destinationPath), out error);
				if (!success)
					exception = new Exception(error.LocalizedDescription);
				else
				{
					NSFileManager.SetSkipBackupAttribute(destinationPath, true);
					if (!LocalLibraryService.Instance.CanReadRecordingMetadata(destinationPath))
					{
						isCustomError = true;
						Logger.Log("ERROR: DidFinishDownloading: Unable to read the file metadata");
						exception = new Exception("We are sorry, but there is a technical issue preventing PlayOn Cloud from downloading or playing your recording. Please report this issue to our support team through this app's Account tab.");
					}
				}

				DownloadProgress progress = new DownloadProgress()
				{
					Status = success ? DownloadStatus.Completed : DownloadStatus.Failed,
					Id = itemID,
					Title = title,
					LocalFilePath = destinationPath,
					Url = downloadTask.OriginalRequest.Url.ToString(),
					IsCustomError = isCustomError
				};

				Logger.Log("INFO: DidFinishDownloading: ID: " + progress.Id + ". Exception: " + exception);
				if (itemDownloader != null)
					InvokeOnMainThread(() => itemDownloader.Fire_DownloadComplete(new AsyncCompletedEventArgs(exception, false, progress)));
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: DidFinishDownloading: " + ex);
			}
		}

		public override void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
		{
			try
			{
				Logger.Log("INFO: DidCompleteWithError: " + error);
				if (error != null)
				{
					if (task.OriginalRequest == null)
						return;

					string itemID = task.OriginalRequest.Url.ToString();
					string title = task.OriginalRequest.Url.ToString();
					var downloadItem = DownloadSessionMetadata.GetDownloadItem(task.OriginalRequest.Url.ToString());
					if (downloadItem != null)
					{
						itemID = downloadItem.Id;
						if (!string.IsNullOrEmpty(downloadItem.FileName))
							title = downloadItem.FileName.Replace(".mp4", string.Empty);

						DownloadSessionMetadata.RemoveDownloadItem(task.OriginalRequest.Url.ToString());
					}

					var isCanceled = (error.Code == -999);
					DownloadProgress progress = new DownloadProgress()
					{
						Status = isCanceled ? DownloadStatus.Canceled : DownloadStatus.Failed,
						Id = itemID,
						Title = title,
						Url = task.OriginalRequest.Url.ToString()
					};

					if (!isCanceled && (downloadItem != null) && (downloadItem.RetryCount <= 3))
						progress.WillRetryOnError = true;

					Logger.Log("INFO: DidCompleteWithError: ID: " + progress.Id + ". Error: " + error);
					if (itemDownloader != null)
						InvokeOnMainThread(() => itemDownloader.Fire_DownloadComplete(new AsyncCompletedEventArgs(new NSErrorException(error), isCanceled, progress)));

					if (!isCanceled && (downloadItem != null) && (downloadItem.RetryCount <= 3))
					{
						downloadItem.RetryCount++;
#pragma warning disable 4014
						itemDownloader.Download(downloadItem);
#pragma warning restore 4014
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: DidCompleteWithError: " + ex);
			}
		}

		public override void DidBecomeInvalid(NSUrlSession session, NSError error)
		{
			Logger.Log("INFO: Session DidBecomeInvalid: " + session?.Configuration?.Identifier);
		}

		public override void DidFinishEventsForBackgroundSession(NSUrlSession session)
		{
			Logger.Log("INFO: DidFinishEventsForBackgroundSession: sessionIdentifier: " + session.Configuration.Identifier);
			var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
			session.FinishTasksAndInvalidate();
			itemDownloader.ClearSession();
			appDelegate.FireBackgroundSessionCompletionHandler();
		}

		private void calculateTimeRemaining(DownloadProgress progress)
		{
			DownloadItem item = DownloadSessionMetadata.GetDownloadItem(progress.Url.ToString());

			if (item == null)
				return;

			if (!item.TimeStarted.HasValue)
				item.TimeStarted = DateTime.UtcNow;

			try
			{
				var timeElapsed = DateTime.UtcNow.Subtract(item.TimeStarted.Value);
				var lastSpeed = progress.DownloadedLength / timeElapsed.TotalSeconds;
				progress.ExpectedDuration = (long)((progress.TotalLength / lastSpeed) * 1000);
				progress.DownloadedDuration = (long)timeElapsed.TotalMilliseconds;
				progress.Percent = (double)progress.DownloadedLength / progress.TotalLength;
				progress.Id = item.Id;

				DownloadSessionMetadata.SaveDownloadItem(item);
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: calculateTimeRemaining: " + ex);
			}
		}
	}

	public static class DownloadSessionMetadata
	{
		public static void SaveDownloadItem(DownloadItem item)
		{
			NSDictionary dictionary;
			if (item.TimeStarted.HasValue)
			{
				NSDate dateVal = (NSDate)DateTime.SpecifyKind(item.TimeStarted.Value, DateTimeKind.Utc);
				dictionary = NSDictionary.FromObjectsAndKeys(
					new object[] { item.Id, item.Url, item.FileName, dateVal, item.RetryCount },
					new object[] { "Id", "Url", "FileName", "TimeStarted", "RetryCount" });
			}
			else
				dictionary = NSDictionary.FromObjectsAndKeys(
					new object[] { item.Id, item.Url, item.FileName, item.RetryCount },
					new object[] { "Id", "Url", "FileName", "RetryCount" });

			NSUserDefaults.StandardUserDefaults.SetValueForKey(dictionary, new NSString(item.Url));
			NSUserDefaults.StandardUserDefaults.Synchronize();
		}

		public static DownloadItem GetDownloadItem(string url)
		{
			NSDictionary dictionary = NSUserDefaults.StandardUserDefaults.DictionaryForKey(new NSString(url));
			if (dictionary != null)
			{
				var result = new DownloadItem()
				{
					Id = dictionary["Id"] != null ? dictionary["Id"].ToString() : null,
					Url = dictionary["Url"] != null ? dictionary["Url"].ToString() : null,
					FileName = dictionary["FileName"] != null ? dictionary["FileName"].ToString() : null,
					RetryCount = dictionary["RetryCount"] != null ? Convert.ToInt32(dictionary["RetryCount"].ToString()) : 0
				};

				var date = dictionary["TimeStarted"] as NSDate;
				if (date != null)
					result.TimeStarted = (DateTime)date;

				return result;
			}

			return null;
		}

		public static void RemoveDownloadItem(string url)
		{
			NSDictionary dictionary = NSUserDefaults.StandardUserDefaults.DictionaryForKey(new NSString(url));
			if (dictionary != null)
			{
				NSUserDefaults.StandardUserDefaults.RemoveObject(new NSString(url));
				NSUserDefaults.StandardUserDefaults.Synchronize();
			}
		}
	}
}
