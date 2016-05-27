using System;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MusicPlayer.Models;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MusicPlayer.Data;
using MusicPlayer.Managers;

namespace MusicPlayer.Api
{
	public class FileSystemProvider : MusicProvider
	{
		public FileSystemProvider () : base(null)
		{
		}
		public override MediaProviderCapabilities[] Capabilities {
			get {
				return new[]{MediaProviderCapabilities.None};
			}
		}
		#region implemented abstract members of MusicProvider

		HashSet<string> supportedExtensions = new HashSet<string> {
			".mp3",
			".mp4",
			".m4v",
		};


		public override string Email
		{
			get
			{
				return "NA";
			}
		}

		static readonly string musicPath = System.Environment.GetFolderPath (Environment.SpecialFolder.MyMusic);

		public bool Disabled { get ; set; } = false;

		protected override async Task<bool> Sync ()
		{
			if (Disabled)
				return true;
			return await Task.Run (async () => {
				try {
					var start = DateTime.Now;
					//await Database.Main.ExecuteAsync ("update Track set Deleted = 0 where ServiceId = ?", Id);
					var files = GetFiles (musicPath,Settings.LastFilesystemSync).ToList ();
					if(!files.Any())
						return true;
					Console.WriteLine (files.Count ());

					Console.WriteLine ("Loading Filesystem took : {0}", (DateTime.Now - start).TotalMilliseconds);
					var lastTime = DateTime.Now;
					var items = files.Select (x => FromFilePath (x)).ToList ();
					Console.WriteLine ("Converting too FullTrackData took : {0}", (DateTime.Now - lastTime).TotalMilliseconds);
					lastTime = DateTime.Now;
					//items.BatchForeach(1000, (batch) => MusicProvider.ProcessTracks(batch.ToList()));
					await MusicProvider.ProcessTracks (items);
					Console.WriteLine ("ProcessTracks FullTrackData took : {0}", (DateTime.Now - lastTime).TotalMilliseconds);
					lastTime = DateTime.Now;
					await FinalizeProcessing (this.Id);
					Console.WriteLine ("FinalizeProcessing took : {0}", (DateTime.Now - lastTime).TotalMilliseconds);
					Console.WriteLine ("Parsing Filesystem took : {0}", (DateTime.Now - start).TotalMilliseconds);
					var lastUpdate = files.Max(File.GetLastWriteTime);
					var lastCreate = files.Max(File.GetCreationTime);
					Settings.LastFilesystemSync = lastUpdate > lastCreate ? lastUpdate: lastCreate;
					return true;
				} catch (Exception ex) {
					LogManager.Shared.Report (ex);
					return false;
				}

			});
		}
		DateTime lastDateTime;
		FullTrackData FromFilePath (string filePath)
		{
			string id = "";
			string Title = "";
			string Artist = "";
			string Album = "";
			string AlbumArtist = "";
			string Genre = "";
			int Year = 0;
			int Disc = 0;
			int TotalDiscs = 0;
			int Track = 0;
			int BeatsPerMinute = 0;
			double Duration = 0;
			int rating = 0;
			bool hasImage;
			try {
				using (var file = TagLib.File.Create (filePath)) {
					if (string.IsNullOrEmpty (file.Tag.MusicIpId)) {
						using (var md5 = MD5.Create ()) {
							using (var stream = System.IO.File.OpenRead (filePath)) {
								byte[] hash;
								hash = md5.ComputeHash (stream);

								id = BitConverter.ToString (hash).Replace ("-", "").ToLower ();
							}
						}
						file.Tag.MusicIpId = id;	
						file.Save ();
					} else {
						id = file.Tag.MusicIpId;
					}
					
					TagLib.Tag Tag = file.GetTag(TagLib.TagTypes.Id3v2);
					var ratingTag = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)Tag, "WindowsUser", true);
					//based on 0-5;
					//taglib sharp is 0-255;
					rating = ratingTag.Rating / 51;
					Title = file?.Tag?.Title ?? FullTrackData.GetTitleFromFileName (filePath);
					Artist = file.Tag.FirstPerformer ?? FullTrackData.GetArtistFromFileName (filePath);
					Album = file.Tag.Album ?? "";
					AlbumArtist = file.Tag.FirstAlbumArtist ?? "";
					Genre = file.Tag.FirstGenre ?? "";
					Year = (int)file.Tag.Year;
					Disc = (int)file.Tag.Disc;
					TotalDiscs = (int)file.Tag.DiscCount;
					Track = (int)file.Tag.Track;
					BeatsPerMinute = (int)file.Tag.BeatsPerMinute;
					Duration = file.Properties.Duration.TotalMilliseconds;
				}
			} catch (Exception ex) {
				Title = FullTrackData.GetTitleFromFileName (filePath);
				Artist = FullTrackData.GetArtistFromFileName (filePath);
			}
			return new FullTrackData (Title, Artist, AlbumArtist, Album, Genre) {
				Disc = Disc,
				Track = Track,
				Year = Year,
				Id = id,
				FileLocation = filePath,
				FileExtension = Path.GetExtension (filePath)?.Trim ('.'),
				MediaType = FromFile (filePath),
				Priority = 1,
				ServiceId = Id,
				ServiceType = ServiceType,
				Rating = rating,
			};
		}
		public override async Task<string> GetShareUrl (Song song)
		{
			return null;
		}

		static MediaType FromFile (string filePath)
		{
			try {
				#if __IOS__ || __OSX__
//				using (var asset = AVFoundation.AVAsset.FromUrl (Foundation.NSUrl.FromFilename (filePath))) {
//					var movie = asset.Tracks.Where (x => x.MediaType == AVFoundation.AVMediaType.Video).Any ();
//					return movie ? MediaType.Video : MediaType.Audio;
//				}

				#endif
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
			var extension = Path.GetExtension (filePath).Trim ('.');
			switch (extension) {
			case "m4v":
			case "mp4":
				return MediaType.Video;
			default:
				return MediaType.Audio;
			}
		}

		public override  Task<bool> Resync ()
		{
			Settings.LastFilesystemSync = null;
			return SyncDatabase();
		}

		static IEnumerable<string> GetFiles (string directory, DateTime? changedDate = null)
		{
			var date = changedDate ?? DateTime.MinValue;
			var files = Directory.EnumerateFiles (directory, "*.*", SearchOption.AllDirectories).Where (x => x.EndsWith (".mp3", StringComparison.InvariantCultureIgnoreCase) || x.EndsWith ("mp4", StringComparison.InvariantCultureIgnoreCase) || x.EndsWith (".aac", StringComparison.InvariantCultureIgnoreCase) || x.EndsWith (".m4a", StringComparison.InvariantCultureIgnoreCase) || x.EndsWith ("m4v", StringComparison.InvariantCultureIgnoreCase));

			var filterd = files.Where (x => File.GetCreationTime (x) > date || File.GetLastWriteTime (x) > date);
			return filterd;
		}

		public override async Task<Uri> GetPlaybackUri (MusicPlayer.Models.Track track)
		{
			var uri = new Uri (track.FileLocation).AbsoluteUri;
			return new Uri(uri);
		}

		public override async Task<MusicPlayer.Models.DownloadUrlData> GetDownloadUri (MusicPlayer.Models.Track track)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<bool> LoadRadioStation (MusicPlayer.Models.RadioStation station, bool isContinuation)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.RadioStationSeed seed)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.Track track)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.AlbumIds track)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.ArtistIds track)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<bool> DeleteRadioStation (MusicPlayer.Models.RadioStation station)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> DeletePlaylist (MusicPlayer.Models.Playlist playlist)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> DeletePlaylistSong (MusicPlayer.Models.PlaylistSong song)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> MoveSong (MusicPlayer.Models.PlaylistSong song, string previousId, string nextId, int index)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> AddToPlaylist (System.Collections.Generic.List<MusicPlayer.Models.Track> songs, MusicPlayer.Models.Playlist playlist)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> AddToPlaylist (System.Collections.Generic.List<MusicPlayer.Models.Track> songs, string playlistName)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> SetRating (MusicPlayer.Models.Track track, int rating)
		{
			using (TagLib.File file = TagLib.File.Create(track.FileLocation))
			{
				TagLib.Tag Tag = file.GetTag(TagLib.TagTypes.Id3v2);
				TagLib.Id3v2.Tag.DefaultVersion = 3;
				TagLib.Id3v2.Tag.ForceDefaultVersion = true;
				var frame = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)Tag, "WindowsUser", true);
				//based on 0-5;
				//taglib sharp is 0-255;
				frame.Rating = (byte)(rating * 51);
				file.Save();
			}
			return true;
		}
 
		public override async Task<System.Collections.Generic.List<MusicPlayer.Models.Song>> GetAlbumDetails (string id)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<SearchResults> GetArtistDetails (string id)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<System.Collections.Generic.List<MusicPlayer.Models.OnlinePlaylistEntry>> GetPlaylistEntries (MusicPlayer.Models.OnlinePlaylist playlist)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<bool> RecordPlayack (MusicPlayer.Models.Scrobbling.PlaybackEndedEvent data)
		{
			return true;
		}

		public override async Task<SearchResults> Search (string query)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<bool> AddToLibrary (MusicPlayer.Models.OnlinePlaylist playlist)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> AddToLibrary (MusicPlayer.Models.RadioStation station)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> AddToLibrary (MusicPlayer.Models.OnlineSong song)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> AddToLibrary (MusicPlayer.Models.OnlineAlbum album)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> AddToLibrary (MusicPlayer.Models.Track track)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override ServiceType ServiceType {
			get {
				return ServiceType.FileSystem;
			}
		}

		public override bool RequiresAuthentication {
			get {
				return false;
			}
		}

		public override string Id {
			get {
				return "filesystem";
			}
		}

		#endregion
	}
}

