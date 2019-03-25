using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PCLStorage;
using PlayOnCloud.Droid;
using PlayOnCloud.Model;
using Xamarin.Forms;
using FileAccess = PCLStorage.FileAccess;

[assembly: Dependency(typeof(ItemDownloader))]

namespace PlayOnCloud.Droid
{
    internal class ItemDownloader : IItemDownloader
    {
        private const string SessionId = "com.PlayOnCloud.BackgroundLibraryDownloadSession";

        public static readonly string ImageToDownload =
            "http://eoimages.gsfc.nasa.gov/images/imagerecords/74000/74393/world.topo.200407.3x5400x2700.jpg";

        public static readonly int BufferSize = 4096;
        private object _syncRoot = new object();

        public static string DownloaderPath
        {
            get
            {
                // string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                //string fileName = Android.Net.Uri.Parse(file.Url).Path.Split('/').Last();
                //return Path.Combine(ApplicationContext.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).AbsolutePath, fileName);
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var filename = Path.Combine(path, "testfile.txt");
                return path;
            }
        }

        public event EventHandler<AsyncCompletedEventArgs> DownloadComplete;
        public event EventHandler<DownloadProgress> DownloadProgress;

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
                downloadItem.FileName = item.FileName;

                await DownloadDb(downloadItem);
                return Tuple.Create(true, item.Url);
            }
            Logger.Log("ERROR: Failed to start downloading of item: ID: " + item.Id + ". FileName: " + item.FileName);


            return Tuple.Create(true, string.Empty);
        }

        public void Cancel(string id)
        {
            throw new NotImplementedException();
        }

        public string GetDownloaderPath()
        {
            return DownloaderPath;
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

        public void CreateSession(string sessionID)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> canStoreOnDevice(DownloadItem downloadItem)
        {
            var canStore = true;

            try
            {
                var freeSize =
                    (ulong)
                    Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim)
                        .FreeSpace;
                //  ulong freeSize = 200008434535455;

                Logger.Log("Available size on device in bytes: " + freeSize);

                if (freeSize > 0)
                {
                    var cookieContainer = new CookieContainer();

                    using (var handler = new HttpClientHandler {CookieContainer = cookieContainer})
                    using (var client = new HttpClient(handler) {BaseAddress = new Uri(downloadItem.Url)})
                    {
                        foreach (var header in downloadItem.Headers)
                            cookieContainer.Add(new Uri(downloadItem.Url), new Cookie(header.Key, header.Value));

                        var request = new HttpRequestMessage(HttpMethod.Head, downloadItem.Url);
                        var resp = await client.SendAsync(request);
                        if (resp != null && resp.IsSuccessStatusCode && resp.Content?.Headers?.ContentLength != null)
                            canStore = freeSize > (ulong) resp.Content.Headers.ContentLength;
                    }
                }
                else
                {
                    canStore = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("There was an error checking available storage: " + ex);
            }

            return canStore;
        }


        public static async Task DownloadDb(DownloadItem downloadItem)
        {
            try
            {
                var ext = Android.OS.Environment.ExternalStorageDirectory.Path + "/Downloads";
                var rootfolder = FileSystem.Current.LocalStorage;
                var appfolder = await rootfolder.CreateFolderAsync(ext, CreationCollisionOption.OpenIfExists);
                var dbfolder = await appfolder.CreateFolderAsync("Db", CreationCollisionOption.OpenIfExists);
                var file = await dbfolder.CreateFileAsync(downloadItem.FileName, CreationCollisionOption.ReplaceExisting);
                using (var fileHandler = await file.OpenAsync(FileAccess.ReadAndWrite))
                {
                    var cookieContainer = new CookieContainer();

                    using (var handler = new HttpClientHandler {CookieContainer = cookieContainer})
                    using (var client = new HttpClient(handler) {BaseAddress = new Uri(downloadItem.Url)})
                    {
                        foreach (var header in downloadItem.Headers)
                            cookieContainer.Add(new Uri(downloadItem.Url), new Cookie(header.Key, header.Value));

                        var request = new HttpRequestMessage(HttpMethod.Head, downloadItem.Url);

                        var resp = await client.SendAsync(request);
                        if (resp != null && resp.IsSuccessStatusCode && resp.Content?.Headers?.ContentLength != null)
                        {
                            var dataBuffer = await resp.Content.ReadAsByteArrayAsync();
                            var dataBuffer1 = await resp.Content.ReadAsStreamAsync();
                            var dataBuffer2 = resp.Content.LoadIntoBufferAsync();
                            await fileHandler.WriteAsync(dataBuffer, 0, dataBuffer.Length);
                        }


                        //var resp2 = await client.GetAsync(request.RequestUri.AbsolutePath);
                        //if (resp2.IsSuccessStatusCode)
                        //{
                        //    var content = await resp2.Content.ReadAsByteArrayAsync();
                        //    await fileHandler.WriteAsync(content, 0, content.Length);
                        //}
                    }
                    //var _progressBar = new ProgressBar();
                    //_progressBar.Progress = 0;
                    //var progressReporter = new Progress<DownloadBytesProgress>();
                    //progressReporter.ProgressChanged +=
                    //    (s, args) => _progressBar.Progress = (int) (100 * args.PercentComplete);

                    //var downloadTask = CreateDownloadTask(ImageToDownload,
                    //    progressReporter);
                    //var bytesDownloaded = await downloadTask;
                    //Debug.WriteLine("Downloaded {0} bytes.", bytesDownloaded);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<int> CreateDownloadTask(string urlToDownload,
            IProgress<DownloadBytesProgress> progessReporter)
        {
            var receivedBytes = 0;
            var totalBytes = 0;
            var client = new WebClient();

            using (var stream = await client.OpenReadTaskAsync(urlToDownload))
            {
                var buffer = new byte[4096];
                totalBytes = int.Parse(client.ResponseHeaders[HttpResponseHeader.ContentLength]);

                for (;;)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        await Task.Yield();
                        break;
                    }

                    receivedBytes += bytesRead;
                    if (progessReporter != null)
                    {
                        var args = new DownloadBytesProgress(urlToDownload, receivedBytes, totalBytes);
                        progessReporter.Report(args);
                    }
                }
            }
            return receivedBytes;
        }

        //        progress.Id = item.Id;
        //        progress.Percent = (double) progress.DownloadedLength / progress.TotalLength;

        //private void calculateTimeRemaining(DownloadProgress progress)
        //{
        //    DownloadItem item = DownloadSessionMetadata.GetDownloadItem(progress.Url);

        //    if (item == null)
        //        return;

        //    if (!item.TimeStarted.HasValue)
        //        item.TimeStarted = DateTime.UtcNow;

        //    try
        //    {
        //        var timeElapsed = DateTime.UtcNow.Subtract(item.TimeStarted.Value);
        //        var lastSpeed = progress.DownloadedLength / timeElapsed.TotalSeconds;

        //        progress.ExpectedDuration = (long) (progress.TotalLength / lastSpeed * 1000);

        //        progress.DownloadedDuration = (long) timeElapsed.TotalMilliseconds;

        //        //DownloadSessionMetadata.SaveDownloadItem(item);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log("ERROR: calculateTimeRemaining: " + ex);
        //    }
        //}
    }

    //public static class DownloadSessionMetadata
    //{
    //    public static void SaveDownloadItem(DownloadItem item)
    //    {
    //        NSDictionary dictionary;
    //        if (item.TimeStarted.HasValue)
    //        {
    //            NSDate dateVal = (NSDate) DateTime.SpecifyKind(item.TimeStarted.Value, DateTimeKind.Utc);
    //            dictionary = NSDictionary.FromObjectsAndKeys(
    //                new object[] {item.Id, item.Url, item.FileName, dateVal, item.RetryCount},
    //                new object[] {"Id", "Url", "FileName", "TimeStarted", "RetryCount"});
    //        }
    //        else
    //        {
    //            dictionary = NSDictionary.FromObjectsAndKeys(
    //                new object[] {item.Id, item.Url, item.FileName, item.RetryCount},
    //                new object[] {"Id", "Url", "FileName", "RetryCount"});
    //        }

    //        NSUserDefaults.StandardUserDefaults.SetValueForKey(dictionary, new NSString(item.Url));
    //        NSUserDefaults.StandardUserDefaults.Synchronize();
    //    }

    //    public static DownloadItem GetDownloadItem(string url)
    //    {
    //        NSDictionary dictionary = NSUserDefaults.StandardUserDefaults.DictionaryForKey(new NSString(url));
    //        if (dictionary != null)
    //        {
    //            var result = new DownloadItem
    //            {
    //                Id = dictionary["Id"] != null ? dictionary["Id"].ToString() : null,
    //                Url = dictionary["Url"] != null ? dictionary["Url"].ToString() : null,
    //                FileName = dictionary["FileName"] != null ? dictionary["FileName"].ToString() : null,
    //                RetryCount =
    //                    dictionary["RetryCount"] != null ? Convert.ToInt32(dictionary["RetryCount"].ToString()) : 0
    //            };

    //            var date = dictionary["TimeStarted"] as NSDate;
    //            if (date != null)
    //                result.TimeStarted = (DateTime) date;

    //            return result;
    //        }

    //        return null;
    //    }

    //    public static void RemoveDownloadItem(string url)
    //    {
    //        NSDictionary dictionary = NSUserDefaults.StandardUserDefaults.DictionaryForKey(new NSString(url));
    //        if (dictionary != null)
    //        {
    //            NSUserDefaults.StandardUserDefaults.RemoveObject(new NSString(url));
    //            NSUserDefaults.StandardUserDefaults.Synchronize();
    //        }
    //    }
    //}

    public class DownloadBytesProgress
    {
        public DownloadBytesProgress(string fileName, int bytesReceived, int totalBytes)
        {
            Filename = fileName;
            BytesReceived = bytesReceived;
            TotalBytes = totalBytes;
        }

        public int TotalBytes { get; }

        public int BytesReceived { get; }

        public float PercentComplete
        {
            get { return (float) BytesReceived / TotalBytes; }
        }

        public string Filename { get; private set; }

        public bool IsFinished
        {
            get { return BytesReceived == TotalBytes; }
        }
    }
}