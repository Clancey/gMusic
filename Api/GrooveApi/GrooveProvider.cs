using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicPlayer;
using MusicPlayer.Api;
using MusicPlayer.Models;
using MusicPlayer.Models.Scrobbling;
using MusicPlayer.Managers;

namespace Groove
{
	public class GrooveProvider : MusicProvider
	{

		public GrooveProvider(GrooveApi api) : base(api)
	    {
			Api.AutoAuthenticate = true;
		}

		public new GrooveApi Api => (GrooveApi)base.Api;

		public override MediaProviderCapabilities[] Capabilities => new[] { MediaProviderCapabilities.NewReleases | MediaProviderCapabilities.Playlists | MediaProviderCapabilities.Radio | MediaProviderCapabilities.Searchable | MediaProviderCapabilities.Trending };

		public override string Email
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override string Id => Api?.Identifier;

		public override bool RequiresAuthentication => true;

		public override ServiceType ServiceType => ServiceType.Groove;

		public override Task<bool> AddToLibrary(RadioStation station)
		{
			throw new NotImplementedException();
		}

		public override async Task<bool> AddToLibrary(Track track)
		{
			try
			{
				var resp = await Api.AddToLibrary($"music.{track.Id}");
				return true;
			}
			catch(Exception ex)
			{
				LogManager.Shared.Report(ex);
				return false;
			}
		}

		public override Task<bool> AddToLibrary(OnlineAlbum album)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> AddToLibrary(OnlineSong song)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> AddToLibrary(OnlinePlaylist playlist)
		{
			throw new NotImplementedException();
		}



		public override Task<bool> AddToPlaylist(List<Track> songs, string playlistName)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> AddToPlaylist(List<Track> songs, Playlist playlist)
		{
			throw new NotImplementedException();
		}

		public override Task<RadioStation> CreateRadioStation(string name, AlbumIds track)
		{
			throw new NotImplementedException();
		}

		public override Task<RadioStation> CreateRadioStation(string name, ArtistIds track)
		{
			throw new NotImplementedException();
		}

		public override Task<RadioStation> CreateRadioStation(string name, Track track)
		{
			throw new NotImplementedException();
		}

		public override Task<RadioStation> CreateRadioStation(string name, RadioStationSeed seed)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> DeletePlaylist(Playlist playlist)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> DeletePlaylistSong(PlaylistSong song)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> DeleteRadioStation(RadioStation station)
		{
			throw new NotImplementedException();
		}

		public override Task<List<Song>> GetAlbumDetails(string id)
		{
			throw new NotImplementedException();
		}

		public override Task<SearchResults> GetArtistDetails(string id)
		{
			throw new NotImplementedException();
		}

		public override async Task<DownloadUrlData> GetDownloadUri(Track track)
		{
			var resp = await GetPlaybackUri(track);
			return new DownloadUrlData {
				Url = resp.AbsoluteUri,
			};
		}

		public override async Task<Uri> GetPlaybackUri(Track track)
		{
			var resp = await Api.GetFullTrackStream(track.Id);
			return new Uri(resp.Url);
		}

		public override Task<List<OnlinePlaylistEntry>> GetPlaylistEntries(OnlinePlaylist playlist)
		{
			throw new NotImplementedException();
		}

		public override Task<string> GetShareUrl(Song song)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> LoadRadioStation(RadioStation station, bool isContinuation)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> MoveSong(PlaylistSong song, string previousId, string nextId, int index)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> RecordPlayack(PlaybackEndedEvent data)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> Resync()
		{
			//TODO: Clear out cached data
			return SyncDatabase();
		}

		public override Task<SearchResults> Search(string query)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> SetRating(Track track, int rating)
		{
			throw new NotImplementedException();
		}

		protected override async Task<bool> Sync()
		{
			var s = await Api.Authenticate();
			return await SyncTracks(DateTime.MinValue);
		}

		public async Task<bool> SyncTracks(DateTime filterDateTime, int page = 0)
		{
			try
			{
				var resp = await Api.BrowseUserCollection(GrooveNamespace.Music, GrooveTypes.Tracks, "CollectionDate", page);
				//CollectionDate
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return false;
			}
		}
	}
}
