using System;
using System.Net.Http;
using SimpleAuth.Providers;
using MusicPlayer;
using SimpleAuth;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Groove
{
	public class GrooveApi : MicrosoftLiveConnectApi
	{
		public GrooveApi(string identifier, HttpMessageHandler handler = null) : base(identifier,ApiConstants.OneDriveApiKey,ApiConstants.OneDriveSecret,handler:handler)
		{
			BaseAddress = new Uri("https://api.media.microsoft.com");
			Scopes = new [] {"MicrosoftMediaServices.GrooveApiAccess"};
		}

		public Task<UserSubsrciption> GetUserSubscription()
		{
			const string path = "/1/user/music/profile";
			return Get<UserSubsrciption>(path);
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
