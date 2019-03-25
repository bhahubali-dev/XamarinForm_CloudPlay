//#define ENABLE_ITUNES_ITEMS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Xamarin.Forms;
using AVFoundation;
using Foundation;
using MediaPlayer;
using SQLite;
using PlayOnCloud.iOS;
using PlayOnCloud.Model;

[assembly: Dependency(typeof(LocalLibrary))]
namespace PlayOnCloud.iOS
{
	public class LocalLibrary : ILocalLibrary
	{
		private static object databaseSync = new object();

		public static string DatabasePath
		{
			get
			{
				var documents = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User);
				return documents.First().Path + Path.DirectorySeparatorChar + "playoncloud.db";
			}
		}

		public static string LinksPath
		{
			get
			{
				var documents = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User);
				return documents.First().Path + Path.DirectorySeparatorChar + "links";
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
			if(!hardLinkAvailable)
			{
				try
				{
					if (!Directory.Exists(LocalLibrary.LinksPath))
						Directory.CreateDirectory(LocalLibrary.LinksPath);

					NSError error;
					NSFileManager.DefaultManager.Link(item.LocalFilePath, linkPath, out error);
					if (error == null)
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

			queryiTunesLibraryForMediaItems(libraryItems);
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
			catch (Exception ex)
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
			catch (Exception ex)
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
			catch (Exception ex)
			{
				Logger.Log("ERROR: LocalLibrary.updateItemsFromDatabase: " + ex);
			}
		}

		public bool CreateMediaItem(LibraryItem libraryItem, bool updateMetadata)
		{
			try
			{
				using (AVAsset asset = AVAsset.FromUrl(NSUrl.FromFilename(libraryItem.LocalFilePath)))
				{
					if (isRecording(asset))
					{
						if (updateMetadata)
							fetchMetadata(asset, ref libraryItem);

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
					}
					else
					{
						Logger.Log($"WARNNING: {libraryItem.LocalFilePath} is not a MMT recording with valid PLVF flag. DELETING");
						NSError error;
						if (!NSFileManager.DefaultManager.Remove(NSUrl.FromFilename(libraryItem.LocalFilePath), out error))
							Logger.Log($"WARNNING: Unable to delete {libraryItem.LocalFilePath}: " + error);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: LocalLibrary.CreateMediaItem: " + ex);
			}

			return false;
		}

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
			catch (Exception ex)
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
			catch (Exception ex)
			{
				Logger.Log("ERROR: LocalLibrary.DeleteMediaItem: " + ex);
			}

			return false;
		}

		public bool CanReadRecordingMetadata(string localFilePath)
		{
			try
			{
				using (AVAsset asset = AVAsset.FromUrl(NSUrl.FromFilename(localFilePath)))
					return isRecording(asset);
			}
			catch (Exception ex)
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
			catch (Exception ex)
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
			catch (Exception ex)
			{
				Logger.Log("ERROR: LocalLibrary.queryApplicationLibraryForMediaItems: " + ex);
			}
		}

		private LibraryItem libraryItemFromFile(string file)
		{
			using (AVAsset asset = AVAsset.FromUrl(NSUrl.FromFilename(file)))
			{
				if (isRecording(asset))
				{
					LibraryItem libraryItem = new LibraryItem();
					libraryItem.Storage = LibraryItemStorage.AppLocal;
					libraryItem.LocalFilePath = file;
					fetchMetadata(asset, ref libraryItem);
					return libraryItem;
				}
				else
				{
					Logger.Log($"WARNNING: {file} is not a MMT recording with valid PLVF flag. DELETING");
					NSError error;
					if (!NSFileManager.DefaultManager.Remove(NSUrl.FromFilename(file), out error))
						Logger.Log($"WARNNING: Unable to delete {file}: " + error);
				}
			}

			return null;
		}

		public void queryiTunesLibraryForMediaItems(List<LibraryItem> libraryItems)
		{
#if ENABLE_ITUNES_ITEMS
			try
			{
				var mq = new MPMediaQuery();
				var value = NSNumber.FromInt32((int) MPMediaType.TypeAnyVideo);
				var predicate = MPMediaPropertyPredicate.PredicateWithValue(value, MPMediaItem.MediaTypeProperty);
				mq.AddFilterPredicate(predicate);

				List<MPMediaItem> mediaItems = new List<MPMediaItem>(mq.Items);
				foreach (MPMediaItem mediaItem in mediaItems)
				{
					AVAsset asset = AVAsset.FromUrl(mediaItem.AssetURL);
					if ((asset != null) && isRecording(asset))
					{
						LibraryItem libraryItem = new LibraryItem();
						libraryItem.Storage = LibraryItemStorage.iTunes;
						libraryItem.ID = mediaItem.PersistentID.ToString();
						libraryItem.LocalFilePath = mediaItem.AssetURL.ToString();
						fetchMetadata(asset, ref libraryItem);

						libraryItems.Add(libraryItem);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: LocalLibrary.queryiTunesLibraryForMediaItems: " + ex);
			}
#endif
		}

		private bool isRecording(AVAsset asset)
		{
			string plvf = fetchStringMetadata(asset.Metadata, NSObject.FromObject("com.apple.iTunes.PLVF"), "itlk");
			var isRecording = (!string.IsNullOrEmpty(plvf) && (plvf == "1"));

			return isRecording;
		}

		private byte[] fetchThumbnailData(AVAsset asset)
		{
			AVMetadataItem[] coverArtMetadataItems = AVMetadataItem.FilterWithKey(asset.CommonMetadata,
				AVMetadata.CommonKeyArtwork,
				AVMetadata.KeySpaceCommon);

			byte[] thumbnailData = null;
			if (coverArtMetadataItems.Length > 0)
			{
				NSData artImageData = (NSData)coverArtMetadataItems[0].Value;

				thumbnailData = new byte[artImageData.Length];
				Marshal.Copy(artImageData.Bytes, thumbnailData, 0, (int)artImageData.Length);
			}

			return thumbnailData;
		}

		private string fetchStringMetadata(AVMetadataItem[] metadataItems, NSObject key, string keySpace)
		{
			AVMetadataItem[] entryMetadataItems = AVMetadataItem.FilterWithKey(metadataItems,
				key,
				keySpace);

			string entry = null;
			if (entryMetadataItems.Length > 0)
				entry = entryMetadataItems[0].StringValue;

			return entry;
		}

		private int fetchNumericMetadata(AVMetadataItem[] metadataItems, NSObject key, string keySpace)
		{
			AVMetadataItem[] entryMetadataItems = AVMetadataItem.FilterWithKey(metadataItems,
				key,
				keySpace);

			int entry = 0;
			if (entryMetadataItems.Length > 0)
				entry = (int)entryMetadataItems[0].NumberValue;

			return entry;
		}

		private List<Chapter> fetchChapters(AVAsset asset)
		{
			List<Chapter> chapters = new List<Chapter>();

			string[] preferred = NSLocale.PreferredLanguages;
			List<string> languages = new List<string>(preferred);
			languages.AddRange(asset.AvailableChapterLocales.Select(locale => locale.Identifier));

			AVTimedMetadataGroup[] groups = asset.GetChapterMetadataGroupsBestMatchingPreferredLanguages(languages.ToArray());
			foreach (AVTimedMetadataGroup group in groups)
			{
				AVMetadataItem[] item = AVMetadataItem.FilterWithKey(group.Items, NSObject.FromObject("title"), AVMetadata.KeySpaceCommon);
				if ((item != null) && (item.Length > 0))
				{
					Chapter newChapter = new Chapter();
					newChapter.StartTime = item[0].Time.Seconds;
					if (item[0].StringValue == "Video")
						newChapter.Type = ChapterType.Video;
					else if (item[0].StringValue == "Advertisement")
						newChapter.Type = ChapterType.Advertisement;
					else
						newChapter.Type = ChapterType.Unknown;

					chapters.Add(newChapter);
				}
			}

			return chapters;
		}

		private void fetchMetadata(AVAsset asset, ref LibraryItem libraryItem)
		{
			if (libraryItem != null)
			{
				//Common metadata
				libraryItem.IsLocal = true;
				libraryItem.SmallThumbnailData = libraryItem.LargeThumbnailData = fetchThumbnailData(asset);
				libraryItem.Title = fetchStringMetadata(asset.CommonMetadata, AVMetadata.CommonKeyTitle, AVMetadata.KeySpaceCommon);
				libraryItem.Duration = (long)asset.Duration.Seconds * 1000;
				libraryItem.Description = fetchStringMetadata(asset.MetadataForFormat("com.apple.itunes"), NSObject.FromObject("©cmt"), AVMetadata.KeySpaceiTunes);
				libraryItem.Series = fetchStringMetadata(asset.MetadataForFormat("com.apple.itunes"), NSObject.FromObject("tvsh"), AVMetadata.KeySpaceiTunes);
				libraryItem.Season = fetchStringMetadata(asset.MetadataForFormat("com.apple.itunes"), NSObject.FromObject("tvsn"), AVMetadata.KeySpaceiTunes);
				libraryItem.Episode = fetchStringMetadata(asset.MetadataForFormat("com.apple.itunes"), NSObject.FromObject("tves"), AVMetadata.KeySpaceiTunes);
				libraryItem.AirDate = fetchStringMetadata(asset.Metadata, NSObject.FromObject("com.apple.iTunes.AirDate"), "itlk");
				libraryItem.BrowsePath = fetchStringMetadata(asset.Metadata, NSObject.FromObject("com.apple.iTunes.Browsepath"), "itlk");

				DateTime recordedDate;
				if (DateTime.TryParse(fetchStringMetadata(asset.Metadata, NSObject.FromObject("com.apple.iTunes.RecordingTimestamp"), "itlk"), out recordedDate))
					libraryItem.Recorded = recordedDate;

				//Chapters
				string hasChapters = fetchStringMetadata(asset.Metadata, NSObject.FromObject("com.apple.iTunes.HasChapters"), "itlk");
				if (!string.IsNullOrEmpty(hasChapters) && (hasChapters == "1"))
				{
					libraryItem.HasChapters = true;
					libraryItem.Chapters = fetchChapters(asset);
				}

				if (!string.IsNullOrEmpty(libraryItem.LocalFilePath) && File.Exists(libraryItem.LocalFilePath))
					libraryItem.Updated = File.GetLastWriteTime(libraryItem.LocalFilePath);
			}
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
			catch (Exception ex)
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
			catch (Exception ex)
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
							conn.Table<CloudItemProperties>().Delete(item => (item.Id == id) && (item.Email == email));
					}
				}

				return true;
			}
			catch (Exception ex)
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
