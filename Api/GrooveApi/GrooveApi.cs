using System;
using System.Net.Http;
using SimpleAuth.Providers;
using MusicPlayer;
using SimpleAuth;
using System.Threading.Tasks;
using System.Collections.Generic;
using MusicPlayer.Managers;
using Newtonsoft.Json;

namespace Groove
{
	public class GrooveApiExtraData
	{
		string generatedDeviceId;

		[JsonProperty("gdid")]
		public string GeneratedDeviceId
		{
			get { return generatedDeviceId ?? (generatedDeviceId = Guid.NewGuid().ToString()); }
			set { generatedDeviceId = value; }
		}
	}
	public class GrooveApi : MicrosoftLiveConnectApi
	{
		public GrooveApi(string identifier, HttpMessageHandler handler = null) : base(identifier,ApiConstants.OneDriveApiKey,ApiConstants.OneDriveSecret,handler:handler)
		{
			BaseAddress = new Uri("https://api.media.microsoft.com/1/");
			Scopes = new [] {
				"MicrosoftMediaServices.GrooveApiAccess",
				"wl.offline_access",
				"wl.basic",
			};
		}

		public Task<UserSubsrciption> GetUserSubscription()
		{
			const string path = "/user/music/profile";
			return Get<UserSubsrciption>(path);
		}

		public GrooveApiExtraData ExtraData { get; set; } = new GrooveApiExtraData();
		public override string ExtraDataString
		{
			get { return ExtraData?.ToJson() ?? ""; }
			set
			{
				ExtraData = !string.IsNullOrWhiteSpace(value)
					? value.ToObject<GrooveApiExtraData>()
					: new GrooveApiExtraData();
			}
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
				LogManager.Shared.Identify(email,data);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		public Task<TrackActionResponse> AddToLibrary(params string[] ids)
		{
			const string path = "content/music/collection/add";
			return Post<TrackActionResponse>(new TrackActionRequest { TrackIds = ids },path);

		}

		public Task<StreamResponse> GetFullTrackStream(string id)
		{
			const string path = "/content/{id}/stream?clientInstanceId={clientInstanceId}";
			var queryParams = new Dictionary<string, string>
			{
				{"id" ,id },
				{"clientInstanceId",ExtraData.GeneratedDeviceId}
			};
			return Get<StreamResponse>(path, queryParams);
		}

		public Task<ContentResponse> BrowseUserCollection(GrooveNamespace theNamespace, GrooveTypes type, string orderBy = null, int maxItems = 25,int page = 0, string continuationToken = null,string jsonp = null)
		{
			const string path = "/content/{namespace}/collection/{type}/browse";
			var queryParams = new Dictionary<string, string>
			{
				{"namespace", theNamespace.GetEnumMember()},
				{"type",type.GetEnumMember()}
			};

			if (!string.IsNullOrEmpty(orderBy))
				queryParams["orderBy"] = orderBy;

			if (maxItems > 25 || maxItems < 0)
				throw new ArgumentException("OrderBy must be between 0 and 25");

			if (page > 0)
				queryParams["page"] = page.ToString();

			if (!string.IsNullOrWhiteSpace(continuationToken))
				queryParams["continuationToken"] = continuationToken;

			if (!string.IsNullOrWhiteSpace(jsonp))
				queryParams["jsonp"] = jsonp;

			return Get<ContentResponse>(path, queryParams);

		}

	}

	public class GooveAuthenticator : OAuthAuthenticator
	{
		public override Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			return base.GetTokenPostData(clientSecret);
		}
	}

}
