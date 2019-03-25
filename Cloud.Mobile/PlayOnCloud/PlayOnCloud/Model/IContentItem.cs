using System.Collections.ObjectModel;

namespace PlayOnCloud.Model
{
	public interface IContentItem : IModel
	{
		string Name { get; set; }

		ObservableCollection<IContentItem> Children { get; set; }

		IContentItem Parent { get; set; }

		bool IsRoot { get; }

		bool IsFolder { get; }

		bool IsChannel { get; }

		bool IsFromSearch { get; set; }

		bool IsDeepLink { get; set; }

		string SmallThumbnailUrl { get; }

		string LargeThumbnailUrl { get; }

		bool Expired { get; set; }
	}
}
