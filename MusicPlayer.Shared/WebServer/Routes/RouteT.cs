using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MusicPlayer.Server
{
	public abstract class Route<T> : Route
	{
		public abstract Task<T> GetResponse (string method, System.Net.HttpListenerRequest request, NameValueCollection queryString, string data);

		public override async Task<string> GetResponseString (string method, System.Net.HttpListenerRequest request, NameValueCollection queryString, string data)
		{
			var item = await GetResponse (method, request, queryString, data);
			return await Task.Run (() => JsonConvert.SerializeObject (item, new JsonSerializerSettings {
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			}));
		}

	}
}

