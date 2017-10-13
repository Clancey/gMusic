using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicPlayer.Server
{
	public abstract class Route
	{
		public virtual bool IsSecured { get; set; } = true;
		public string Path { get; set; }
		public static void Enable<T>(WebServer server)
		{
			var type = typeof(T);
			var path = type.GetCustomAttributes(true).OfType<PathAttribute>().FirstOrDefault();
			if (path == null)
				throw new Exception("Cannot automatically regiseter Route without Path attribute");
			server.Router.AddRoute(path.Path, type); 
		}
		public abstract bool SupportsMethod (string method);

		public virtual string ContentType {
			get {
				return "application/json";
			}
		}

		public virtual Task<string> GetResponseString (string method, HttpListenerRequest request, NameValueCollection queryString, string data)
		{
			throw new Exception ("You need to provide either GetResponseString or GetResponseBytes");
		}

		public virtual Task<string> GetResponseString (HttpListenerRequest request)
		{
			return Task.FromResult<string>(null);
		}
		internal HttpListenerRequest Request;
		public virtual async Task<byte[]> GetResponseBytes (HttpListenerRequest request)
		{
			Request = request;
			var responseString = await GetResponseString (request);
			if (responseString != null) {
				return Encoding.UTF8.GetBytes (responseString);
			}

			var method = request.HttpMethod;
			string data;
			using (var reader = new StreamReader (request.InputStream))
				data = reader.ReadToEnd ();

			var queryParams = request.QueryString;
			var path = queryParams.Count == 0 ? request.Url.PathAndQuery : request.Url.PathAndQuery.Replace(request.Url.Query, "");
			var valuesFromPath = GetValuesFromPath(Path, path);
			if (valuesFromPath != null)
				foreach (var val in valuesFromPath)
					queryParams.Add(val.Key, val.Value);

			var responseData = await GetResponseBytes (method, request,queryParams, data);
			if (responseData != null)
				return responseData;
			

			responseString = await GetResponseString (method, request ,queryParams,data);
			return Encoding.UTF8.GetBytes (responseString);
		}
		public virtual async Task ProcessReponse (HttpListenerContext context)
		{
			try {
				byte [] buf = await GetResponseBytes (context.Request);
				if (buf != null) {
					context.Response.ContentType = ContentType;
					context.Response.ContentLength64 = buf.Length;
					context.Response.OutputStream.Write (buf, 0, buf.Length);
					context.Response.StatusCode = 200;
				}
			} catch (Exception e) {
				Console.WriteLine (e);
				context.Response.StatusCode = 500;
			} // suppress any exceptions
		}

		public static Dictionary<string, string> GetValuesFromPath (string path, string currentPath)
		{
			if (!path.Contains ("{"))
				return null;
			var parts = path.Split ('/');
			var indecies = new List<int> ();
			for (var i = 0; i < parts.Length; i++) {
				var part = parts [i];
				if (!part.StartsWith ("{") || !part.EndsWith ("}"))
					continue;
				indecies.Add (i);
			}
			if (indecies.Count == 0)
				return null;
			var valueParts = currentPath.Split (new [] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			var returnDictionary = new Dictionary<string, string> ();
			foreach (var i in indecies) {
				var key = parts [i].Trim ('{', '}');
				var value = valueParts [i];
				returnDictionary [key] = value;
			}
			return returnDictionary;

		}

		public virtual Task<byte[]> GetResponseBytes (string method,HttpListenerRequest request, NameValueCollection queryString, string data)
		{
			return Task.FromResult<byte[]>(null);
		}

		public virtual Task<bool> CheckAuthentication (HttpListenerContext context)
		{
			
			if (!IsSecured)
				return Task.FromResult(true);

			//TODO: add Authentication
			return Task.FromResult(true);
			//try {
			//	string inKey = null;
			//	var apikey = Settings.GetSecretString (null,"NodeServerApiKey");
			//	var header = context.Request.Headers.AllKeys.FirstOrDefault (x => x.ToLower () == "apikey");
			//	if (!string.IsNullOrWhiteSpace (header))
			//		inKey = context.Request.Headers [header];
			//	else {
			//		var key = context.Request.QueryString.AllKeys.FirstOrDefault (x => x.ToLower () == "apikey");
			//		if (string.IsNullOrWhiteSpace (key))
			//			return Task.FromResult (false);

			//		inKey = context.Request.QueryString [key];
			//	}
			//	return Task.FromResult (apikey == inKey);
			//} catch (Exception ex) {
			//	Console.WriteLine (ex);
			//}
			//return Task.FromResult (false);

		}

	}
}

