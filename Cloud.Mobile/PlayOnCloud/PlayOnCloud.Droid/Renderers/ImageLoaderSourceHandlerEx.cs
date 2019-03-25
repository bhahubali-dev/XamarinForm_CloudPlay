using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PlayOnCloud.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportImageSourceHandler(typeof(UriImageSource), typeof(ImageLoaderSourceHandlerEx))]
namespace PlayOnCloud.Droid.Renderers
{
    public class ImageLoaderSourceHandlerEx : IImageSourceHandler
    {
        
        private static readonly Dictionary<string, TaskCompletionSource<object>> currentCalls = new Dictionary<string, TaskCompletionSource<object>>();

        private static readonly object lockObj = new object();

        private readonly ImageLoaderSourceHandler originalImageLoaderSourceHandler = new ImageLoaderSourceHandler();

        public async Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = new CancellationToken())
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

                var uiImage = await originalImageLoaderSourceHandler.LoadImageAsync(imagesource, Forms.Context, cancelationToken);

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