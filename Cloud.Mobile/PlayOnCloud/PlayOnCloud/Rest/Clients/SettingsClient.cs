using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayOnCloud.Model;
using MediaMallTechnologies;

namespace PlayOnCloud
{
	public static class SettingsClient
	{
		public static Task<CredentialsValidation> ValidateCredentials(string channelID, ChannelLoginInfo loginInfo)
		{
			var content = new ByteArrayContent(AccessInfoEncryptor.Encrypt(JsonConvert.SerializeObject(loginInfo)));
			return RestService.Instance.PostContentDirectoryDataAsync<CredentialsValidation>("settings/" + channelID + "/credentials/validation", content);
		}

		public static Task<IEnumerable<ProviderCode>> GetCableProviders(string channelID)
		{
			return RestService.Instance.GetContentDirectoryJsonDataAsync<IEnumerable<ProviderCode>>("settings/" + channelID + "/providercodes");
		}
	}
}
