#if !__OSX__
﻿using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;


namespace MusicPlayer.Managers
{

	internal class LogManager : ManagerBase<LogManager>
	{
		public void Report(Exception ex,
							   [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{

				Console.WriteLine(ex);
				Console.WriteLine("{0} - {1} - {2} \r\n {3}", sourceFilePath, memberName, sourceLineNumber, ex);
				Crashes.TrackError(ex, new Dictionary<string, string>{
					{"Exception",ex.Message},
					{"Method",memberName},
					{"File Name",sourceFilePath},
					{"Line Number",sourceLineNumber.ToString()},
				});
			}
			catch (Exception ex1)
			{
				Console.WriteLine(ex1);
			}
		}

		void TrackEvent(string name, Dictionary<string, string> data = null)
		{
			Task.Run(() => Analytics.TrackEvent(name, data));
		}
		void TrackEvent(string name, string key, string value)
		{
			TrackEvent(name, new Dictionary<string, string> { { key, value } });
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
				Report(ex, memberName, sourceFilePath, sourceLineNumber);
			}
			catch (Exception ex1)
			{
				Console.WriteLine(ex1);
			}
		}

		public void Log (string message, string key = null, string value = null, [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			try {
				var dictionary = new Dictionary<string, string> () {
					{"Method", memberName},
					{"File",sourceFilePath },
					{"Line number",sourceLineNumber.ToString() },
				};
				if (!string.IsNullOrWhiteSpace (key))
					dictionary [key] = value;
				TrackEvent(message, dictionary);
			} catch (Exception ex) {
				Console.WriteLine (ex);
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
				TrackEvent(message, dictionary);
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
				TrackEvent(message, dictionary);
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
					{"Song type", track.ServiceExtra2 },
					{"File Extension",track?.FileExtension ?? "NULL" },
					{"Media Type",track?.MediaType.ToString() ?? "NULL" },
					{"Service Type",track?.ServiceType.ToString() ?? "NULL" },
					{"Method", memberName},
					{"File",sourceFilePath },
					{"Line number",sourceLineNumber.ToString() },
				};
				TrackEvent(message, dictionary);
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
				TrackEvent("Failed to get playback url", dictionary);
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
				TrackEvent(message, dictionary);
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
				TrackEvent("Pressed Play");
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
				TrackEvent("Pressed Pause");
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
				TrackEvent("Pressed Next");
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
				TrackEvent("Pressed Previous");
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
				TrackEvent("Play Album", new Dictionary<string, string> {
					{ "Album", album?.Name },
					{ "Artist", album?.Artist },
					{ "Album Artist",album?.AlbumArtist },
				});
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
				TrackEvent("Artist Play", new Dictionary<string, string> { { "Artist", artist.Name } });
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
				TrackEvent("Play Song", new Dictionary<string, string>{
					{"Song",song?.ToString()},
					{"Title",song?.Name},
					{"Artist",song?.Artist},
					{"Album", song?.Album},
					{"Genre", song?.Genre},
					{"Id", song?.Id},
					{"ServiceTypes", song?.ServiceTypesString},
				});
			}
			catch (Exception ex)
			{
				Report(ex);
			}
		}

		public void Identify(string email, Dictionary<string, string> data = null)
		{
			//Ignore for now, since Mobile Center cannot identify a user
		}

		public void LogPlay(Genre genre)
		{
			try
			{
				TrackEvent("Play Genre", "Genre", genre?.Name);
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
				TrackEvent("Play Online playlist");
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
				TrackEvent("Play playlist entry");
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
				if (entry is AutoPlaylist a)
					TrackEvent("Play AutoPlaylist", "Type", a.Id);
				else
					TrackEvent( "Play Playlist");
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
				TrackEvent("Play Station", new Dictionary<string, string>{
					{"Station",station?.ToString()},
					{"Id", station?.Id},
				});
			}
			catch (Exception ex)
			{
				Report(ex);
			}
		}

		public void LogNotImplemented(Dictionary<string, string> extra, string method, string file, int lineNumber)
		{
			try
			{
				Report(new Exception($"Not Implemented Exception: {method}"), method, file, lineNumber);
			}
			catch (Exception ex)
			{
				Report(ex);
			}

		}
	}
}
#endif