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
using MusicPlayer.Api;
using MusicPlayer.Api.GoogleMusic;
using Plugin.Connectivity;

namespace YoutubeApi
{
	public class YoutubeApiExtraData
	{
		[JsonProperty ("lst")]
		public string LastSongSyncTag { get; set; }

		[JsonProperty ("lpt")]
		public string LastPlaylistTag { get; set; }

		[JsonProperty ("plid")]
		public string PlaylistId {get;set;}
	}

	public class YoutubeOauthApi : OauthApiKeyApi
	{
		public override string ExtraDataString {
			get { return ExtraData?.ToJson () ?? ""; }
			set {
				extraDataString = value;
				ExtraData = !string.IsNullOrWhiteSpace (value)
					? value.ToObject<YoutubeApiExtraData> ()
					: new YoutubeApiExtraData ();
			}
		}

		public YoutubeApiExtraData ExtraData { get; set; } = new YoutubeApiExtraData();


		protected override async Task<Account> PerformAuthenticate ()
		{
			if (!CrossConnectivity.Current.IsConnected)
				return null;
			if (Identifier == YoutubeProvider.DefaultId)
				return null;
			var account = await CheckForGoogleAccount ();
			if (account != null) {
				currentAccount = account;
				await RefreshAccount(account);
				CurrentAccount = account;
				return account;
			}

			return await base.PerformAuthenticate ();
		}

		async Task<Account> CheckForGoogleAccount ()
		{
			var account = CurrentOAuthAccount ?? GetAccount<OAuthAccount> (Identifier);
			if (account != null) {
				if(string.IsNullOrWhiteSpace(GetAccountParentId (account)))
					return null;
				return account;
			}
			var api = ApiManager.Shared.GetMusicProvider (ServiceType.Google);
			var googleId = api?.Id;
			if (string.IsNullOrWhiteSpace (googleId))
				return null;
			account = GetAccount<OAuthAccount> (googleId);
			if (account == null)
				return null;
			
			currentAccount = new OAuthAccount () {
				ClientId = ClientId,
				Identifier = Identifier,
				TokenType = account.TokenType,
				RefreshToken = "NA",
				UserData = new Dictionary<string, string>{
					{"ParentId", api.Id},
				}
			};
			return currentAccount;
		}

		string cultureShort = "en";
		string cultureLong = "en_US";
		string sessionId = Guid.NewGuid ().ToString ();
		string extraDataString;

		public YoutubeOauthApi (string id, HttpMessageHandler handler = null)
			: base (id, MusicPlayer.ApiConstants.YouTubeApiKey, "key", AuthLocation.Query, new GoogleMusicAuthenticator { ClientId = MusicPlayer.ApiConstants.YouTubeClientId }, handler)
		{
			BaseAddress = new Uri ("https://www.googleapis.com/youtube/v3/");
			AutoAuthenticate = true;
			ClientId = MusicPlayer.ApiConstants.YouTubeClientId;
			ClientSecret = MusicPlayer.ApiConstants.YouTubeClientSecret;
			Scopes = new[] { "https://www.google.com/accounts/OAuthLogin",
				"https://www.googleapis.com/auth/userinfo.email",
				"https://www.googleapis.com/auth/youtube",
				"https://www.googleapis.com/auth/youtube.force-ssl",
				"https://gdata.youtube.com",
				"https://www.googleapis.com/auth/plus.stream.read",
				"https://www.googleapis.com/auth/plus.stream.write",
				"https://www.googleapis.com/auth/plus.stream.moderate",
				"https://www.googleapis.com/auth/plus.stream.write",
//				"https://www.googleapis.com/auth/plus.circles.read",
//				"https://www.googleapis.com/auth/plus.circles.write",
//				"https://www.googleapis.com/auth/plus.me",
				"https://www.googleapis.com/auth/picasa",
				"https://www.googleapis.com/auth/plus.media.upload",
				"https://www.googleapis.com/auth/plus.settings",
				"https://www.googleapis.com/auth/plus.pages.manage",
				"https://www.googleapis.com/auth/identity.plus.page.impersonation",
				"https://www.googleapis.com/auth/supportcontent" };
			ForceRefresh = true;
			CrossConnectivity.Current.ConnectivityChanged += (sender, args) => {
				if (CrossConnectivity.Current.IsConnected && this.CurrentAccount == null) {
					Authenticate ();
				}
			};
			TokenUrl = "https://accounts.google.com/o/oauth2/token";
			try {
				var culture = Thread.CurrentThread.CurrentCulture;
				cultureShort = culture.TwoLetterISOLanguageName;
				cultureLong = culture.Name.Replace ("-", "_");
			} catch (Exception ex) {
				Debug.WriteLine (ex);
				//				Logger.Log(ex);
				cultureShort = "en";
				cultureLong = "en_US";
			}
		}

		public string Email {
			get {
				string email = "";
				CurrentOAuthAccount?.UserData?.TryGetValue ("email", out email);
				if (string.IsNullOrWhiteSpace (email)){
					var parentID = GetAccountParentId (CurrentOAuthAccount);
					if (!string.IsNullOrWhiteSpace (parentID)) {
						return ApiManager.Shared.GetMusicProvider (parentID)?.Email;
					}
				}
				return email;
			}
		}
		protected override WebAuthenticator CreateAuthenticator ()
		{
			return new GoogleMusicAuthenticator {
				Scope = Scopes.ToList (),
				ClientId = ClientId,
				DeviceName = Utility.DeviceName,
				DeviceId = Utility.DeviceId,
				Culture = cultureShort,
				ClearCookiesBeforeLogin = CalledReset,
			};
		}

		protected override async Task<OAuthAccount> GetAccountFromAuthCode (WebAuthenticator authenticator, string identifier)
		{
			var account = await base.GetAccountFromAuthCode (authenticator, identifier);
			account.UserData ["MasterToken"] = account.Token;
			account.UserData ["MasterRefreshToken"] = account.RefreshToken;
			SaveAccount (account);
			//Google now gets the real token from the master token.
			account.Identifier = identifier;
			currentAccount = account;
			await PrepareClient (Client);
			await GetAuthToken (account);
			return account;
		}


		Task<bool> refreshTask;
		object locker = new object ();

		protected override async Task<bool> RefreshAccount (Account account)
		{
			if (!CrossConnectivity.Current.IsConnected)
				return false;
			lock (locker) {
				if (refreshTask == null || refreshTask.IsCompleted)
					refreshTask = refreshAccount (account);
			}
			return await refreshTask;
		}

		protected async Task<bool> refreshAccount (Account account)
		{
			var parentId = GetAccountParentId (account);
			if (!string.IsNullOrWhiteSpace (parentId)) {
				return await RefreshUsingGoogleMusic (account, parentId);
			}
			if (!await base.RefreshAccount (account))
				return false;
			account.UserData ["MasterToken"] = (account as OAuthAccount)?.Token;
			await GetAuthToken (account as OAuthAccount);
			if (!account.IsValid ()) {
				if (await Authenticate () == null)
					return false;
			}
			await PrepareClient (Client);
			SaveAccount (account);
			return true;
		}

		string GetAccountParentId(Account account)
		{
			string parentID = null;
			account.UserData?.TryGetValue ("ParentId", out parentID);
			return parentID;
		}

		async Task<bool> RefreshUsingGoogleMusic(Account account,string parentId)
		{
			var api = ApiManager.Shared.GetMusicProvider<GoogleMusicProvider> (parentId);
			var gAccount = api.Api.CurrentAccount;
			if(gAccount == null)
				gAccount = await api.Api.Authenticate();
			if (gAccount == null)
				return false;
			if (!gAccount.IsValid ())
				await api.Api.RefreshYoutubeAccount (gAccount);
			account.UserData ["MasterToken"] = gAccount.UserData ["MasterToken"];
			await GetAuthToken (account as OAuthAccount);

			if (!account.IsValid ()) {
				if (await Authenticate () == null)
					return false;
			}
			await PrepareClient (Client);
			SaveAccount (account);
			return true;
		}

		internal async Task GetAuthToken (OAuthAccount account)
		{
			try{
				var deviceId = Utility.DeviceId;

				var data = new Dictionary<string, string> {
					{ "app_id", "com.google.ios.youtube" },
					{ "client_id", "755541669657-kbosfavg7pk7sr3849c3tf657hpi5jpd.apps.googleusercontent.com" },
					{ "device_id", deviceId },
					{ "hl", cultureShort },
					{ "lib_ver", "1.0" },
					{ "response_type", "token" }, {
						"scope",
						"https://www.googleapis.com/auth/youtube https://www.googleapis.com/auth/youtube.force-ssl https://gdata.youtube.com https://www.googleapis.com/auth/plus.stream.read https://www.googleapis.com/auth/plus.stream.write https://www.googleapis.com/auth/plus.stream.moderate https://www.googleapis.com/auth/plus.stream.write https://www.googleapis.com/auth/plus.circles.read https://www.googleapis.com/auth/plus.circles.write https://www.googleapis.com/auth/plus.me https://www.googleapis.com/auth/picasa https://www.googleapis.com/auth/plus.media.upload https://www.googleapis.com/auth/plus.settings https://www.googleapis.com/auth/plus.pages.manage https://www.google.com/accounts/OAuthLogin https://www.googleapis.com/auth/identity.plus.page.impersonation https://www.googleapis.com/auth/supportcontent"
					},
				};
				//var client = Handler == null ? new HttpClient() : new HttpClient(Handler);
				var client = new HttpClient (new ModernHttpClient.NativeMessageHandler ());
				client.DefaultRequestHeaders.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue (account.TokenType,
						account.UserData ["MasterToken"]);

				var message =
					await client.PostAsync ("https://www.googleapis.com/oauth2/v2/IssueToken", new FormUrlEncodedContent (data));
				var json = await message.Content.ReadAsStringAsync ();
				var resp = Deserialize<MusicPlayer.Api.GoogleMusic.GoogleMusicApi.GoogleTokenIssueResponse> (json);
				if (resp?.Error?.Code == 400) {
					account.Token = "";
					account.RefreshToken = "";
					account.ExpiresIn = 0;

				} else {
					account.Token = resp.Token;
					account.ExpiresIn = resp.ExpiresIn;
					account.Created = DateTime.UtcNow;
					await PrepareClient (Client);
				}
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}
		}
	}
}

