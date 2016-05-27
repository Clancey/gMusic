using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicPlayer;
using MusicPlayer.Api;
using MusicPlayer.Models;
using MusicPlayer.Models.Scrobbling;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using System.Linq;
using Punchclock;
using System.Web;
using System.Net.Http;

namespace SoundCloud
{
	public class SoundCloudProvider : MusicProvider
	{

		public SoundCloudApi Api { get; set; }

		public SoundCloudProvider(HttpMessageHandler handler = null) : this(new SoundCloudApi("soundcloud",handler))
		{

		}

		public SoundCloudProvider(SoundCloudApi api) : base(api)
		{
			Api = api;
		}

		public override MediaProviderCapabilities[] Capabilities
		{
			get
			{
				return new MediaProviderCapabilities[] { MediaProviderCapabilities.Playlists, MediaProviderCapabilities.Searchable };
			}
		}


		public override bool RequiresAuthentication => Api.Identifier != "soundcloud";

		public override string Id => Api?.Identifier ?? "soundcloud";

		public override ServiceType ServiceType
		{
			get
			{
				return ServiceType.SoundCloud;
			}
		}
		public override string Email
		{
			get
			{
				return RequiresAuthentication ? "Email" : null;
			}
		}
		public override async Task<bool> AddToLibrary(RadioStation station)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override Task<bool> AddToLibrary(Track track)
		{
			return SetRating(track, 5);
		}

		public override async Task<bool> AddToLibrary(OnlineAlbum album)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override Task<bool> AddToLibrary(OnlineSong song)
		{
			return SetRating(song.TrackData, 5);
		}

		public override async Task<bool> AddToLibrary(OnlinePlaylist playlist)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> AddToPlaylist(List<Track> songs, string playlistName)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> AddToPlaylist(List<Track> songs, Playlist playlist)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<RadioStation> CreateRadioStation(string name, AlbumIds track)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override Task<RadioStation> CreateRadioStation(string name, ArtistIds track)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<RadioStation> CreateRadioStation(string name, Track track)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override Task<RadioStation> CreateRadioStation(string name, RadioStationSeed seed)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<bool> DeletePlaylist(Playlist playlist)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> DeletePlaylistSong(PlaylistSong song)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<bool> DeleteRadioStation(RadioStation station)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<List<Song>> GetAlbumDetails(string id)
		{
			App.ShowNotImplmented();
			return new List<Song>();
		}

		public override async Task<SearchResults> GetArtistDetails(string id)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<DownloadUrlData> GetDownloadUri(Track track)
		{
			const string apiKey = "XSGYiNkhWe60LlcYKwdw";
			var url = $"http://api.soundcloud.com/tracks/{track.Id}/stream?client_id={apiKey}";
			return new DownloadUrlData
			{
				Url = url
			};
		}

		public override async Task<Uri> GetPlaybackUri(Track track)
		{
			var url = $"http://api.soundcloud.com/tracks/{track.Id}/stream?client_id={sapiKey}";
			return new Uri(url);
		}

		public override async Task<List<OnlinePlaylistEntry>> GetPlaylistEntries(OnlinePlaylist playlist)
		{
			App.ShowNotImplmented();
			return new List<OnlinePlaylistEntry>();
		}

		const string sapiKey = "XSGYiNkhWe60LlcYKwdw";
		public override async Task<string> GetShareUrl(Song song)
		{
			var track = (await MusicManager.Shared.GetTracks(song.Id, Id)).FirstOrDefault();
			if (track == null)
				return null;
			var url = $"http://api.soundcloud.com/tracks/{track.Id}/stream?client_id={sapiKey}";
			return url;
		}

		public override async Task<bool> LoadRadioStation(RadioStation station, bool isContinuation)
		{
			App.ShowNotImplmented();
			return false;
			//throw new NotImplementedException();
		}

		public override async Task<bool> MoveSong(PlaylistSong song, string previousId, string nextId, int index)
		{
			App.ShowNotImplmented();
			return false;
			//throw new NotImplementedException();
		}

		public override async Task<bool> RecordPlayack(PlaybackEndedEvent data)
		{
			//App.ShowNotImplmented();
			return true;
		}

		public override Task<bool> Resync()
		{
			return SyncDatabase();
		}

		public override async Task<SearchResults> Search(string query)
		{
			try{
				var result = new SearchResults();
				var path = $"tracks.json?client_id={sapiKey}&filter=streamable&limit=200&q={HttpUtility.UrlEncode(query)}";
				var resp = await Api.Get<   List<STrack>>(path);
				var tracks = resp?.Select(x => new FullTrackData(x.Title, x.User?.Username, "", "", x.Genre)
				{
					Id = x.Id.ToString(),
					Duration = x.Duration,
					ArtistServerId = x.UserId.ToString(),
					MediaType = MediaType.Audio,
					PlayCount = x.UserPlaybackCount ?? 0,
					ServiceId = Api.CurrentAccount.Identifier,
					ServiceType = this.ServiceType,
					FileExtension = "mp3",
					Year = x.ReleaseYear ?? 0,
					AlbumArtwork = new List<AlbumArtwork> { new AlbumArtwork { Url = x.ArtworkUrl } },
				}).Select(x => new OnlineSong(x.Title, x.NormalizedTitle)
				{
					Id = x.SongId,
					Artist = x.Artist,
					Album = x.Album,
					AlbumId = x.AlbumId,
					ArtistId = x.ArtistId,
					Genre = x.Genre,
					Rating = x.Rating,
					TrackData = x,
				}).ToList();
				if((tracks?.Count ?? 0) > 0)
 					result.Songs.AddRange(tracks);
				return result;
			}
			catch(Exception ex) {
				LogManager.Shared.Report(ex);
				//Logger.LogBadRequest (ex, "soundcloud: " + text);
			}
			return null;

		}

		public override async Task<bool> SetRating(Track track, int rating)
		{
			try
			{
				if (rating > 1)
				{
					var s = await Api.Put(null, $"me/favorites/{track.Id}");
					return true;
				}
				var s1 = await Api.Delete(null, $"me/favorites/{track.Id}");
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
				
		}

		protected override async Task<bool> Sync()
		{
			await Api.Authenticate();
			await SyncLibrary();
			await FinalizeProcessing(Id);
			return false;
		}


		async Task<bool> SyncLibrary(string href = "")
		{
			try
			{
				var resp = await SyncRequestQueue.Enqueue(1, () => string.IsNullOrWhiteSpace(href) ? Api.GetFavorites() : Api.Get<SApiResponse<STrack>>(href)).ConfigureAwait(false);

				Task<bool> nextTask = null;
				if (!string.IsNullOrWhiteSpace(resp?.NextUrl))
					nextTask = SyncLibrary(resp?.NextUrl);

				var tracks = resp?.Items.Select(x => new FullTrackData(x.Title, x.User?.Username, "", "", x.Genre)
				{
					Id = x.Id.ToString(),
					Duration = x.Duration,
					ArtistServerId = x.UserId.ToString(),
					MediaType = MediaType.Audio,
					PlayCount = x.UserPlaybackCount ?? 0,
					ServiceId = Api.CurrentAccount.Identifier,
					ServiceType = this.ServiceType,
					FileExtension = "mp3",
					Rating = 5,
					Year = x.ReleaseYear ?? 0,
					AlbumArtwork = new List<AlbumArtwork> { new AlbumArtwork { Url = x.ArtworkUrl } },
				}).ToList();
				if ((tracks?.Count ?? 0) == 0)
					return true;
				await MusicProvider.ProcessTracks(tracks);
				if (nextTask != null)
					return await nextTask;
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		async Task CheckUserId()
		{
			var user = await Api.GetUserInfo();

		}
	}
}

