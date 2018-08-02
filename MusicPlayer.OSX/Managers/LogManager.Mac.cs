using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin;
using System.Threading.Tasks;

namespace MusicPlayer.Managers
{

	internal class LogManager : ManagerBase<LogManager>
	{
		public void Identify(string email, Dictionary<string, string> data = null)
		{
			//Xamarin.Insights.Identify(email, data);
		}
		public void Report(Exception ex,
							   [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{

				Console.WriteLine(ex);
				Console.WriteLine("{0} - {1} - {2} \r\n {3}", sourceFilePath, memberName, sourceLineNumber, ex);

				//Task.Run(()=>{
				//	Insights.Report(ex, new Dictionary<string, object>{
				//		{"Method",memberName},
				//		{"File Name",sourceFilePath},
				//		{"Line Number",sourceLineNumber},
				//	});
				//});
			}
			catch (Exception ex1)
			{
				Console.WriteLine(ex1);
			}
		}
		public void Report(MusicPlayer.Api.GoogleMusic.Error error, string requestText,
							   [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{

				if (error == null)
					return;
				var ex = new Exception(error.Message)
				{
					Data = { { "Code", error.Code }, { "request json", requestText } }
				};
				Task.Run(()=> Report(ex, memberName, sourceFilePath, sourceLineNumber));
			}
			catch (Exception ex1)
			{
				Console.WriteLine(ex1);
			}
		}


		public void Log(string message, string key = null, string value = null, [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{
				var dictionary = new Dictionary<string, string>() {
					{"Method", memberName},
					{"File",sourceFilePath },
					{"Line number",sourceLineNumber.ToString() },
				};
				if (!string.IsNullOrWhiteSpace (key))
					dictionary [key] = value;
				Console.WriteLine(message);
				//Task.Run(()=> Insights.Track(message, dictionary));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		public void Log(string message, MediaItemBase mediaItem,
							   [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{
				var dictionary = new Dictionary<string, string>() {
					{"Media ID",mediaItem?.Id ?? "NULL" },
					{"Media Title",mediaItem?.Name ?? "NULL" },
					{"Media Type",mediaItem?.GetType().Name  ?? "NULL"},
					{"Method", memberName},
					{"File",sourceFilePath },
					{"Line number",sourceLineNumber.ToString() },
				};
				//Task.Run(()=> Insights.Track(message, dictionary));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		public void Log(string message, Song song,
							   [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{
				Console.WriteLine(message);
				var dictionary = new Dictionary<string, string>() {
					{"Song ID",song?.Id  ?? "NULL" },
					{"Song Title",song?.Name  ?? "NULL"},
					{"Song Types",song?.MediaTypesString ?? "NULL" },
					{"Song Service Types",song?.ServiceTypesString  ?? "NULL"},
					{"Song Offline Count",song?.OfflineCount.ToString() ?? "NULL" },
					{"Method", memberName},
					{"File",sourceFilePath },
					{"Line number",sourceLineNumber.ToString() },
				};
				//Task.Run(()=> Insights.Track(message, dictionary));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		public void Log(string message, Track track,
							   [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{
				var dictionary = new Dictionary<string, string>() {
					{"Track Id",track?.Id ?? "NULL" },
					{"Song ID",track?.SongId ?? "NULL" },
					{"Song type", track?.ServiceExtra2 },
					{"File Extension",track?.FileExtension ?? "NULL" },
					{"Media Type",track?.MediaType.ToString() ?? "NULL" },
					{"Service Type",track?.ServiceType.ToString() ?? "NULL" },
					{"Method", memberName},
					{"File",sourceFilePath },
					{"Line number",sourceLineNumber.ToString() },
				};
				//Insights.Track(message, dictionary);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		public void GetPlaybackUrlError(string status, int tryCount, Track track,
							   [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{
				var dictionary = new Dictionary<string, string>() {
					{"Track Id",track?.Id ?? "NULL" },
					{"Song ID",track?.SongId ?? "NULL" },
					{"Song type", track?.ServiceExtra2 ?? "NULL"},
					{"Status", status },
					{"Try Count", tryCount.ToString() },
					{"File Extension",track?.FileExtension ?? "NULL" },
					{"Media Type",track?.MediaType.ToString() ?? "NULL" },
					{"Service Type",track?.ServiceType.ToString() ?? "NULL" },
					{"Method", memberName},
					{"File",sourceFilePath },
					{"Line number",sourceLineNumber.ToString() },
				};
				//Task.Run(()=> Insights.Track("Failed to get playback url", dictionary));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}


		public void Log(string message, BackgroundDownloadFile file,
							   [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{
				var dictionary = new Dictionary<string, string>() {
					{"Error",file?.Error ?? "NULL" },
					{"Track Id",file?.TrackId ?? "NULL" },
					{"Song ID",file?.Track?.SongId ?? "NULL" },
					{"Method", memberName},
					{"File",sourceFilePath },
					{"Line number",sourceLineNumber.ToString() },
				};
				//Task.Run(()=> Insights.Track(message, dictionary));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		public void PressedPlay()
		{
			try
			{
				//Task.Run(()=>Insights.Track("Pressed Play"));
			}
			catch (Exception ex)
			{
				Report(ex);
			}
		}

		public void PressPause()
		{
			try
			{
				//Task.Run(()=>{
				//	Insights.Track("Pressed Pause");
				//});
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Report(ex);
			}
		}

		public void PressNext()
		{
			try
			{
				//Task.Run(()=>Insights.Track("Pressed Next"));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Report(ex);
			}
		}

		public void PressBack()
		{
			try
			{
				//Task.Run(()=> Insights.Track("Pressed Previous"));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Report(ex);
			}
		}

		public void LogPlay(Album album)
		{
			try
			{
				//Task.Run(()=> Insights.Track("Play Album", new Dictionary<string, string> {
				//	{ "Album", album?.Name },
				//	{ "Artist", album?.Artist },
				//	{ "Album Artist",album?.AlbumArtist },
				//}));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Report(ex);
			}
		}


		public void LogPlay(Artist artist)
		{
			try
			{
				//Task.Run(()=> Insights.Track("Artist Play", new Dictionary<string, string> { { "Artist", artist.Name } }));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Report(ex);
			}
		}

		public void LogPlay(Song song)
		{
			try
			{
				//Task.Run(()=> Insights.Track("Play Song", new Dictionary<string, string>{
				//	{"Song",song?.ToString()},
				//	{"Title",song?.Name},
				//	{"Artist",song?.Artist},
				//	{"Album", song?.Album},
				//	{"Genre", song?.Genre},
				//	{"Id", song?.Id},
				//	{"ServiceTypes", song?.ServiceTypesString},
				//}));
			}
			catch (Exception ex)
			{
				Report(ex);
			}
		}
		public void LogPlay(Genre genre)
		{
			try
			{
				//Task.Run(()=> Insights.Track("Play Genre", new Dictionary<string, string>{
				//	{"Genre",genre?.Name},
				//}));
			}
			catch (Exception ex)
			{
				Report(ex);
			}
		}


		public void LogPlayback(OnlinePlaylistEntry entry)
		{
			try
			{
				//Task.Run(()=> Insights.Track("Play Online playlist", new Dictionary<string, string>
				//{
					//}));
			}
			catch (Exception ex)
			{
				Report(ex);
			}
		}

		public void LogPlayback(PlaylistSong entry)
		{
			try
			{
				//Task.Run(()=> Insights.Track("Play playlist entry", new Dictionary<string, string>
				//{
					//}));
			}
			catch (Exception ex)
			{
				Report(ex);
			}
		}

		public void LogPlayback(Playlist entry)
		{
			try
			{
				//Task.Run(()=> Insights.Track("Play Playlist", new Dictionary<string, string>
				//{
					//}));
			}
			catch (Exception ex)
			{
				Report(ex);
			}
		}

		
		public void LogPlay(RadioStation station)
		{
			try
			{
				//Task.Run(()=> Insights.Track("Play Station", new Dictionary<string, string>{
				//	{"Station",station?.ToString()},
				//	{"Id", station?.Id},
				//}));
			}
			catch (Exception ex)
			{
				Report(ex);
			}
		}

		public void LogNotImplemented(Dictionary<string,string> extra,string method, string file, int lineNumber)
		{
			try
			{
				extra = extra ?? new Dictionary<string, string>();
				extra["Method"] = method;
				extra["File"] = file;
				extra["Line number"] = lineNumber.ToString();
				//Task.Run(() =>
				//	{
				//	Insights.Report(new Exception($"Not Implemented Exception: {method}"),extra,Insights.Severity.Error);
				//});
			}
			catch (Exception ex)
			{
				Report(ex);
			}

		}
	}
}

