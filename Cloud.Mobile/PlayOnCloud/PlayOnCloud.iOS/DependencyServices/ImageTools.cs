using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Foundation;
using UIKit;
using PlayOnCloud.iOS;
using FFImageLoading;

[assembly: Dependency(typeof(ImageTools))]
namespace PlayOnCloud.iOS
{
	public class ImageTools : IImageTools
	{
		public void GetImageSize(byte[] data, out int width, out int height)
		{
			width = 0;
			height = 0;

			if ((data != null) && (data.Length > 0))
			{
				try
				{
					using (UIImage image = new UIImage(NSData.FromArray(data)))
					{
						if (image != null)
						{
							width = (int)image.Size.Width;
							height = (int)image.Size.Height;
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Log("ERROR: ImageToolsService.GetImageSize: " + ex);
				}
			}
		}

		public async Task<byte[]> GetImageFromCache(string key)
		{
			try
			{
				var configuration = ImageService.Instance.Config;
				using (var stream = await configuration.DiskCache.TryGetStreamAsync(configuration.MD5Helper.MD5(key)))
				{
					if (stream != null)
					{
						using (MemoryStream ms = new MemoryStream())
						{
							stream.CopyTo(ms);
							return ms.ToArray();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: ImageToolsService.GetImageFromCache: " + ex);
			}

			return null;
		}

		public Task PreloadUrl(string url)
		{
			try
			{
				var configuration = ImageService.Instance.Config;
				var exist = configuration.DiskCache.ExistsAsync(configuration.MD5Helper.MD5(url)).Result;
				if (!exist)
					return ImageService.Instance.LoadUrl(url).DownloadOnlyAsync();
			}
			catch (Exception ex)
			{
				Logger.Log("ERROR: ImageToolsService.PreloadUrl: " + ex);
			}

			return Task.Delay(0);
		}
	}
}
