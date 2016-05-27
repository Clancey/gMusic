using System;
using SimpleAuth;
using System.Threading.Tasks;
using System.Web;
using System.Linq;

namespace SoundCloud
{
	public class SoundCloudAuthenticator : OAuthAuthenticator
	{
		public SoundCloudAuthenticator(string redirectUrl, string clientId, string clientSecret)
		{
			this.ClientId = clientId;
			this.ClientSecret = clientSecret;
			RedirectUrl = new Uri(redirectUrl);
			TokenUrl = "https://api.soundcloud.com/oauth2/token";
			BaseUrl = "https://soundcloud.com/connect";
		}
		public override async Task<Uri> GetInitialUrl()
		{
			var scope = string.Join("%20", Scope.Select(HttpUtility.UrlEncode));

			return new Uri($"{BaseUrl}?display=popup&scope={scope}&response_type=code&client_id={ClientId}&redirect_uri={RedirectUrl.AbsoluteUri}");
		}

		public override async Task<System.Collections.Generic.Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			var tokens = await base.GetTokenPostData(clientSecret);
			tokens["redirect_uri"] = RedirectUrl.AbsoluteUri;
			return tokens;
		}

	}
}

