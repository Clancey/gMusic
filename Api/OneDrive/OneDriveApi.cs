using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using SimpleAuth;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MusicPlayer.Data;
using MusicPlayer.Managers;

namespace OneDrive
{

	public class OneDriveApiExtraData
	{
		[JsonPropertyAttribute("ls")]
		public string LastSongSync { get; set; }

		//[JsonProperty("lp")]
		//public long LastPlaylistSync { get; set; }

		//[JsonProperty("lps")]
		//public long LastPlaylistSongSync { get; set; }

		//[JsonProperty("lr")]
		//public long LastRadioSync { get; set; }

		//[JsonProperty("lg")]
		//public long LastGenreSync { get; set; }

		//[JsonProperty("hgdid")]
		//public bool HasGeneratedDeviceId { get; set; }

		//[JsonProperty("gdid")]
		//public string GeneratedDeviceId { get; set; }
	}
	public class OneDriveApi : OAuthApi
	{

		public override string ExtraDataString
		{
			get { return ExtraData?.ToJson() ?? ""; }
			set
			{
				base.ExtraDataString = value;
				ExtraData = !string.IsNullOrWhiteSpace(value)
				                   ? value.ToObject<OneDriveApiExtraData>()
				                   : new OneDriveApiExtraData();
			}
		}

		public OneDriveApiExtraData ExtraData { get; set; } = new OneDriveApiExtraData();
		public OneDriveApi(string id, HttpMessageHandler handler = null)
			: base(id, MusicPlayer.ApiConstants.OneDriveApiKey, MusicPlayer.ApiConstants.OneDriveSecret, handler)
		{
			TokenUrl = "https://login.live.com/oauth20_token.srf";
			Scopes = new []
			{
				"wl.basic",
				"wl.skydrive",
				"onedrive.readwrite",
				"wl.offline_access",
			};
			BaseAddress = new Uri("https://api.onedrive.com/v1.0/");
			this.EnsureApiStatusCode = false;
		}

		protected OneDriveApi(string identifier, string clientId, string clientSecret, HttpMessageHandler handler = null) : base(identifier, clientId, clientSecret, handler)
		{

		}

		protected override WebAuthenticator CreateAuthenticator()
		{
			return new OneDriveAuthenticator(ClientId,ClientSecret)
			{
				Scope = Scopes.ToList(),
			};
		}

		public override void ResetData()
		{
			base.ResetData();
			ExtraData = new OneDriveApiExtraData ();
		}

		public async Task<string> GetDrive()
		{
			var resp = await Get();
			return resp;
		}


		[Path("drive")]
		public async Task<string> GetAudio()
		{
			var resp = await Get();
			return resp;
		}

		[Path("drive/special/{folder}/view.delta")]
		public async Task<OneDriveDeltaResponse> GetSpecialFolderDelta(string folder,int pageCount = 0,string token = "")
		{
			var queryParams = new Dictionary<string, string>
			{
				{"folder",folder},
			};
			if (pageCount > 0)
				queryParams.Add("top", pageCount.ToString());
			if (!string.IsNullOrWhiteSpace(token) && token != "0")
				queryParams.Add("token", token);
			var resp = await Get<OneDriveDeltaResponse>(queryParameters:queryParams);
			return resp;
		}


		public async Task Identify()
		{

			try
			{
				var data = await Get<Dictionary<string, string>>("https://apis.live.net/v5.0/me");
				foreach (var pair in data)
				{
					CurrentAccount.UserData[pair.Key] = pair.Value;
				}
				SaveAccount(CurrentAccount);
				string email;
				if (!CurrentAccount.UserData.TryGetValue("email", out email))
					return;
				//Settings.CurrentUserDetails = new UserDetails
				//{
				//	Email = email,
				//	UserData = data,
				//};
				//Xamarin.Insights.Identify(email,data);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		[Path("/drive/items/{itemId}/content")]
		public async Task<string> GetItemDownloadUrl(string itemId)
		{	
			var queryParams = new Dictionary<string, string>
			{
				{"itemId",itemId},
			};
			var url = await GetRedirectUrl(HttpMethod.Get, queryParameters:queryParams);
			return url;
		}

		[Path("/drive/items/{itemId}/action.createLink")]
		public async Task<string> GetShareUrl(string itemId)
		{
			var queryParams = new Dictionary<string, string>
			{
				{"itemId",itemId},
			};
			var resp = await Post<OneDriveShareResponse>(new { type = "view" },queryParameters:queryParams);
			return resp?.Link?.WebUrl;
		}

		public virtual async Task<string> GetRedirectUrl(HttpMethod method, string path = null, HttpContent content = null, Dictionary<string, string> queryParameters = null, Dictionary<string, string> headers = null, bool authenticated = true, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				path = GetType().GetMethods().Where(x => x.Name == methodName).Select(x => GetValueFromAttribute<PathAttribute>(x)).Where(x => !string.IsNullOrWhiteSpace(x)).FirstOrDefault();
			}

			if (string.IsNullOrWhiteSpace(path))
				throw new Exception("Missing Path Attribute");

			if (queryParameters != null)
				path = CombineUrl(path, queryParameters);

			//Merge attributes with passed in headers.
			//Passed in headers overwrite attributes
			var attributeHeaders = GetType().GetMethods().Where(x => x.Name == methodName).Select(x => GetHeadersFromMethod(x)).Where(x => x?.Any() ?? false).FirstOrDefault();
			if (attributeHeaders?.Any() ?? false)
			{
				if (headers != null)
					foreach (var header in headers)
					{
						attributeHeaders[header.Key] = header.Value;
					}
				headers = attributeHeaders;
			}


			var message = await SendMessage(path, content, method, headers, authenticated,completionOption: HttpCompletionOption.ResponseHeadersRead);
			if (EnsureApiStatusCode)
				message.EnsureSuccessStatusCode();
			
			string responseUri = message.RequestMessage.RequestUri.ToString();
			return responseUri;
		}
	}
}
