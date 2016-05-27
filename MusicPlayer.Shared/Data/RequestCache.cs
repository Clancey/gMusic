using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using MusicPlayer.Api.GoogleMusic;

namespace MusicPlayer.Data
{
	class RequestCache<T>
	{
		const string CacheName = "requestCache";
		readonly string path = Path.Combine(Locations.LibDir, CacheName + typeof(T).Name);
		const int QueueLimit = 5;

		//static RequestCache<ArtistResult> artistResults;
		//public static RequestCache<ArtistResult> ArtistResults => artistResults ?? (artistResults = new RequestCache<ArtistResult>());

		static RequestCache<AlbumDataResultObject> albumResults;
		public static RequestCache<AlbumDataResultObject> AlbumResults => albumResults ?? (albumResults = new RequestCache<AlbumDataResultObject>());

		static RequestCache<SearchResultResponse> webSearchResults;
		public static RequestCache<SearchResultResponse> WebSearchResults => webSearchResults ?? (webSearchResults = new RequestCache<SearchResultResponse>());

		RequestCache()
		{
			// Load last cache
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				try
				{
					var s = File.ReadAllText(path);
					
					var cs = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, T>>(s);
					cacheResults = cs ?? new Dictionary<string, T>();
					
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}


		}
		Dictionary<string, T> cacheResults = new Dictionary<string, T>();
		void saveCache()
		{
			if (!string.IsNullOrEmpty(path))
			{
				try
				{
					var s = Newtonsoft.Json.JsonConvert.SerializeObject(cacheResults);
					if (File.Exists(path))
						File.Delete(path);
					File.WriteAllText(path,s);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}


		public void Add(string id, T result)
		{
			if (cacheResults.ContainsKey(id) || result == null)
				return;
			cacheResults.Add(id, result);
			while (cacheResults.Count > QueueLimit)
			{
				var temp = cacheResults.First();
				cacheResults.Remove(temp.Key);
			}
			saveCache();
		}
		public T Get(string id)
		{
			if (!cacheResults.ContainsKey(id))
				return default(T);
			return cacheResults[id];

		}

	}
}
