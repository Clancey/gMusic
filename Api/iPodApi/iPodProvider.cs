using System;
using MusicPlayer.Api;
using MediaPlayer;
using Foundation;
using System.Linq;
using System.Threading.Tasks;
using MusicPlayer.Models;
using MusicPlayer.Managers;
using System.Collections.Generic;
using MusicPlayer.Data;

namespace MusicPlayer.Api.iPodApi
{
	public class iPodProvider : MusicProvider
	{

		public override MediaProviderCapabilities[] Capabilities {
			get {
				return new[]{ MediaProviderCapabilities.None};
			}
		}
		public override ServiceType ServiceType  => ServiceType.iPod;

		public override string Id  => "iPod";
		public override bool RequiresAuthentication => false;

		public iPodProvider() : base(null)
		{
		}
		public override string Email
		{
			get
			{
				return  null;
			}
		}

		#region implemented abstract members of MusicProvider
		public bool Disabled => !Settings.IncludeIpod;
		protected override async System.Threading.Tasks.Task<bool> Sync()
		{
			if(Disabled)
				return true;
			return await Task.Run(async ()=>{
				try{
					await Database.Main.ExecuteAsync("update Track set Deleted = 1 where ServiceId = ?", Id);
					var mediaQuery = MPMediaQuery.SongsQuery;
					var predicate = MPMediaPropertyPredicate.PredicateWithValue(NSNumber.FromBoolean(false),MPMediaItem.IsCloudItemProperty);
					mediaQuery.AddFilterPredicate(predicate);
					if (mediaQuery.Items == null)
						return true;

					var items = mediaQuery.Items.Where(x=> x.AssetURL != null && !string.IsNullOrEmpty(x.AssetURL.AbsoluteString)).Select(x => new FullTrackData(x.Title,x.Artist,x.AlbumArtist,x.AlbumTitle,x.Genre) {
						Id = x.PersistentID.ToString(),
						AlbumServerId = x.AlbumPersistentID.ToString(),
						Disc = x.DiscNumber,
						Duration = x.PlaybackDuration,
						FileExtension = "mp3",
						MediaType = MediaType.Audio,
						Priority = 1,
						ServiceId = Id,
						ServiceType = ServiceType,
						Track = x.AlbumTrackNumber,
		//				Year = x.ReleaseDate
					});
					await items.BatchForeach(100, (batch) => MusicProvider.ProcessTracks(batch.ToList()));
					await FinalizeProcessing(Id);
					return true;
				}
				catch(Exception ex)
				{
					LogManager.Shared.Report(ex);
					return false;
				}
			
			});
		}

		public override System.Threading.Tasks.Task<bool> Resync()
		{
			return SyncDatabase();
		}

		public override Task<Uri> GetPlaybackUri(MusicPlayer.Models.Track track)
		{
			return Task.Run(() =>{
				var query = MPMediaQuery.SongsQuery;
				NSNumber songId = UInt64.Parse (track.Id);
				MPMediaPropertyPredicate predicate = MPMediaPropertyPredicate.PredicateWithValue (songId, MPMediaItem.PersistentIDProperty);
				query.AddFilterPredicate (predicate);
				var s = query.Items.FirstOrDefault ();
				var url = s?.AssetURL?.AbsoluteString;
				return url == null ? null : new Uri(url);
			});

		}
		public override async Task<string> GetShareUrl (Song song)
		{
			return null;
		}
		public static MPMediaItem GetItem(Track track)
		{
			var query = MPMediaQuery.SongsQuery;
			NSNumber songId = UInt64.Parse(track.Id);
			MPMediaPropertyPredicate predicate = MPMediaPropertyPredicate.PredicateWithValue(songId, MPMediaItem.PersistentIDProperty);
			query.AddFilterPredicate(predicate);
			var s = query.Items?.FirstOrDefault();
			return s;
		}
		public override System.Threading.Tasks.Task<MusicPlayer.Models.DownloadUrlData> GetDownloadUri(MusicPlayer.Models.Track track)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<bool> LoadRadioStation(MusicPlayer.Models.RadioStation station, bool isContinuation)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation(string name, MusicPlayer.Models.RadioStationSeed seed)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation(string name, MusicPlayer.Models.Track track)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation(string name, MusicPlayer.Models.AlbumIds track)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation(string name, MusicPlayer.Models.ArtistIds track)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<bool> DeleteRadioStation(MusicPlayer.Models.RadioStation station)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<bool> DeletePlaylist(MusicPlayer.Models.Playlist playlist)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<bool> DeletePlaylistSong(MusicPlayer.Models.PlaylistSong song)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<bool> MoveSong(MusicPlayer.Models.PlaylistSong song, string previousId, string nextId, int index)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<bool> AddToPlaylist(System.Collections.Generic.List<MusicPlayer.Models.Track> songs, MusicPlayer.Models.Playlist playlist)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<bool> AddToPlaylist(System.Collections.Generic.List<MusicPlayer.Models.Track> songs, string playlistName)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<bool> SetRating(MusicPlayer.Models.Track track, int rating)
		{
			throw new NotImplementedException();
		}

		public override async Task<System.Collections.Generic.List<MusicPlayer.Models.Song>> GetAlbumDetails(string id)
		{
			return new List<Song>();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.SearchResults> GetArtistDetails(string id)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<System.Collections.Generic.List<MusicPlayer.Models.OnlinePlaylistEntry>> GetPlaylistEntries(MusicPlayer.Models.OnlinePlaylist playlist)
		{
			throw new NotImplementedException();
		}

		public override async Task<bool> RecordPlayack(MusicPlayer.Models.Scrobbling.PlaybackEndedEvent data)
		{
			//throw new NotImplementedException();
			return true;
		}

		public override System.Threading.Tasks.Task<MusicPlayer.SearchResults> Search(string query)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary(MusicPlayer.Models.OnlinePlaylist playlist)
		{
			throw new NotImplementedException();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary(MusicPlayer.Models.RadioStation station)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> AddToLibrary(OnlineSong song)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> AddToLibrary(OnlineAlbum album)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> AddToLibrary(Track track)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

