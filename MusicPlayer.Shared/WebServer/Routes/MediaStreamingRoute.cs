using System;
using MusicPlayer.Managers;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
namespace MusicPlayer.Server
{

	[Path("api/GetMediaStream/{TrackId}")]
	public class MediaStreamingRoute : Route
	{
		string contentType = "audio/mpeg";
		public override string ContentType => contentType;

		public override bool SupportsMethod(string method) => method == "GET";

		public override async System.Threading.Tasks.Task<byte[]> GetResponseBytes(string method, System.Net.HttpListenerRequest request, System.Collections.Specialized.NameValueCollection queryString, string data)
		{
			Console.WriteLine(request.Url);
			Console.WriteLine("Request Headers");
			foreach (var hk in request.Headers.AllKeys)
			{
				if(hk != null)
					Console.WriteLine($"{request.Headers[hk]}");
			}
			var SongId = queryString["SongId"];
			var playbackData = PlaybackManager.Shared.NativePlayer.GetPlaybackData(SongId, false);
			var currentDownloadHelper = playbackData.DownloadHelper;
			if (string.IsNullOrWhiteSpace(currentDownloadHelper.MimeType))
			{
				var success = await currentDownloadHelper.WaitForMimeType();
			}
			contentType = currentDownloadHelper.MimeType;
			return new byte[0];
		}
		const string RangeKey = "Range";
		public override async Task ProcessReponse(System.Net.HttpListenerContext context)
		{

			try
			{
				var request = context.Request;
				Console.WriteLine(request.Url);
				Console.WriteLine("Request Headers");
				bool hasRange = false;
				foreach (var hk in request.Headers.AllKeys)
				{
					if (hk == RangeKey)
					{
						hasRange = true;
					}
					Console.WriteLine($"{hk} - {request.Headers[hk]}");
				}

				var method = request.HttpMethod;
				string data;
				using (var reader = new StreamReader(request.InputStream))
					data = reader.ReadToEnd();

				var queryParams = request.QueryString;
				var path = queryParams.Count == 0 ? request.Url.PathAndQuery : request.Url.PathAndQuery.Replace(request.Url.Query, "");
				var valuesFromPath = GetValuesFromPath(Path, path);
				if (valuesFromPath != null)
				{
					foreach (var val in valuesFromPath)
					{
						if (val.Key != null)
							queryParams.Add(val.Key, val.Value);
					}
				}


				var id = queryParams["TrackId"];
				var songId = PlaybackManager.Shared.NativePlayer.SongIdTracks[id];
				var playbackData = PlaybackManager.Shared.NativePlayer.GetPlaybackData(songId, false);
				var currentDownloadHelper = playbackData.DownloadHelper;
				if (string.IsNullOrWhiteSpace(currentDownloadHelper.MimeType))
				{
					var success = await currentDownloadHelper.WaitForMimeType();
				}
				var resp = context.Response;
				resp.Headers.Add("Accept-Ranges", "bytes");
				resp.ContentType = ContentType;

				if (hasRange)
				{
					var range = context.Request.GetRange();

					var length = range.End - range.Start + 1;
					resp.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", range.Start, range.End, currentDownloadHelper.TotalLength));

        			resp.StatusCode = 206;
					resp.StatusDescription = "Partial Content";
					Console.WriteLine($"Range : {range.Start} - {range.End}");

					currentDownloadHelper.Seek(range.Start, SeekOrigin.Begin);

					var bytes = new byte[length];
					var readBytes = await currentDownloadHelper.ReadAsync(bytes, 0, (int)length);
					resp.ContentLength64 = readBytes;
					await context.Response.OutputStream.WriteAsync(bytes, 0, readBytes);
					//resp.StatusCode = 200;

				}
				else
				{
					resp.ContentLength64 = currentDownloadHelper.TotalLength;
					if (context.Response.OutputStream.CanWrite)
						currentDownloadHelper.CopyTo(context.Response.OutputStream);
					resp.StatusCode = 200;
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				context.Response.StatusCode = 500;
			} // suppress any exceptions

			//return base.ProcessReponse(context);
		}


	}
}
