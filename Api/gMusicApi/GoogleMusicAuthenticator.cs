using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using SimpleAuth;
using MusicPlayer.Managers;
using SimpleAuth.Providers;

namespace MusicPlayer.Api
{
	internal class GoogleMusicAuthenticator : GoogleAuthenticator
	{
		public override string BaseUrl { get; set; } = "https://accounts.google.com/o/oauth2/programmatic_auth?";
		public override Uri RedirectUrl { get; set; } = new Uri("http://localhost");

		public string DeviceName { get; set; }
		public string Culture { get; set; }
		public string DeviceId { get; set; }

		public override async Task<Uri> GetInitialUrl()
		{
			string json = "";
			var client = new HttpClient(new ModernHttpClient.NativeMessageHandler());
			try
			{
				var form = new Dictionary<string, string>
				{
					{"chrome_installed", "false"},
					{"client_id", "228293309116.apps.googleusercontent.com"},
					{"device_id", DeviceId.Replace("ios:","")},
					{"device_name", DeviceName},
					{"hl", Culture},
					{"lib_ver", "1.0"},
					{"mediator_client_id", "936475272427.apps.googleusercontent.com"},
					{"package_name", "com.google.android.music"},
					{"redirect_uri", "com.google.sso.228293309116:/authCallback"}
				};
				var message = new FormUrlEncodedContent(form);
				message.Headers.Add("X-HTTP-Method-Override", "GET");
				var responose = await client.PostAsync("https://www.googleapis.com/oauth2/v3/authadvice", message);
				json = await responose.Content.ReadAsStringAsync();
				if (json.Contains("error") && json.Contains("invalidDeviceId"))
				{
					//Settings.DeviceId = "";
				}
				var result = await json.ToObjectAsync<OAuthRootObject>();
				if (!string.IsNullOrWhiteSpace(result.Error))
				{
					LogManager.Shared.Report(new Exception($"{result.Error} - {result.ErrorDescription}"));
				}

				return new Uri(result.Uri);
			}
			catch (Exception ex)
			{
				ex.Data["Json"] = json;
				ex.Data["Culture"] = Culture;
				ex.Data["DeviceName"] = DeviceName;
				LogManager.Shared.Report(ex);
			}
			return null;
		}

		public override bool CheckUrl(Uri url, Cookie[] cookies)
		{
			if (cookies.Length == 0)
				return false;
			var cookie =
				cookies.FirstOrDefault(x => x.Name.IndexOf("oauth_code", StringComparison.InvariantCultureIgnoreCase) == 0);
			if (string.IsNullOrWhiteSpace(cookie?.Value)) return false;

			Cookies = cookies?.Select (x => new CookieHolder { Domain =x.Domain, Path = x.Path, Name = x.Name, Value = x.Value }).ToArray ();
			FoundAuthCode(cookie.Value);
			return true;
		}

		public override async Task<Dictionary<string, string>> GetTokenPostData(string clientSecret)
		{
			var data = await base.GetTokenPostData(clientSecret);
			data.Remove ("redirect_uri");
			data["scope"] = string.Join(" ", Scope);
			return data;
		}

		public class OAuthRootObject
		{
			//public Error error { get; set; }

			public string Error { get; set; }
			[Newtonsoft.Json.JsonProperty("error_description")]
			public string ErrorDescription { get; set; }
			public string Token { get; set; }

			public long ExpiresIn { get; set; }

			public string Uri { get; set; }

			//public class Error
			//{
			//	public int Code { get; set; }
			//	public string Message { get; set; }
			//	public List<Datum> Data { get; set; } = new List<Datum>();

			//	public class Datum
			//	{
			//		public string Domain { get; set; }
			//		public string Reason { get; set; }
			//		public string Message { get; set; }
			//	}
			//}
		}
	}
}