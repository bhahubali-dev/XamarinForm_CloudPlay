using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayOnCloud.Model;
using PlayOnCloud.ViewModel;

namespace PlayOnCloud
{
	public class RestService
	{
		private const string defaultCDSRestUrl = "https://{0}cds.playonrecorder.com/api/";
		private const string defaultWSARestUrl = "https://{0}api.playonrecorder.com/v2/";
		private const string internetConnectionUrl = "http://www.google.com";

		private const string sharedSettingsApplicationToken = "RestService.ApplicationToken";
		private const string sharedSettingsDomainPrefix = "RestService.DomainPrefix";

		private const int SessionExpiredStatusCode = 205;
		private const int MediaBrowsingExceptionStatusCode = 432;
		private const int MediaPlaybackExceptionStatusCode = 433;
		private const int UpgradeExceptionStatusCode = 410;
		private const int MaintenanceExceptionStatusCode = 503;
		private const int TooManyRequestsStatusCode = 429;

		private static object syncRoot = new object();
		private static volatile RestService instance;
		private static HttpClient client;
		private static HttpClient clientConnection;
		private static SemaphoreSlim isConnectedSemaphore = new SemaphoreSlim(1, 1);
		private static bool isConnected = true;

		private DateTime RateLimitRetryAfter = DateTime.MinValue;
		private string domainPrefix;
		private string appToken;
		private string cdsRestUrl;
		private string wsaRestUrl;

		private int RateLimitLeftMinutes
		{
			get
			{
				var minutes = RateLimitRetryAfter.Subtract(DateTime.UtcNow).TotalMinutes;
				if (minutes < 2)
					return 1;

				return (int)minutes;
			}
		}

		public static RestService Instance
		{
			get
			{
				if (instance == null)
					lock (syncRoot)
						if (instance == null)
						{
							client = new HttpClient
							{
								MaxResponseContentBufferSize = long.MaxValue
							};

							client.DefaultRequestHeaders.Add("x-mmt-os", getOSHeader());
							client.DefaultRequestHeaders.Add("x-mmt-device", getDeviceHeader());
							client.DefaultRequestHeaders.Add("x-mmt-app", "PlayOnCloud");
							client.DefaultRequestHeaders.Add("x-mmt-version", AppVersionNumberService.Instance.GetVersion());
							client.DefaultRequestHeaders.Add("User-Agent", UserAgentService.Instance.GetUserAgent());

							clientConnection = new HttpClient
							{
								MaxResponseContentBufferSize = long.MaxValue,
								Timeout = TimeSpan.FromSeconds(25)
							};

							clientConnection.DefaultRequestHeaders.Add("User-Agent", UserAgentService.Instance.GetUserAgent());

							instance = new RestService();
						}

				return instance;
			}
		}

		private static string getOSHeader()
		{
			switch (Device.OS)
			{
				case TargetPlatform.iOS:
					return "ios";

				case TargetPlatform.Android:
					return "android";

				case TargetPlatform.Windows:
					return "windows";

				case TargetPlatform.WinPhone:
					return "winphone";

				default:
					return "unknown";
			}
		}

		private static string getDeviceHeader()
		{
			switch (Device.Idiom)
			{
				case TargetIdiom.Phone:
					if (Device.OS == TargetPlatform.iOS)
						return "iphone";

					return "phone";

				case TargetIdiom.Tablet:
					if (Device.OS == TargetPlatform.iOS)
						return "ipad";

					return "tablet";

				case TargetIdiom.Desktop:
					return "desktop";

				default:
					return "unknown";
			}
		}

		public RestService()
		{
			appToken = SharedSettingsService.Instance.GetStringValue(sharedSettingsApplicationToken);
			domainPrefix = SharedSettingsService.Instance.GetStringValue(sharedSettingsDomainPrefix);
			configureUrls();
		}

		private void configureUrls()
		{
			var dp = domainPrefix;
			if (dp == null)
				dp = string.Empty;

			cdsRestUrl = string.Format(defaultCDSRestUrl, dp);
			wsaRestUrl = string.Format(defaultWSARestUrl, dp);
			LoggerService.Instance.Log(string.Format("INFO: CDS: {0}, WSA: {1}, AT: {2}", cdsRestUrl, wsaRestUrl, !string.IsNullOrEmpty(appToken)));
		}

		public void SetDomainPrefixAndAppToken(string prefix, string token)
		{
			appToken = token;
			domainPrefix = prefix;
			SharedSettingsService.Instance.SetStringValue(sharedSettingsApplicationToken, token);
			SharedSettingsService.Instance.SetStringValue(sharedSettingsDomainPrefix, domainPrefix);
			configureUrls();
		}

		public string CDSRestUrl => cdsRestUrl;

		public string WSARestUrl => wsaRestUrl;

		public async Task<bool> GetIsConnected()
		{
			await isConnectedSemaphore.WaitAsync();
			try
			{
				return isConnected;
			}
			finally
			{
				isConnectedSemaphore.Release();
			}
		}

		public async Task SetIsConnected(bool connected)
		{
			await isConnectedSemaphore.WaitAsync();
			try
			{
				if (isConnected != connected)
					LoggerService.Instance.Log("INFO: RestService: SetIsConnected: old: " + isConnected + " new: " + connected);

				isConnected = connected;
			}
			finally
			{
				isConnectedSemaphore.Release();
			}
		}

		public event EventHandler<ServerConnectionLostEventArgs> ServerConnectionLost;

		public event EventHandler ServerConnectionRestored;

		public HttpClient Client
		{
			get { return client; }
		}

		public User User
		{
			get
			{
				//XXX: This is extremely bad design and should be passed to the rest service, not retreived from the vm
				Cloud viewModel = Application.Current.MainPage?.BindingContext as Cloud;
				return viewModel?.Account?.UserInfo;
			}
		}

		public async Task<T> GetJsonDataAsync<T>(Uri uri)
		{
			return await tryToMakeRequest<T>(() => getJsonDataAsync<T>(uri), uri.ToString());
		}

		private async Task<T> getJsonDataAsync<T>(Uri uri)
		{
			try
			{
				if (client != null)
				{
					using (var response = await client.GetAsync(uri).ConfigureAwait(false))
					{
						await SetIsConnected(true);
						var statusCode = (int)response.StatusCode;

						if (statusCode == SessionExpiredStatusCode)
							throw new SessionExpiredException();
						else if (statusCode == MediaBrowsingExceptionStatusCode)
						{
							var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
							if (!string.IsNullOrEmpty(content))
								Device.BeginInvokeOnMainThread(async () => await Application.Current.MainPage.DisplayAlert("Error", content, "OK"));
						}
						else if (statusCode == UpgradeExceptionStatusCode)
						{
							Device.BeginInvokeOnMainThread(async () => await Application.Current.MainPage.DisplayAlert("Upgrade required", "Please upgrade to the latest version of the app.", "OK"));
							throw new UpgradeException();
						}
						else if (statusCode == MaintenanceExceptionStatusCode)
						{
							Device.BeginInvokeOnMainThread(async () => await Application.Current.MainPage.DisplayAlert("Scheduled maintenance", "Thank you for your patience while we improve the PlayOn Cloud experience. Please try again in a few minutes.", "OK"));
							throw new MaintenanceException();
						}
						else if (response.IsSuccessStatusCode)
						{
							var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
							return JsonConvert.DeserializeObject<T>(content);
						}
					}
				}
			}
			catch (WebException)
			{
				throw;
			}
			catch (TaskCanceledException)
			{
				throw;
			}
			catch (SessionExpiredException)
			{
				throw;
			}
			catch (UpgradeException)
			{
				throw;
			}
			catch (MaintenanceException)
			{
				throw;
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: GetJsonDataAsync: Exception: " + ex + ". URL: " + uri.ToString());
			}

			return default(T);
		}

		public async Task<byte[]> GetDataAsync(Uri uri)
		{
			return await tryToMakeRequest<byte[]>(() => getDataAsync(uri), uri.ToString());
		}

		private async Task<byte[]> getDataAsync(Uri uri)
		{
			try
			{
				if (client != null)
				{
					using (var response = await client.GetAsync(uri).ConfigureAwait(false))
					{
						await SetIsConnected(true);
						if (response.IsSuccessStatusCode)
							return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
					}
				}
			}
			catch (WebException)
			{
				throw;
			}
			catch (TaskCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: GetDataAsync: Exception: " + ex + ". URL: " + uri.ToString());
			}

			return null;
		}

		public async Task<T> GetContentDirectoryJsonDataAsync<T>(string relativeUrl)
		{
			return await GetJsonDataAsync<T>(new Uri(CDSRestUrl + relativeUrl));
		}

		public async Task<byte[]> GetContentDirectoryDataAsync(string relativeUrl)
		{
			return await GetDataAsync(new Uri(CDSRestUrl + relativeUrl));
		}

		public async Task<T> PostContentDirectoryDataAsync<T>(string relativeUrl, HttpContent content)
		{
			return await tryToMakeRequest<T>(() => postContentDirectoryDataAsync<T>(relativeUrl, content), relativeUrl);
		}

		private async Task<T> postContentDirectoryDataAsync<T>(string relativeUrl, HttpContent content)
		{
			try
			{
				using (var response = await client.PostAsync(new Uri(CDSRestUrl + relativeUrl), content).ConfigureAwait(false))
				{
					await SetIsConnected(true);
					if (response.IsSuccessStatusCode)
					{
						var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						if (!string.IsNullOrEmpty(data))
							return JsonConvert.DeserializeObject<T>(data);
					}
				}
			}
			catch (WebException)
			{
				throw;
			}
			catch (TaskCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: PostContentDirectoryDataAsync: Exception: " + ex + ". URL: " + relativeUrl);
			}

			return default(T);
		}

		public async Task PostContentDirectoryDataAsync(string relativeUrl, HttpContent content)
		{
			try
			{
				using (var response = await client.PostAsync(new Uri(CDSRestUrl + relativeUrl), content).ConfigureAwait(false))
					await SetIsConnected(true);
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: PostContentDirectoryDataAsync: Exception: " + ex + ". URL: " + relativeUrl);
			}
		}

		public async Task<RestRequestResponse> MakeRecorderAPIPostRestRequest(string relativeUrl, Dictionary<string, string> postData)
		{
			MultipartFormDataContent content = new MultipartFormDataContent();
			foreach (var keyValue in postData)
				content.Add(new StringContent(keyValue.Value), keyValue.Key);

			return await MakeRecorderAPIRestRequest(relativeUrl, RequestMethod.POST, content, true);
		}

		public async Task<RestRequestResponse> MakeRecorderAPIRestRequest(string relativeUrl, RequestMethod requestMethod, HttpContent httpContent, bool renewTokenIfNeeded = true)
		{
			return await tryToMakeRequest<RestRequestResponse>(() => makeRecorderAPIRestRequest(relativeUrl, requestMethod, httpContent, renewTokenIfNeeded), relativeUrl);
		}

		private async Task<RestRequestResponse> makeRecorderAPIRestRequest(string relativeUrl, RequestMethod requestMethod, HttpContent httpContent, bool renewTokenIfNeeded = true)
		{
			try
			{
				if (DateTime.UtcNow < RateLimitRetryAfter)
				{
					LoggerService.Instance.Log("WARNING: MakeRecorderAPIRestRequest: URL: " + relativeUrl + ". Request not allowed. We reached rate limit. Retry at: " + RateLimitRetryAfter.ToString());
					throw new ThrottleException(relativeUrl, RateLimitLeftMinutes);
				}

				if (client != null)
				{
					var uri = new Uri(WSARestUrl + relativeUrl);

					using (var response = await performRecorderAPIRequest(uri, requestMethod, httpContent).ConfigureAwait(false))
					{
						await SetIsConnected(true);
						if ((response != null) && response.IsSuccessStatusCode)
						{
							var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

							//This should be async but given we return on a pool thread it does not really matter
							var result = JsonConvert.DeserializeObject<RestRequestResponse>(content);
							if (result != null)
							{
								if ((result.ErrorCode == ErrorCodes.TOKEN_EXPIRED) && renewTokenIfNeeded)
								{
									if (await renewToken())
										return await MakeRecorderAPIRestRequest(relativeUrl, requestMethod, httpContent, false);
									else
										return null;
								}

								if ((result.ErrorCode == ErrorCodes.UPGRADE) || (result.ErrorCode == ErrorCodes.MAINTENANCE))
								{
									string title = result.ErrorCode == ErrorCodes.MAINTENANCE ? "Maintenance" : "Error";
									string errorMessage = result.ErrorMessageClean;
									if (string.IsNullOrEmpty(errorMessage))
										errorMessage = "Unknown error";

									Device.BeginInvokeOnMainThread(async () => await Application.Current.MainPage.DisplayAlert(title, errorMessage, "OK"));
								}
							}
							else
								LoggerService.Instance.Log("ERROR: MakeRecorderAPIRestRequest: result is null. Content:" + content + ". URL: " + relativeUrl);

							return result;
						}
						else
						{
							if (response != null)
							{
								var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
								LoggerService.Instance.Log("ERROR: MakeRecorderAPIRestRequest: IsSuccessStatusCode is false. Content:" + content + ". URL: " + relativeUrl);
								if ((int)response.StatusCode == TooManyRequestsStatusCode)
								{
									LoggerService.Instance.Log("ERROR: MakeRecorderAPIRestRequest: URL: " + relativeUrl + ". Making too many requests.");
									if (response.Headers.RetryAfter.Delta.HasValue)
									{
										LoggerService.Instance.Log("ERROR: MakeRecorderAPIRestRequest: Retry after value: " + response.Headers.RetryAfter.Delta.Value);
										RateLimitRetryAfter = DateTime.UtcNow.Add(response.Headers.RetryAfter.Delta.Value);
										throw new ThrottleException(relativeUrl, RateLimitLeftMinutes);
									}
									else
										LoggerService.Instance.Log("ERROR: MakeRecorderAPIRestRequest: Retry after value is missing");
								}
							}
							else
								LoggerService.Instance.Log("ERROR: MakeRecorderAPIRestRequest: response is null" + ". URL: " + relativeUrl);
						}
					}
				}
			}
			catch (WebException)
			{
				throw;
			}
			catch (TaskCanceledException)
			{
				throw;
			}
			catch (ThrottleException)
			{
				throw;
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: MakeRecorderAPIRestRequest: Exception: " + ex + ". URL: " + relativeUrl);
			}

			return null;
		}

		public async Task<T> MakeRecorderAPIRestRequest<T>(string relativeUrl, RequestMethod requestMethod, HttpContent httpContent)
		{
			try
			{
				RestRequestResponse response = await MakeRecorderAPIRestRequest(relativeUrl, requestMethod, httpContent, true);
				if ((response != null) && response.Success)
					return response.Data.ToObject<T>();
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: MakeRecorderAPIRestRequest (Task<T>): Exception: " + ex + ". URL: " + relativeUrl);
			}

			return default(T);
		}

		private async Task<HttpResponseMessage> performRecorderAPIRequest(Uri uri, RequestMethod requestMethod, HttpContent httpContent)
		{
			HttpMethod httpMethod;

			switch (requestMethod)
			{
				case RequestMethod.GET:
					httpMethod = HttpMethod.Get;
					break;

				case RequestMethod.POST:
					httpMethod = HttpMethod.Post;
					break;

				case RequestMethod.PUT:
					httpMethod = HttpMethod.Put;
					break;

				case RequestMethod.DELETE:
					httpMethod = HttpMethod.Delete;
					break;

				default:
					return null;
			}

			var requestMessage = new HttpRequestMessage(httpMethod, uri);
			requestMessage.Content = httpContent;

			var user = User;
			if (user != null)
				requestMessage.Headers.Add("Authorization", user.Token);

			var at = appToken;
			if (!string.IsNullOrEmpty(at))
				requestMessage.Headers.Add("x-mmt-app-auth", at);

			return await client.SendAsync(requestMessage).ConfigureAwait(false);
		}

		private async Task<bool> renewToken()
		{
			var user = User;
			if (user != null)
			{
				LoggerService.Instance.Log("INFO: RestService: user token has expired: renewing");
				MultipartFormDataContent content = new MultipartFormDataContent
				{
					{ new StringContent(user.AuthToken), "auth_token" }
				};

				try
				{
					RestRequestResponse response = await MakeRecorderAPIRestRequest("login/at", RequestMethod.POST, content, false);
					if ((response != null) && response.Success)
					{
						var result = response.Data.ToObject<User>();
						if (result != null)
						{
							LoggerService.Instance.Log("INFO: RestService: token renewed");
							user.Token = result.Token;
							user.Email = result.Email;
						    user.Name = result.FirstName + " " + result.LastName;
                            UserStoreService.Instance.SaveUserCredentials(user);
							return true;
						}
						else
							LoggerService.Instance.Log("ERROR: RestService: unable to read user object from response");
					}
					else
						LoggerService.Instance.Log("ERROR: RestService: failed to renew user token");
				}
				catch (Exception ex)
				{
					LoggerService.Instance.Log("ERROR: RestService renewToken exception: " + ex);
				}
			}

			//allow only 5 requests per minute
			if (DateTime.UtcNow > RateLimitRetryAfter)
				RateLimitRetryAfter = DateTime.UtcNow.Add(TimeSpan.FromSeconds(12));

			return false;
		}

		public async Task<bool> CheckIfConnectionRestored()
		{
			if (await isConnectedSemaphore.WaitAsync(1000))
			{
				try
				{
					var scr = ServerConnectionRestored;
					if ((scr != null) && !isConnected)
					{
						isConnected = await IsInternetConnected();
						if (isConnected)
							scr(this, null);
					}

					return isConnected;
				}
				finally
				{
					isConnectedSemaphore.Release();
				}
			}

			return false;
		}

		public async Task<bool> CheckIfConnectionLost(bool silent)
		{
			var connected = await IsInternetConnected();
			await isConnectedSemaphore.WaitAsync();

			try
			{
				var scl = ServerConnectionLost;
				if (isConnected && !connected && (scl != null))
				{
					var args = new ServerConnectionLostEventArgs() { Silent = silent };
					scl(this, args);

					isConnected = !await args.Cancel.Task;
				}

				return isConnected;
			}
			finally
			{
				isConnectedSemaphore.Release();
			}
		}

		public async Task<bool> IsInternetConnected()
		{
			try
			{
				var requestMessage = new HttpRequestMessage(HttpMethod.Head, internetConnectionUrl);
				using (var response = await clientConnection.SendAsync(requestMessage).ConfigureAwait(false))
					return (response != null) && response.IsSuccessStatusCode;

			}
			catch
			{
				// ignored
			}

			return false;
		}

		private async Task<T> tryToMakeRequest<T>(Func<Task<T>> action, string url)
		{
			int retryCount = 1;
			while (true)
			{
				var requestStartTime = DateTime.UtcNow;

				try
				{
					var connected = true;
					if (!await GetIsConnected())
						connected = await IsInternetConnected();

					if(connected)
						return await action.Invoke();
					else
						return default(T);
				}
				catch (Exception ex)
				{
					if (ex is TaskCanceledException)
						LoggerService.Instance.Log("ERROR: tryToMakeRequest TaskCanceledException URL: " + url + " Request made at: " + requestStartTime.ToString());
					else if (ex is WebException)
						LoggerService.Instance.Log("ERROR: tryToMakeRequest WebException URL: " + url + " Request made at: " + requestStartTime.ToString() + " Error: " + ex);
					else
						throw ex;

					if (retryCount > 3)
					{
						var repeatRequest = await CheckIfConnectionLost(false);
						if (!repeatRequest)
							return default(T);
						else
						{
							await SetIsConnected(true);
							retryCount = 0;
						}
					}
					else
					{
						if (!(await CheckIfConnectionLost(false)))
							return default(T);
					}
				}

				LoggerService.Instance.Log("INFO: tryToMakeRequest: Retrying (retry - " + retryCount.ToString() + ") URL: " + url);
				retryCount++;
			}
		}
	}

	public class ServerConnectionLostEventArgs
	{
		public ServerConnectionLostEventArgs()
		{
			this.Cancel = new TaskCompletionSource<bool>();
		}

		public bool Silent { get; set; }
		public TaskCompletionSource<bool> Cancel { get; set; }
	}
}
