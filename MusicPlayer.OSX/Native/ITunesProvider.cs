using System;
using MusicPlayer.Api;
using System.IO;
using System.Threading.Tasks;
using MusicPlayer.Data;
using ITunesLibraryParser;
using System.Linq;
using MusicPlayer.Managers;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class ITunesProvider : MusicProvider
	{
		public ITunesProvider () : base(null)
		{
		}
		public override MediaProviderCapabilities[] Capabilities {
			get {
				return new[]{ MediaProviderCapabilities.None};
			}
		}
		public bool Disabled { get; set; }

		#region implemented abstract members of MusicProvider

		static string[] validKinds = new string[] {
			//"DuplicateItem",
			//"iPhone/iPod touch/iPad app",
			//"iPhone/iPod touch app",
			//"iPad app",
			"MPEG audio file",
			//"Matched AAC audio file",
			//"Purchased AAC audio file",
			"AAC audio file",
			"MPEG-4 video file",
			"Internet audio stream",
			"WAV audio file",
			//"Purchased book",
			//"Protected book",
			//"PDF document",
			"MPEG-4 audio file",
			//"Purchased MPEG-4 video file",

		};
		public override string Email
		{
			get
			{
				return "NA";
			}
		}

		static MediaType FromKind (string kind)
		{
			switch (kind) {
			case "MPEG-4 video file":
				return MediaType.Video;
			default:
				return MediaType.Audio;
			}
		}

		static bool FileUriExists (ItunesTrack track)
		{
			var url = track?.Location;
			if (string.IsNullOrWhiteSpace (url))
				return false;
			try {
				var uri = new Uri (url);
				if (!uri.IsFile)
					return true;
				var file = uri.LocalPath;
				if (File.Exists (file))
					return true;

				Console.WriteLine (url);
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return File.Exists (url);
			}
			return false;
		}

		static string CreateFilePath (ItunesTrack track)
		{
			//var path = Path.Combine(musicPath,track.Artist,track.Album,track.TrackNumbe
			return "";
		}
		public override async Task<string> GetShareUrl (Song song)
		{
			return null;
		}

		static readonly string musicPath = System.Environment.GetFolderPath (Environment.SpecialFolder.MyMusic);

		protected override async Task<bool> Sync ()
		{
			var itunesPath = Path.Combine (musicPath, "iTunes", "iTunes Music Library.xml");
			if (!File.Exists (itunesPath))
				return false;

			if (Disabled)
				return true;
			return await Task.Run (async () => {
				try {
					var start = DateTime.Now;
					await Database.Main.ExecuteAsync ("update Track set Deleted = 1 where ServiceId = ?", Id);
//					var lib = new ITunesLibrary ();
//					var tracks = lib.Parse (itunesPath);
//					Console.WriteLine (tracks.Count ());
//
//					Console.WriteLine ("Loading Library took : {0}", (DateTime.Now - start).TotalMilliseconds);
//					var lastTime = DateTime.Now;
//					var items = tracks.Where (x => validKinds.Contains (x.Kind) && FileUriExists (x)).Select (x => new FullTrackData (x.Name, x.Artist, x.AlbumArtist, x.Album, x.Genre) {
//						Id = x.TrackId.ToString (),
//						Disc = x.DiscNumber ?? 0,
////						Duration = x.
//						FileLocation = x.Location,
//						FileExtension = Path.GetExtension (x.Location)?.Trim ('.'),
//						MediaType = FromKind (x.Kind),
//						Priority = 1,
//						ServiceId = Id,
//						ServiceType = ServiceType,
//						Track = x.TrackNumber ?? 0,
//						Year = x.Year ?? 0
//					}).ToList ();
//					Console.WriteLine ("Converting too FullTrackData took : {0}", (DateTime.Now - lastTime).TotalMilliseconds);
//					lastTime = DateTime.Now;
//					//items.BatchForeach(1000, (batch) => MusicProvider.ProcessTracks(batch.ToList()));
//					await MusicProvider.ProcessTracks (items);
//					Console.WriteLine ("ProcessTracks FullTrackData took : {0}", (DateTime.Now - lastTime).TotalMilliseconds);
//					lastTime = DateTime.Now;
					await FinalizeProcessing (this.Id);
					//Console.WriteLine ("FinalizeProcessing took : {0}", (DateTime.Now - lastTime).TotalMilliseconds);
					Console.WriteLine ("Parsing iPod took : {0}", (DateTime.Now - start).TotalMilliseconds);
					return true;
				} catch (Exception ex) {
					LogManager.Shared.Report (ex);
					return false;
				}

			});
		}

		public override System.Threading.Tasks.Task<bool> Resync ()
		{
			throw new NotImplementedException ();
		}

		public override async System.Threading.Tasks.Task<Uri> GetPlaybackUri (MusicPlayer.Models.Track track)
		{
			return new Uri (track.FileLocation);
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.DownloadUrlData> GetDownloadUri (MusicPlayer.Models.Track track)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> LoadRadioStation (MusicPlayer.Models.RadioStation station, bool isContinuation)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.RadioStationSeed seed)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.Track track)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.AlbumIds track)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.ArtistIds track)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> DeleteRadioStation (MusicPlayer.Models.RadioStation station)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> DeletePlaylist (MusicPlayer.Models.Playlist playlist)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> DeletePlaylistSong (MusicPlayer.Models.PlaylistSong song)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> MoveSong (MusicPlayer.Models.PlaylistSong song, string previousId, string nextId, int index)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToPlaylist (System.Collections.Generic.List<MusicPlayer.Models.Track> songs, MusicPlayer.Models.Playlist playlist)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToPlaylist (System.Collections.Generic.List<MusicPlayer.Models.Track> songs, string playlistName)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> SetRating (MusicPlayer.Models.Track track, int rating)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<System.Collections.Generic.List<MusicPlayer.Models.Song>> GetAlbumDetails (string id)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<SearchResults> GetArtistDetails (string id)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<System.Collections.Generic.List<MusicPlayer.Models.OnlinePlaylistEntry>> GetPlaylistEntries (MusicPlayer.Models.OnlinePlaylist playlist)
		{
			throw new NotImplementedException ();
		}

		public override async System.Threading.Tasks.Task<bool> RecordPlayack (MusicPlayer.Models.Scrobbling.PlaybackEndedEvent data)
		{
			return true;
			//throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<SearchResults> Search (string query)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary (MusicPlayer.Models.OnlinePlaylist playlist)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary (MusicPlayer.Models.RadioStation station)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary (MusicPlayer.Models.OnlineSong song)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary (MusicPlayer.Models.OnlineAlbum album)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary (MusicPlayer.Models.Track track)
		{
			throw new NotImplementedException ();
		}

		public override ServiceType ServiceType {
			get {
				return ServiceType.iPod;
			}
		}

		public override bool RequiresAuthentication {
			get {
				return false;
			}
		}

		public override string Id {
			get {
				return "iTunes";
			}
		}
		#endregion
	}
}

