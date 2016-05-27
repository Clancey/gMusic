using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleAuth;

namespace OneDrive
{
	class OneDriveAuthenticator : OAuthAuthenticator
	{
		public OneDriveAuthenticator( string clientId, string clientSecret, string redirectUrl = "http://localhost/") : base("https://login.live.com/oauth20_authorize.srf", "https://login.live.com/oauth20_token.srf", redirectUrl, clientId, clientSecret)
		{

		}

		public override Dictionary<string, string> GetInitialUrlQueryParameters()
		{
			var parameters = base.GetInitialUrlQueryParameters();
			parameters["locale"] = Thread.CurrentThread.CurrentCulture?.TwoLetterISOLanguageName;
			parameters["display"] = "touch";
			return parameters;
		}

		public override async Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			var data = await base.GetTokenPostData(clientSecret);
			data["redirect_uri"] = RedirectUrl.OriginalString;
			return data;
		}
	}
}
