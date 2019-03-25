using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlayOnCloud.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportImageSourceHandler(typeof(UriImageSource), typeof(ImageLoaderSourceHandlerEx))]
namespace PlayOnCloud.iOS
{
	public class ImageLoaderSourceHandlerEx : IImageSourceHandler
	{
		private static readonly Dictionary<string, TaskCompletionSource<object>> currentCalls = new Dictionary<string, TaskCompletionSource<object>>();

		private static readonly object lockObj = new object();

		private readonly ImageLoaderSourceHandler originalImageLoaderSourceHandler = new ImageLoaderSourceHandler();

		public async Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = new CancellationToken(), float scale = 1)
		{
			var uriImageSource = imagesource as UriImageSource;

			if ((uriImageSource != null) && (uriImageSource.Uri != null))
			{
				var key = uriImageSource.Uri.ToString();

				TaskCompletionSource<object> existingTask = null;

				lock (lockObj)
					currentCalls.TryGetValue(key, out existingTask);

				if (existingTask != null)
					await existingTask.Task;

				var task = new TaskCompletionSource<object>();

				lock (lockObj)
					currentCalls[key] = task;

				var uiImage = await originalImageLoaderSourceHandler.LoadImageAsync(imagesource, cancelationToken, scale);

				lock (lockObj)
					if (currentCalls.TryGetValue(key, out existingTask) && (existingTask == task))
						currentCalls.Remove(key);

				task.SetResult(null);

				return uiImage;
			}

			return null;
		}
	}
}

