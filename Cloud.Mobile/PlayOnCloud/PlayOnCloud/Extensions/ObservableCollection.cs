using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using PlayOnCloud.Model;

namespace PlayOnCloud
{
	static class ObservableCollectionExtensions
	{
		public static Task<List<LibraryItem>> UpdateItems(this ObservableCollection<LibraryItem> collection, IEnumerable<LibraryItem> items)
		{
			var tcs = new TaskCompletionSource<List<LibraryItem>>();

			Device.BeginInvokeOnMainThread(() =>
			{
				IModelIDEqualityComparer ec = new IModelIDEqualityComparer();
				List<LibraryItem> result = new List<LibraryItem>();

				var removedItems = collection.Except(items, ec).ToList();
				foreach (var removedItem in removedItems)
					collection.Remove(removedItem as LibraryItem);

				var addedItems = items.Except(collection, ec);
				foreach (var addedItem in addedItems)
				{
					collection.Add(addedItem as LibraryItem);
					result.Add(addedItem as LibraryItem);
				}

				tcs.SetResult(result);
			});

			return tcs.Task;
		}

		public static Task<bool> AddItems(this ObservableCollection<LibraryItem> collection, IEnumerable<LibraryItem> items)
		{
			var tcs = new TaskCompletionSource<bool>();

			Device.BeginInvokeOnMainThread(() =>
			{
				List<LibraryItem> result = new List<LibraryItem>();
				foreach (var item in items)
					collection.Add(item);

				tcs.SetResult(true);
			});

			return tcs.Task;
		}

		public static Task<bool> RemoveItems(this ObservableCollection<LibraryItem> collection, IEnumerable<LibraryItem> items)
		{
			var tcs = new TaskCompletionSource<bool>();

			Device.BeginInvokeOnMainThread(() =>
			{
				List<LibraryItem> result = new List<LibraryItem>();
				foreach (var item in items)
					if (collection.Contains(item))
						collection.Remove(item);

				tcs.SetResult(true);
			});

			return tcs.Task;
		}

		public static Task<List<RecordQueueItem>> UpdateItems(this ObservableCollection<RecordQueueItem> collection, IEnumerable<RecordQueueItem> items)
		{
			var tcs = new TaskCompletionSource<List<RecordQueueItem>>();

			Device.BeginInvokeOnMainThread(() =>
			{
				IModelIDEqualityComparer ec = new IModelIDEqualityComparer();
				List<RecordQueueItem> result = new List<RecordQueueItem>();

				var removedItems = collection.Except(items, ec).ToList();
				foreach (var removedItem in removedItems)
					collection.Remove(removedItem as RecordQueueItem);

				var addedItems = items.Except(collection, ec);
				foreach (var addedItem in addedItems)
				{
					collection.Add(addedItem as RecordQueueItem);
					result.Add(addedItem as RecordQueueItem);
				}

				foreach (var item in collection)
				{
					var serverItem = items.FirstOrDefault(i => i.ID == item.ID);
					if (serverItem != null)
						item.Rank = serverItem.Rank;
				}

				collection.SortByRank();
				tcs.SetResult(result);
			});

			return tcs.Task;
		}

		public static void SortByRank(this ObservableCollection<RecordQueueItem> collection)
		{
			var sorted = collection.OrderBy(q => q.Rank).ToList();
			for (int i = 0; i < sorted.Count; i++)
			{
				int index = collection.IndexOf(sorted[i]);
				if (index != i)
					collection.Move(index, i);
			}
		}

		public static Task UpdateItems(this ObservableCollection<Notification> collection, IEnumerable<Notification> items)
		{
			var tcs = new TaskCompletionSource<bool>();

			Device.BeginInvokeOnMainThread(() =>
			{
				IModelIDEqualityComparer ec = new IModelIDEqualityComparer();

				var removedItems = collection.Except(items, ec).ToList();
				foreach (var removedItem in removedItems)
					collection.Remove(removedItem as Notification);

				var addedItems = items.Except(collection, ec);
				foreach (var addedItem in addedItems)
					collection.Add(addedItem as Notification);

				tcs.SetResult(true);
			});

			return tcs.Task;
		}

		public static Task SortByDate(this ObservableCollection<Notification> collection)
		{
			var tcs = new TaskCompletionSource<bool>();

			Device.BeginInvokeOnMainThread(() =>
			{
				var sorted = collection.OrderByDescending(q => q.Created).ToList();
				for (int i = 0; i < sorted.Count; i++)
				{
					int index = collection.IndexOf(sorted[i]);
					if (index != i)
						collection.Move(index, i);
				}

				tcs.SetResult(true);
			});

			return tcs.Task;
		}

		private class IModelIDEqualityComparer : IEqualityComparer<IModel>
		{
			public bool Equals(IModel x, IModel y)
			{
				return (x.ID == y.ID);
			}

			public int GetHashCode(IModel obj)
			{
				if (obj != null)
				{
					if (!string.IsNullOrEmpty(obj.ID))
						return obj.ID.GetHashCode();

					return obj.GetHashCode();
				}

				return 0;
			}
		}

		public static Task SortByTitle(this ObservableCollection<LibraryItem> collection)
		{
			var tcs = new TaskCompletionSource<bool>();

			Device.BeginInvokeOnMainThread(() =>
			{
				var sorted = collection.AsEnumerable().OrderBy(x => x.FullTitle).ToList();
				for (int i = 0; i < sorted.Count; i++)
				{
					int index = collection.IndexOf(sorted[i]);
					if (index != i)
						collection.Move(index, i);
				}

				tcs.SetResult(true);
			});

			return tcs.Task;
		}

		public static Task SortByDate(this ObservableCollection<LibraryItem> collection)
		{
			var tcs = new TaskCompletionSource<bool>();

			Device.BeginInvokeOnMainThread(() =>
			{
				var sorted = collection.AsEnumerable().OrderByDescending(x => x.Updated).ToList();
				for (int i = 0; i < sorted.Count; i++)
				{
					int index = collection.IndexOf(sorted[i]);
					if (index != i)
						collection.Move(index, i);
				}

				tcs.SetResult(true);
			});

			return tcs.Task;
		}
	}
}
