using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using PlayOnCloud.Droid;
using PlayOnCloud.Model;
using SQLite;
using System.IO;
using Android.Content.Res;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Android.Media;
using Java.Lang;

[assembly: Dependency(typeof(LocalLibrary))]
namespace PlayOnCloud.Droid
{
	class LocalLibrary : ILocalLibrary
	{
       
        private static object databaseSync = new object();


        public static string DatabasePath
        {

            get
            {
                string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                return Path.Combine(path, "playoncloud.db");
            }
        }

        public static string LinksPath
        {
            get
            {
                string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                return Path.Combine(path, Path.DirectorySeparatorChar + "links");

            }
        }

        public string GetDatabasePath()
        {
            return DatabasePath;
        }

        private string GetLinkPathForMediaItem(LibraryItem item, bool create)
        {
            string linkPath = null;
            if (string.IsNullOrEmpty(item.HardLinkFileName))
            {
                if (create)
                {
                    linkPath = LocalLibrary.LinksPath +
                        Path.DirectorySeparatorChar +
                        Guid.NewGuid().ToString() +
                        Path.GetExtension(item.LocalFilePath);
                }
            }
            else
            {
                linkPath = LocalLibrary.LinksPath +
                    Path.DirectorySeparatorChar +
                    item.HardLinkFileName +
                    Path.GetExtension(item.LocalFilePath);
            }

            return linkPath;
        }

        public string GetLinkForMediaItem(LibraryItem item)
        {
            var linkPath = GetLinkPathForMediaItem(item, true);

            var hardLinkAvailable = File.Exists(linkPath);
            if (!hardLinkAvailable)
            {
                try
                {
                    if (!Directory.Exists(LocalLibrary.LinksPath))
                        Directory.CreateDirectory(LocalLibrary.LinksPath);
                    else
                    {
                        item.HardLinkFileName = Path.GetFileNameWithoutExtension(linkPath);

                        UpdateMediaItem(item, false);
                        hardLinkAvailable = true;
                    }
                    
                }
                catch
                {
                    Logger.Log("ERROR: Failed to create hard link directory");
                }
            }

            return hardLinkAvailable ? linkPath : null;
        }

        public List<LibraryItem> GetMediaItems()
        {
            List<LibraryItem> libraryItems = new List<LibraryItem>();

            //  queryiTunesLibraryForMediaItems(libraryItems);
            queryApplicationLibraryForMediaItems(libraryItems);
            updateLibraryItemsDatabase(libraryItems);

            return libraryItems;
        }

        public List<LibraryItem> GetNewMediaItems(List<string> existing)
        {
            List<LibraryItem> libraryItems = new List<LibraryItem>();
            queryApplicationLibraryForNewMediaItems(libraryItems, existing);
            updateItemsFromDatabase(libraryItems);

            return libraryItems;
        }

        public List<string> GetDeletedMediaItems(List<string> existing)
        {
            try
            {
                string[] files = Directory.GetFiles(ItemDownloader.DownloaderPath, "*.mp4");
                return existing.Except(files).ToList();
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.GetDeletedMediaItems: " + ex);
            }

            return null;
        }

        private void updateLibraryItemsDatabase(List<LibraryItem> libraryItems)
        {
            try
            {
                lock (databaseSync)
                {
                    using (var conn = new SQLiteConnection(DatabasePath))
                    {
                        conn.CreateTable<LibraryItemProperties>();

                        List<LibraryItemProperties> orphanProperties = new List<LibraryItemProperties>();
                        foreach (LibraryItemProperties itemProperties in conn.Table<LibraryItemProperties>())
                        {
                            var libItem = libraryItems.FirstOrDefault(item =>
                            {
                                var found = false;
                                if (item.Storage == itemProperties.Storage)
                                {
                                    if (item.Storage == LibraryItemStorage.AppLocal)
                                    {
                                        // match local items by relative path
                                        string relativePath = item.LocalFilePath.Substring(ItemDownloader.DownloaderPath.Length);
                                        found = relativePath == itemProperties.Path;
                                    }
                                    else if (item.Storage == LibraryItemStorage.iTunes)
                                    {
                                        // match iTunes items by relative path
                                        found = item.ID == itemProperties.Id;
                                    }
                                }

                                return found;
                            });

                            if (libItem != null)
                            {
                                if (libItem.Storage == LibraryItemStorage.AppLocal)
                                    libItem.ID = itemProperties.Id;

                                libItem.BookmarkTime = itemProperties.BookmarkTime;
                                libItem.HardLinkFileName = itemProperties.HardLinkName;
                            }
                            else
                                orphanProperties.Add(itemProperties);
                        }

                        foreach (LibraryItemProperties orphanProperty in orphanProperties)
                            conn.Table<LibraryItemProperties>().Delete(item => (orphanProperty.Storage == item.Storage) && (orphanProperty.Id == item.Id));
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.updateLibraryItemsDatabase: " + ex);
            }
        }

        private void updateItemsFromDatabase(List<LibraryItem> libraryItems)
        {
            try
            {
                if (libraryItems.Any())
                {
                    lock (databaseSync)
                    {
                        using (var conn = new SQLiteConnection(DatabasePath))
                        {
                            conn.CreateTable<LibraryItemProperties>();
                            var dbItems = conn.Table<LibraryItemProperties>().ToList();
                            string downloaderPath = ItemDownloader.DownloaderPath;
                            foreach (var libraryItem in libraryItems)
                            {
                                string relativePath = libraryItem.LocalFilePath.Substring(downloaderPath.Length);
                                var dbItem = dbItems.FirstOrDefault(i => (i.Storage == libraryItem.Storage) && (i.Path == relativePath));
                                if (dbItem != null)
                                {
                                    libraryItem.ID = dbItem.Id;
                                    libraryItem.BookmarkTime = dbItem.BookmarkTime;
                                    libraryItem.HardLinkFileName = dbItem.HardLinkName;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.updateItemsFromDatabase: " + ex);
            }
        }


        public bool CreateMediaItem(LibraryItem libraryItem, bool updateMetadata)
        {

            try
            {


               
                using (var conn = new SQLiteConnection(DatabasePath))
                {
                    LibraryItemProperties dbItem = null;

                    if (libraryItem.Storage == LibraryItemStorage.AppLocal)
                    {
                        string relativePath = libraryItem.LocalFilePath.Substring(ItemDownloader.DownloaderPath.Length);
                        dbItem = conn.Table<LibraryItemProperties>().FirstOrDefault(item => (relativePath == item.Path) && (libraryItem.Storage == item.Storage));
                    }
                    else if (libraryItem.Storage == LibraryItemStorage.iTunes)
                        dbItem = conn.Table<LibraryItemProperties>().FirstOrDefault(item => (item.Id == libraryItem.ID) && (libraryItem.Storage == item.Storage));

                    if (dbItem != null)
                        conn.Delete(dbItem);

                    LibraryItemProperties properties = new LibraryItemProperties()
                    {
                        Id = libraryItem.ID,
                        Storage = libraryItem.Storage,
                        Path = (libraryItem.Storage == LibraryItemStorage.AppLocal) ? libraryItem.LocalFilePath.Substring(ItemDownloader.DownloaderPath.Length) : libraryItem.LocalFilePath
                    };

                    conn.Insert(properties);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.CreateMediaItem: " + ex);
            }

            return false;
        }

        //private void fetchMetadata(AVAsset asset, ref LibraryItem libraryItem)
        //{
        //    if (libraryItem != null)
        //    {
        //        //Common metadata
        //        libraryItem.IsLocal = true;
              
        //       if (!string.IsNullOrEmpty(libraryItem.LocalFilePath) && File.Exists(libraryItem.LocalFilePath))
        //            libraryItem.Updated = File.GetLastWriteTime(libraryItem.LocalFilePath);
        //    }
        //}

        public bool UpdateMediaItem(LibraryItem libraryItem, bool createIfNotExisting)
        {
            var updated = false;

            try
            {
                lock (databaseSync)
                {
                    using (var conn = new SQLiteConnection(DatabasePath))
                    {
                        LibraryItemProperties dbItem = null;

                        if (libraryItem.Storage == LibraryItemStorage.AppLocal)
                        {
                            string relativePath = libraryItem.LocalFilePath.Substring(ItemDownloader.DownloaderPath.Length);
                            dbItem = conn.Table<LibraryItemProperties>().FirstOrDefault(item => (relativePath == item.Path) && (libraryItem.Storage == item.Storage));
                        }
                        else if (libraryItem.Storage == LibraryItemStorage.iTunes)
                            dbItem = conn.Table<LibraryItemProperties>().FirstOrDefault(item => (item.Id == libraryItem.ID) && (libraryItem.Storage == item.Storage));

                        if (dbItem != null)
                        {
                            dbItem.BookmarkTime = libraryItem.BookmarkTime;
                            dbItem.HardLinkName = libraryItem.HardLinkFileName;

                            conn.Update(dbItem);
                        }
                        else if (createIfNotExisting)
                        {
                            LibraryItemProperties properties = new LibraryItemProperties()
                            {
                                Id = libraryItem.ID,
                                Storage = libraryItem.Storage,
                                Path = (libraryItem.Storage == LibraryItemStorage.AppLocal) ? libraryItem.LocalFilePath.Substring(ItemDownloader.DownloaderPath.Length) : libraryItem.LocalFilePath,
                                HardLinkName = libraryItem.HardLinkFileName
                            };

                            conn.Insert(properties);
                            updated = true;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.UpdateMediaItem: " + ex);
            }

            return updated;
        }

        public bool DeleteMediaItem(LibraryItem libraryItem, bool defeleFromDisk)
        {
            if (libraryItem == null)
                return false;

            try
            {
                if (defeleFromDisk && !string.IsNullOrEmpty(libraryItem.LocalFilePath) && File.Exists(libraryItem.LocalFilePath))
                {
                    File.Delete(libraryItem.LocalFilePath);

                    if (!string.IsNullOrEmpty(libraryItem.HardLinkFileName))
                    {
                        var linkPath = GetLinkPathForMediaItem(libraryItem, false);
                        if (!string.IsNullOrEmpty(linkPath) && File.Exists(linkPath))
                            File.Delete(linkPath);
                    }
                }

                lock (databaseSync)
                {
                    using (var conn = new SQLiteConnection(DatabasePath))
                    {
                         conn.Table<LibraryItemProperties>().Delete(item => (libraryItem.Storage == item.Storage) && (libraryItem.ID == item.Id));
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.DeleteMediaItem: " + ex);
            }

            return false;
        }


        public bool CanReadRecordingMetadata(string localFilePath)
        {
            try
            {
                //using (AVAsset asset = AVAsset.FromUrl(NSUrl.FromFilename(localFilePath)))
                //    return isRecording(asset);
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.CanReadRecordingMetadata: " + ex);
            }

            return false;
        }

        public void queryApplicationLibraryForMediaItems(List<LibraryItem> libraryItems)
        {
            try
            {
                string[] files = Directory.GetFiles(ItemDownloader.DownloaderPath, "*.mp4");

                foreach (string file in files)
                {
                    var libraryItem = libraryItemFromFile(file);
                    if (libraryItem != null)
                        libraryItems.Add(libraryItem);
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.queryApplicationLibraryForMediaItems: " + ex);
            }
        }

        public void queryApplicationLibraryForNewMediaItems(List<LibraryItem> libraryItems, List<string> existing)
        {
            try
            {
                string[] files = Directory.GetFiles(ItemDownloader.DownloaderPath, "*.mp4");
                files = files.Except(existing).ToArray();

                foreach (string file in files)
                {
                    var libraryItem = libraryItemFromFile(file);
                    if (libraryItem != null)
                        libraryItems.Add(libraryItem);
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.queryApplicationLibraryForMediaItems: " + ex);
            }
        }

        private LibraryItem libraryItemFromFile(string file)
        {
            //using (AVAsset asset = AVAsset.FromUrl(NSUrl.FromFilename(file)))
            //{
            //    if (isRecording(asset))
            //    {
            //        LibraryItem libraryItem = new LibraryItem();
            //        libraryItem.Storage = LibraryItemStorage.AppLocal;
            //        libraryItem.LocalFilePath = file;
            //        fetchMetadata(asset, ref libraryItem);
            //        return libraryItem;
            //    }
            //    else
            //    {
            //        Logger.Log($"WARNNING: {file} is not a MMT recording with valid PLVF flag. DELETING");
            //        NSError error;
            //        if (!NSFileManager.DefaultManager.Remove(NSUrl.FromFilename(file), out error))
            //            Logger.Log($"WARNNING: Unable to delete {file}: " + error);
            //    }
            //}

            return null;
        }



        public List<string> GetStoredCloudItemIds(string email)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    email = email.ToLower();
                    lock (databaseSync)
                    {
                        using (var conn = new SQLiteConnection(DatabasePath))
                        {
                            var info = conn.GetTableInfo("CloudItemProperties");
                            if ((info != null) && info.Any())
                            {
                                var filtered = conn.Table<CloudItemProperties>().Where(item => item.Email == email);
                                return filtered.ToList().Select(i => i.Id).ToList();
                            }
                            else
                                conn.CreateTable<CloudItemProperties>();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.GetStoredCloudItemIds: " + ex);
            }

            return null;
        }

        public bool CreateCloudItemId(string id, string email)
        {
            bool created = false;

            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    email = email.ToLower();
                    lock (databaseSync)
                    {
                        using (var conn = new SQLiteConnection(DatabasePath))
                        {
                            var info = conn.GetTableInfo("CloudItemProperties");
                            if ((info == null) || !info.Any())
                                conn.CreateTable<CloudItemProperties>();

                            var dbItem = conn.Table<CloudItemProperties>().FirstOrDefault(item => (item.Id == id) && (item.Email == email));
                            if (dbItem != null)
                                conn.Delete(dbItem);

                            CloudItemProperties properties = new CloudItemProperties()
                            {
                                Id = id,
                                Email = email
                            };

                            conn.Insert(properties);
                            created = true;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.CreateCloudItemId: " + ex);
            }

            return created;
        }

        public bool DeleteCloudItemId(string id, string email)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    email = email.ToLower();
                    lock (databaseSync)
                    {
                        using (var conn = new SQLiteConnection(DatabasePath))
                        {
                            conn.Table<CloudItemProperties>()
                                .FirstOrDefault(item => (item.Id == id) && (item.Email == email));
                            //conn.Table<CloudItemProperties>().Delete(item => (item.Id == id) && (item.Email == email));
                        }

                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Log("ERROR: LocalLibrary.DeleteCloudItemId: " + ex);
            }

            return false;
        }

        public bool IsItemAddedForDownload(string id, string email)
        {
            var storedCloudItemIds = GetStoredCloudItemIds(email);
            return (storedCloudItemIds != null) && storedCloudItemIds.Contains(id);
        }


    }
}