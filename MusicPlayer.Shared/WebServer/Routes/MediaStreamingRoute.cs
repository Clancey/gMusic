using System;
using MusicPlayer.Managers;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
namespace MusicPlayer.Server
{

	[Path("api/GetMediaStream/Playback")]
	public class MediaStreamingRoute : Route
	{
		string contentType = "audio/mpeg";
		public override string ContentType => contentType;

		public override bool SupportsMethod(string method) => method == "GET";

	
		const string RangeKey = "Range";
		public override async Task ProcessReponse(System.Net.HttpListenerContext context)
		{

			try
			{
				var request = context.Request;
				//Console.WriteLine(request.Url);
				//Console.WriteLine("Request Headers");
				//foreach (var hk in request.Headers.AllKeys)
				//{
				//	Console.WriteLine($"{hk} - {request.Headers[hk]}");
				//}

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
							queryParams.Add(val.Key,val.Value);
					}
				}


				var resp = context.Response;
				resp.Headers.Add("Accept-Ranges", "bytes");

				var songId = queryParams["SongId"];
				var playbackData =  await PlaybackManager.Shared.NativePlayer.GetPlaybackDataForWebServer(songId);
				var currentDownloadHelper = playbackData.DownloadHelper;
				if (string.IsNullOrWhiteSpace(currentDownloadHelper.MimeType))
				{
					var success = await currentDownloadHelper.WaitForMimeType();
				}
				resp.ContentType = currentDownloadHelper.MimeType;

				if (request.HasRange())
				{
					var range = request.GetRange();

					var length = range.End - range.Start + 1;

					resp.StatusCode = 206;
					resp.StatusDescription = "Partial Content";

					resp.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", range.Start, range.End, currentDownloadHelper.TotalLength));

					Console.WriteLine($"Range : {range.Start} - {range.End}");

					currentDownloadHelper.Seek(range.Start, SeekOrigin.Begin);

					var bytes = new byte[length];
					var readBytes = await currentDownloadHelper.ReadAsync(bytes, 0, (int)length);
					resp.ContentLength64 = readBytes;
					if (resp.OutputStream.CanWrite)
						await resp.OutputStream.WriteAsync(bytes, 0, readBytes);
					//resp.StatusCode = 200;

				}
				else
				{
					resp.StatusCode = 200;
					resp.ContentLength64 = currentDownloadHelper.TotalLength;
					if (resp.OutputStream.CanWrite)
						currentDownloadHelper.CopyTo(context.Response.OutputStream);
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				try
				{
					context.Response.StatusCode = 500;
				}
				catch(Exception)
				{

				}
			} // suppress any exceptions

			//return base.ProcessReponse(context);
		}


	}
}
