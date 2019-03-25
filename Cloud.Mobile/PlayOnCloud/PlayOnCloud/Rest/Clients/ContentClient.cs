using MediaMallTechnologies;
using Newtonsoft.Json;
using PlayOnCloud.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayOnCloud
{
	public static class ContentClient
	{
		public static string GetSmallImageUrl(string id)
		{
			return RestService.Instance.CDSRestUrl + "content/" + id + "/image?size=small";
		}

		public static string GetSmallImageUrl(string id, SizingMode mode)
		{
			return RestService.Instance.CDSRestUrl + "content/" + id + "/image?size=small&sizingmode=" + (int)mode;
		}

		public static string GetSmallImageUrl(string id, int width, int height)
		{
			return RestService.Instance.CDSRestUrl + "content/" + id + "/image?size=small&width=" + width + "&height=" + height;
		}

		public static string GetLargeImageUrl(string id)
		{
			return RestService.Instance.CDSRestUrl + "content/" + id + "/image?size=large";
		}

		public static string GetLargeImageUrl(string id, int width, int height)
		{
			return RestService.Instance.CDSRestUrl + "content/" + id + "/image?size=large&width=" + width + "&height=" + height;
		}

		public static string GetSmallMastheadUrl(string id)
		{
			return RestService.Instance.CDSRestUrl + "content/" + id + "/masthead?size=small";
		}

		public static string GetMediumMastheadUrl(string id)
		{
			return RestService.Instance.CDSRestUrl + "content/" + id + "/masthead?size=medium";
		}

		public static string GetLoginImageUrl(string id)
		{
			return RestService.Instance.CDSRestUrl + "content/" + id + "/loginimage";
		}

		public static Task<byte[]> GetSmallImage(string id, SizingMode mode)
		{
			return RestService.Instance.GetDataAsync(new Uri(GetSmallImageUrl(id, mode)));
		}

		public static Task<byte[]> GetSmallImage(string id, int width, int height)
		{
			return RestService.Instance.GetDataAsync(new Uri(GetSmallImageUrl(id, width, height)));
		}

		public static Task<byte[]> GetImageFromUrl(string url)
		{
			return RestService.Instance.GetDataAsync(new Uri(url));
		}

		public static Task<string> GetRecordToken(string id)
		{
			return RestService.Instance.GetContentDirectoryJsonDataAsync<string>("content/" + id + "/recordtoken");
		}

		public static Task<IEnumerable<ChannelEx>> GetChannels()
		{
			return RestService.Instance.GetContentDirectoryJsonDataAsync<IEnumerable<ChannelEx>>("content");
		}

		public static Task<IEnumerable<ContentItemEx>> GetChildren(string id)
		{
			return RestService.Instance.GetContentDirectoryJsonDataAsync<IEnumerable<ContentItemEx>>("content/" + id);
		}

		public static Task<IEnumerable<ContentItemEx>> LoginAndGetChildren(string id, ChannelLoginInfo info)
		{
			var content = new ByteArrayContent(AccessInfoEncryptor.Encrypt(JsonConvert.SerializeObject(info)));
			return RestService.Instance.PostContentDirectoryDataAsync<IEnumerable<ContentItemEx>>("content/" + id, content);
		}

		public static Task<IEnumerable<ContentItemEx>> Search(string id, string terms)
		{
			return RestService.Instance.GetContentDirectoryJsonDataAsync<IEnumerable<ContentItemEx>>("content/" + id + "/search/" + terms);
		}

		public static Task<IEnumerable<ContentItemEx>> DeepLink(string id, string terms)
		{
			return RestService.Instance.GetContentDirectoryJsonDataAsync<IEnumerable<ContentItemEx>>("content/" + id + "/link/" + terms);
		}

		public static Task<ContentItemEx> GetDetails(string id)
		{
			return RestService.Instance.GetContentDirectoryJsonDataAsync<ContentItemEx>("content/" + id + "/details");
		}
	}
}
