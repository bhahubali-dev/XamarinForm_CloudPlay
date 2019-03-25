using PlayOnCloud.Model;
using SQLite;

namespace PlayOnCloud.iOS
{
	[Table("LibraryItemProperties")]
	public class LibraryItemProperties
	{
		[PrimaryKey, AutoIncrement]
		public int Key { get; set; }

		public string Id { get; set; }

		public double BookmarkTime { get; set; }

		public LibraryItemStorage Storage { get; set; }

		public string Path { get; set; }

		public string HardLinkName { get; set; }
	}
}
