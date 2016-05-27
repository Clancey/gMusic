using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Api;
using SimpleAuth;

namespace MusicPlayer.Api
{
	internal class AmazonAuthenticator : WebAuthenticator
	{
		public override string BaseUrl { get; set; } = "https://www.amazon.com/ap/oa?";
		public override Uri RedirectUrl { get; set; } = new Uri("http://localhost");

		public override async Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			var data = await base.GetTokenPostData(clientSecret);
			data["redirect_uri"] = RedirectUrl.AbsoluteUri;
			return data;
		}
	}
}