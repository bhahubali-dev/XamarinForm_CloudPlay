using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public interface IImageTools
	{
		void GetImageSize(byte[] data, out int width, out int height);

		Task<byte[]> GetImageFromCache(string key);

		Task PreloadUrl(string url);
	}

	public class ImageToolsService
	{
		private static volatile IImageTools instance;
		private static object syncRoot = new object();

		public static IImageTools Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
							instance = DependencyService.Get<IImageTools>();

				return instance;
			}
		}
	}
}
