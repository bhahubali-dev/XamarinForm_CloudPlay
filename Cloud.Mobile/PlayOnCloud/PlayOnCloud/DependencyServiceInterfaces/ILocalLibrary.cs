using System.Collections.Generic;
using PlayOnCloud.Model;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public interface ILocalLibrary
	{
		string GetDatabasePath();

		string GetLinkForMediaItem(LibraryItem item);

		List<LibraryItem> GetMediaItems();

		List<LibraryItem> GetNewMediaItems(List<string> existing);

		List<string> GetDeletedMediaItems(List<string> existing);

		bool CreateMediaItem(LibraryItem libraryItem, bool updateMetadata);

		bool UpdateMediaItem(LibraryItem libraryItem, bool createIfNotExisting);

		bool DeleteMediaItem(LibraryItem item, bool deleteFromDisk);

		List<string> GetStoredCloudItemIds(string email);

		bool CreateCloudItemId(string id, string email);

		bool DeleteCloudItemId(string id, string email);

		bool IsItemAddedForDownload(string id, string email);

		bool CanReadRecordingMetadata(string localFilePath);
	}

	public class LocalLibraryService
	{
		private static volatile ILocalLibrary instance;
		private static object syncRoot = new object();

		public static ILocalLibrary Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<ILocalLibrary>();

				return instance;
			}
		}
	}
}
