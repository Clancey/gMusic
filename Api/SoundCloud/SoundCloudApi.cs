using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using SimpleAuth;
using System.Net;
namespace SoundCloud
{
	public class SoundCloudApi : AuthenticatedApi
	{
		public SoundCloudApi(string id, HttpMessageHandler handler = null) : base(id,MusicPlayer.ApiConstants.SoundCloudSecret , handler)
		{
			redirectUrl = "http://localhost";
			ClientId = MusicPlayer.ApiConstants.SoundCloudClientId;
			ClientSecret = MusicPlayer.ApiConstants.SoundCloudSecret;
			BaseAddress = new Uri("http://api.soundcloud.com/");
			ApiKey = MusicPlayer.ApiConstants.SoundCloudApiKey;
			AuthKey = "client_id";
			AuthLocation = AuthLocation.Query;
			//Scopes = new[] { "non-expiring" };
			//Scopes = new[] { "non-expiring" };
		}


		public string ApiKey { get; protected set; }
		public AuthLocation AuthLocation { get; protected set; }
		public string AuthKey { get; protected set; }
		string redirectUrl;
		WebAuthenticator authenticator;



		protected override T GetAccount<T>(string identifier)
		{
			try
			{
				var data = AuthStorage.GetSecured(identifier, ClientId, ClientSecret, SharedGroupAccess);

				return string.IsNullOrWhiteSpace(data) ? null : (T)(object)Deserialize<SoundCloudAccount>(data);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			return base.GetAccount<T>(identifier);
		}
		string userId;
		public async Task<string> GetUserId()
		{
			if (!string.IsNullOrWhiteSpace(userId))
				return userId;

			var user = await GetUserInfo();
			userId = user.Id;
			return userId;
		}

		public OAuthAccount CurrentOAuthAccount => CurrentAccount as OAuthAccount;

		protected override async Task<Account> PerformAuthenticate()
		{
			var account = CurrentOAuthAccount ?? GetAccount<SoundCloudAccount>(Identifier);
			//TODO: make a verification call
			if (account?.IsValid() ?? false)
			{
				try
				{
					CurrentAccount = account;
					var user = await GetUserInfo();
					return account;
				}
				catch (WebException webEx)
				{
					Console.WriteLine (webEx);
					return CurrentAccount = account;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
			
			authenticator = CreateAuthenticator();

			OAuthApi.ShowAuthenticator(authenticator);

			var token = await authenticator.GetAuthCode();
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Null token");
			}
			account = await GetAccountFromAuthCode(authenticator, Identifier);
			account.Identifier = Identifier;
			SaveAccount(account);
			CurrentAccount = account;
			return account;
		}

		protected override Task<bool> RefreshAccount(Account account)
		{
			//No need, it doesnt expire.
			return  Task.FromResult(true);
		}

		protected virtual WebAuthenticator CreateAuthenticator()
		{
			return new SoundCloudAuthenticator(redirectUrl, ClientId, ClientSecret)
			{
				Scope = new List<string> { "non-expiring" },
				Cookies = CurrentOAuthAccount?.Cookies,
			};
		}
		protected virtual async Task<OAuthAccount> GetAccountFromAuthCode(WebAuthenticator authenticator, string identifier)
		{
			var postData = await authenticator.GetTokenPostData(ClientSecret);
			var reply = await Client.PostAsync("oauth2/token", new FormUrlEncodedContent(postData));
			var resp = await reply.Content.ReadAsStringAsync();
			var result = Deserialize<OauthResponse>(resp);
			if (!string.IsNullOrEmpty(result.Error))
				throw new Exception($"{result.Error} : {result.ErrorDescription}");

			var account = new OAuthAccount()
			{
				ExpiresIn = result.ExpiresIn,
				Created = DateTime.UtcNow,
				RefreshToken = authenticator.AuthCode,
				Scope = authenticator.Scope.ToArray(),
				TokenType = result.TokenType,
				Token = result.AccessToken,
				ClientId = ClientId,
				Identifier = identifier,
				IdToken = result.Id
			};
			return account;
		}

		protected override async Task<string> PrepareUrl(string path, bool authenticated = true)
		{
			if (AuthLocation != AuthLocation.Query)
				return await base.PrepareUrl(path, authenticated);
			var newPath = await ApiKeyApi.PrepareUrl(BaseAddress, path, ApiKey, AuthKey, AuthLocation);
			if(authenticated)
				newPath = await ApiKeyApi.PrepareUrl(BaseAddress, newPath, CurrentOAuthAccount.Token, "oauth_token", AuthLocation.Query);
			return newPath;

		}


		[Path("me")]
		public async Task<UserInfo> GetUserInfo(bool forceRefresh = false)
		{
			string userInfoJson = "";
			if (!forceRefresh && (CurrentAccount?.UserData?.TryGetValue("userInfo", out userInfoJson) ?? false))
			{
				try
				{
					return Deserialize<UserInfo>(userInfoJson);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			
			}

			CurrentAccount.UserData["userInfo"] = userInfoJson = await this.Get();
			SaveAccount(CurrentAccount); 
			return Deserialize<UserInfo>(userInfoJson);
		}

		public async Task<SApiResponse<STrack>> GetFavorites(int limit = 200)
		{
			var user  = await GetUserId();
			return await GetFavorites(user,limit);
		}

		[Path("/users/{userId}/favorites")]
		public async Task<SApiResponse<STrack>> GetFavorites(string userId,int limit = 200)
		{
			var resp = await Get<SApiResponse<STrack>>(queryParameters:  new Dictionary<string, string>{
				{"userId",userId},
				{"limit",limit.ToString()},
				{"linked_partitioning","1"},
			});
			return resp;

		}
	}
}

