using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace PlayOnCloud.Droid
{
	public interface ITypefaceCache
	{
		void StoreTypeface(string key, Typeface typeface);

		void RemoveTypeface(string key);

		Typeface RetrieveTypeface(string key);
	}

	public static class TypefaceCache
	{
		private static ITypefaceCache sharedCache;

		public static ITypefaceCache SharedCache
		{
			get
			{
				if (sharedCache == null)
					sharedCache = new DefaultTypefaceCache();

				return sharedCache;
			}
			set
			{
				if (sharedCache != null && sharedCache.GetType() == typeof(DefaultTypefaceCache))
					((DefaultTypefaceCache)sharedCache).PurgeCache();

				sharedCache = value;
			}
		}
	}

	internal class DefaultTypefaceCache : ITypefaceCache
	{
		private Dictionary<string, Typeface> _cacheDict;

		public DefaultTypefaceCache()
		{
			_cacheDict = new Dictionary<string, Typeface>();
		}

		public Typeface RetrieveTypeface(string key)
		{
			if (_cacheDict.ContainsKey(key))
				return _cacheDict[key];

			return null;
		}

		public void StoreTypeface(string key, Typeface typeface)
		{
			_cacheDict[key] = typeface;
		}

		public void RemoveTypeface(string key)
		{
			_cacheDict.Remove(key);
		}

		public void PurgeCache()
		{
			_cacheDict = new Dictionary<string, Typeface>();
		}
	}

	public static class FontExtensions
	{
		public static Typeface ToExtendedTypeface(this Font font, Context context)
		{
			Typeface typeface = null;

			var hashKey = font.ToHasmapKey();
			typeface = TypefaceCache.SharedCache.RetrieveTypeface(hashKey);
			if (typeface == null && !string.IsNullOrEmpty(font.FontFamily))
			{
				string filename = font.FontFamily;
				if (filename.LastIndexOf(".", System.StringComparison.Ordinal) != filename.Length - 4)
					filename = string.Format("{0}.ttf", filename);

				try
				{
					var path = "fonts/" + filename;
					typeface = Typeface.CreateFromAsset(context.Assets, path);
				}
				catch
				{
					try
					{
						typeface = Typeface.CreateFromFile("fonts/" + filename);
					}
					catch
					{
					}
				}
			}

			if (typeface == null)
				typeface = font.ToTypeface();

			if (typeface == null)
				typeface = Typeface.Default;

			TypefaceCache.SharedCache.StoreTypeface(hashKey, typeface);
			return typeface;
		}

		private static string ToHasmapKey(this Font font)
		{
			return string.Format("{0}.{1}.{2}.{3}", font.FontFamily, font.FontSize, font.NamedSize, (int)font.FontAttributes);
		}
	}
}