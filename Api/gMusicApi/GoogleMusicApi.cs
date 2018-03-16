using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using MusicPlayer.Managers;
using SimpleAuth;
using SimpleAuth.Providers;
using MusicPlayer.Data;
using Plugin.Connectivity;

namespace MusicPlayer.Api.GoogleMusic
{
	public class GoogleMusicApiExtraData
	{
		[JsonProperty("ls")]
		public long LastSongSync { get; set; }

		[JsonProperty("lp")]
		public long LastPlaylistSync { get; set; }

		[JsonProperty("lpss")]
		public long LastPlaylistSongSync { get; set; }

		[JsonProperty("lr")]
		public long LastRadioSync { get; set; }

		[JsonProperty("lg")]
		public long LastGenreSync { get; set; }

		[JsonProperty("hgdid")]
		public bool HasGeneratedDeviceId { get; set; }

		[JsonProperty("gdid")]
		public string GeneratedDeviceId { get; set; }
	}

	internal class GoogleMusicApi : GoogleApi
	{
		static GoogleMusicApi()
		{
			ForceNativeLogin = false;
		}
		public override string ExtraDataString
		{
			get => ExtraData?.ToJson() ?? "";
			set
			{
				extraDataString = value;
				ExtraData = !string.IsNullOrWhiteSpace(value)
					? value.ToObject<GoogleMusicApiExtraData>()
					: new GoogleMusicApiExtraData();
			}
		}
		string deviceName;
		public string DeviceName {
			get {
				if(string.IsNullOrWhiteSpace(deviceName))
					deviceName = Utility.GetSecured ("deviceName","gmusic");
				if (string.IsNullOrEmpty (deviceName))
					deviceName = Device.Name;
				return deviceName;

			}
			set {
				deviceName = value;
				Utility.SetSecured ("deviceName", value, "gmusic");
			}
		}

		public string Tier
		{
			get
			{
				string tier = "none";
				CurrentOAuthAccount?.UserData?.TryGetValue("tier", out tier);
				return tier;
			}
			set
			{
				CurrentOAuthAccount.UserData["tier"] = value;
			}
		}
		public GoogleMusicApiExtraData ExtraData { get; set; } = new GoogleMusicApiExtraData();
		
		protected override async Task<Account> PerformAuthenticate()
		{
			if (!CrossConnectivity.Current.IsConnected)
				return null;
			return await base.PerformAuthenticate();
		}

		string cultureShort = "en";
		string cultureLong = "en_US";
		string sessionId = Guid.NewGuid().ToString();
		string extraDataString;

		public GoogleMusicApi(string id, HttpMessageHandler handler = null)
			: base(id, "936475272427", "KWsJlkaMn1jGLxQpWxMnOox-", handler)
		{
			CurrentShowAuthenticator = null;

			BaseAddress = new Uri("https://mclients.googleapis.com/sj/v2.5/");
			Scopes = new[] {"https://www.google.com/accounts/OAuthLogin", "https://www.googleapis.com/auth/userinfo.email"};
            ForceRefresh = true;
			CrossConnectivity.Current.ConnectivityChanged += (sender, args) =>
			{
				if (CrossConnectivity.Current.IsConnected && this.CurrentAccount == null)
				{
					Authenticate();
				}
			};
			TokenUrl = "https://accounts.google.com/o/oauth2/token";
			try
			{
				var culture = Thread.CurrentThread.CurrentCulture;
				cultureShort = culture.TwoLetterISOLanguageName;
				cultureLong = culture.Name.Replace("-", "_");
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
//				Logger.Log(ex);
				cultureShort = "en";
				cultureLong = "en_US";
			}
		}

		protected override WebAuthenticator CreateAuthenticator()
		{
			return new GoogleMusicAuthenticator
			{
				Scope = Scopes.ToList(),
				ClientId = ClientId,
				DeviceName = Utility.DeviceName,
				DeviceId = MusicPlayer.Api.Utility.DeviceId,
				Culture = cultureShort,
				ClearCookiesBeforeLogin = CalledReset,
			};
		}

		public async Task<T> Post<T>(GoogleMusicApiRequest request, bool includeDeviceHeaders = false) where T : RootApiObject
		{
			if (CurrentAccount == null)
				await Authenticate ();
			var requestText = request.ToJson();
			int tryCount = 0;
			bool success = false;
			const string url = "https://www.googleapis.com/rpc?prettyPrint=false";
			T result = default(T);
			while (!success && tryCount < 3)
			{
				var content = new StringContent(requestText, Encoding.UTF8, "application/json");
				if (includeDeviceHeaders) {
					content.Headers.Add("X-Device-ID", DeviceId);
					content.Headers.Add("X-Device-FriendlyName", DeviceName);
				}
				var message = await PostMessage(url, content);
				var json = await message.Content.ReadAsStringAsync();
				//Debug.WriteLine(json);
				result = Deserialize<T>(json);
				//result = await Post<T>(url, content );
				success = result != null && result.Error == null;
				if (!success && result.Error.Code != 400)
				{
					if (result.Error != null)
						await RefreshAccount(CurrentAccount);
					await Task.Delay(1000);
				}
				tryCount++;
			}
			if(result?.Error != null)
				LogManager.Shared.Report(result.Error,requestText);
            return result;
		}

		public async Task<T> GetLatest<T>(string path, Dictionary<string,string> queryParameters = null , bool includeDeviceHeaders = true) where T : RootApiObject
		{
			if (CurrentAccount == null)
				await Authenticate();
			int tryCount = 0;
			bool success = false;
			if (queryParameters == null)
				queryParameters = new Dictionary<string, string>();
			queryParameters["dv"] = "3000038001007";
			queryParameters["hl"] = cultureShort;
			queryParameters["prettyPrint"] = "false";
			const string url = "";
			Dictionary<string, string> headers = new Dictionary<string, string>();
			if (includeDeviceHeaders)
			{
				headers["X-Device-ID"] = DeviceId;
				headers["X-Device-FriendlyName"] = DeviceName;
			}
			T result = default(T);
			while (!success && tryCount < 3)
			{
				var json = await SendObjectMessage(path, null, HttpMethod.Get, queryParameters, headers, true);
				result = Deserialize<T>(json);
				success = result != null && result.Error == null;
				if (!success && result.Error.Code != 400)
				{
					if (result.Error != null)
						await RefreshAccount(CurrentAccount);
					await Task.Delay(1000);
				}
				tryCount++;
			}
			if (result?.Error != null)
				LogManager.Shared.Report(result.Error, path);
			return result;
		}

		public async Task<T> PostLatest<T>(string path,object body, Dictionary<string, string> queryParameters = null, bool includeDeviceHeaders = true) where T : RootApiObject
		{
			if (CurrentAccount == null)
				await Authenticate();
			int tryCount = 0;
			bool success = false;
			if (queryParameters == null)
				queryParameters = new Dictionary<string, string>();
			queryParameters["dv"] = "3000038001007";
			queryParameters["hl"] = cultureShort;
			queryParameters["prettyPrint"] = "false";


			var requestText = SerializeObject(body);

			Dictionary<string, string> headers = new Dictionary<string, string>();
			if (includeDeviceHeaders)
			{
				headers["X-Device-ID"] = DeviceId;
				headers["X-Device-FriendlyName"] = DeviceName;
			}

			T result = default(T);
			while (!success && tryCount < 3)
			{
				var content = new StringContent(requestText, Encoding.UTF8, "application/json");
				var json = await SendObjectMessage(path, content, HttpMethod.Post, queryParameters,headers );
				result = Deserialize<T>(json);
				success = result != null && result.Error == null;
				if (!success && result.Error.Code != 400)
				{
					if (result.Error != null)
						await RefreshAccount(CurrentAccount);
					await Task.Delay(1000);
				}
				tryCount++;
			}
			if (result?.Error != null)
				LogManager.Shared.Report(result.Error, path);
			return result;
		}


		protected override async Task<OAuthAccount> GetAccountFromAuthCode(WebAuthenticator authenticator, string identifier)
		{
			var account = await base.GetAccountFromAuthCode(authenticator, identifier);
			account.UserData["MasterToken"] = account.Token;
			account.UserData["MasterRefreshToken"] = account.RefreshToken;
			SaveAccount(account);
			//Google now gets the real token from the master token.
			account.Identifier = identifier;
			currentAccount = account;
			await PrepareClient(Client);
			await GetAuthToken(account);
			return account;
		}

		public Task<bool> RefreshYoutubeAccount(Account account)
		{
			return RefreshAccount (account);
		}

		Task<bool> refreshTask;
		object locker = new object();
		protected override async Task<bool> RefreshAccount(Account account)
		{
			if (!CrossConnectivity.Current.IsConnected)
				return false;
			lock (locker) {
				if (refreshTask == null || refreshTask.IsCompleted)
					refreshTask = refreshAccount (account);
			}
			return await refreshTask;
		}

		protected async Task<bool> refreshAccount(Account account)
		{
			if (!await base.RefreshAccount(account))
				return false;
			account.UserData["MasterToken"] = (account as OAuthAccount)?.Token;
			await GetAuthToken(account as OAuthAccount);
			if (!account.IsValid())
			{
				refreshTask = null;
				if (await PerformAuthenticate() == null)
					return false;
			}
			await PrepareClient(Client);

			SaveAccount(account);
			return true;
		}

//		protected override T GetAccount<T> (string identifier)
//		{
//			var acc = base.GetAccount<T> (identifier);
//			var oacc = acc as OAuthAccount;
////			oacc.Token = "ya29.PgJbX9jfcZ7FRwTlcE3T1FwIjywhlLkNqQlrz_tLLj3oe9SaOA9Ls37yp0dXsg7kcwh5xjf0-6boyg";
////			oacc.RefreshToken = "1/KF5nZhIDhLVxt-DR19eq5uJu3TJLuLenqvIEd02uX1190RDknAdJa_sgfheVM0XT";
//			return acc;
//		}

		List<SettingsRootObject.Item> devices;
		int deviceIndex = -1;
		public async Task<string> GetDeviceId(bool next = false)
		{
			try
			{
				if (next)
					deviceIndex ++;
				else if (!string.IsNullOrWhiteSpace(DeviceId))
					return DeviceId;
				if (devices == null || !devices.Any())
				{
					var settings =
						await Get<SettingsRootObject>("https://mclients.googleapis.com/sj/v1.11/devicemanagementinfo");
					if (settings != null)
					{
						devices = settings.Data?.items.Where(x => x.Type == "IOS").OrderBy(x => x.LastAccessedTimeMs).ToList() ?? new List<SettingsRootObject.Item>();
					}
				}

				if (deviceIndex < 0)
					deviceIndex = 0;
				if(deviceIndex >= devices.Count){
					DeviceId = Utility.DeviceId;
				}
				else
				{
					var device = devices[deviceIndex];
					DeviceId = device.Id;
					DeviceName = device.FriendlyName;
				}
				ApiManager.Shared.SaveApi(this);
				return DeviceId;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			return string.IsNullOrWhiteSpace(DeviceId) ?  Utility.DeviceId : DeviceId;
		}
		public bool HasMoreDevices()
		{
			if (devices == null || devices.Count == 0)
				return true;
			return deviceIndex < devices.Count - (ExtraData.HasGeneratedDeviceId ? 0 : 1);
			
		}

		public async Task GetUserConfig()
		{
			try
			{
				var tier = Tier ?? "none";
				if (tier != "none")
					return;
				var resp = await GetLatest<RootConfigApiObject>("config", new Dictionary<string, string>
				{
					["tier"] = Tier,
					["user-targeting"] = "false",
				});
				var isNautilusUser = resp?.Data?.Entries?.FirstOrDefault(x => x.Key == "isNautilusUser");
				Tier = isNautilusUser?.Value == "true" ? "aa" : "fr";
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}


		public async Task Identify()
		{

			try
			{
				var data = await Get<Dictionary<string, string>>("https://www.googleapis.com/oauth2/v3/userinfo");
				foreach (var pair in data)
				{
					CurrentAccount.UserData[pair.Key] = pair.Value;
				}
				SaveAccount(CurrentAccount);
				string email;
				if (!CurrentAccount.UserData.TryGetValue("email", out email))
					return;
				Settings.CurrentUserDetails = new UserDetails
				{
					Email = email,
					UserData = data,
				};
				LogManager.Shared.Identify(email,data);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		internal async Task GetAuthToken(OAuthAccount account)
		{
			try {
				var deviceID = string.IsNullOrWhiteSpace (DeviceId) ? Utility.DeviceId : DeviceId;
				var data = new Dictionary<string, string>
				{
				{"app_id", "com.google.PlayMusic"},
				{"client_id", "228293309116.apps.googleusercontent.com"},
				{"device_id", deviceID},
				{"hl", cultureShort},
				{"lib_ver", "1.0"},
				{"response_type", "token"},
				{
					"scope",
					"https://www.googleapis.com/auth/plus.me https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/skyjam"
				},
			};
				//var client = Handler == null ? new HttpClient() : new HttpClient(Handler);
				var client = new HttpClient ();
				client.DefaultRequestHeaders.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue (CurrentOAuthAccount.TokenType,
						CurrentAccount.UserData ["MasterToken"]);

				var message =
					await client.PostAsync ("https://www.googleapis.com/oauth2/v2/IssueToken", new FormUrlEncodedContent (data));
				var json = await message.Content.ReadAsStringAsync ();
				var resp = Deserialize<GoogleTokenIssueResponse> (json);
				if (resp?.Error?.Code == 400) {
					account.Token = "";
					account.RefreshToken = "";
					account.ExpiresIn = 0;
					SaveAccount (account);

				} else {
					account.Token = resp.Token;
					account.ExpiresIn = resp.ExpiresIn;
					account.Created = DateTime.UtcNow;
					await PrepareClient (Client);
				}
			} catch (Exception ex) {
				LogManager.Shared.Report (ex);
			}
		}
		public override void ResetData ()
		{
			base.ResetData ();
			ExtraData = new GoogleMusicApiExtraData ();
			DeviceId = "";
		}

	
		public class GoogleTokenIssueResponse
		{
			public class ErrorClass
			{
				public int Code { get; set; }
				public string Message { get; set; }
			}

			public ErrorClass Error { get; set; }

			public long ExpiresIn { get; set; }

			public string Token { get; set; }
		}
	}
}