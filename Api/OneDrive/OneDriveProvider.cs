using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer;
using MusicPlayer.Api;
using MusicPlayer.Models;
using MusicPlayer.Models.Scrobbling;
using Punchclock;
using MusicPlayer.Managers;
using System.Linq;

namespace OneDrive
{
    class OneDriveProvider : MusicProvider
    {
		public new OneDriveApi Api {
			get {  return (OneDriveApi) base.Api; }
		}

		public OneDriveProvider(OneDriveApi api) : base(api)
	    {
			Api.AutoAuthenticate = true;
	    }
	    public override ServiceType ServiceType => ServiceType.OneDrive;
	    public override bool RequiresAuthentication => true;
	    public override string Id => Api.Identifier;
		public override MediaProviderCapabilities[] Capabilities { get; } = new MediaProviderCapabilities[] { MediaProviderCapabilities.None };
		protected override async Task<bool> Sync()
	    {
			if (Api.CurrentAccount == null)
			{
				await Api.Authenticate();
				await Api.Identify();
			}
			if (string.IsNullOrEmpty(Email))
				await Api.Identify();
			var s = await GetTracks(Api.ExtraData.LastSongSync);
			await FinalizeProcessing(Id);
			ApiManager.Shared.SaveApi(Api);
			return s;
	    }

	    public override Task<bool> Resync()
	    {
			Api.ExtraData = new OneDriveApiExtraData();
			return Sync();
	    }

		async Task<bool> GetTracks(string continuation = "")
		{
			try
			{
				var resp = await SyncRequestQueue.Enqueue(1, () => Api.GetSpecialFolderDelta("music",token:continuation)).ConfigureAwait(false);
				Task<bool> nextTask = null;
				if (!string.IsNullOrWhiteSpace(resp?.DeltaToken) && !string.IsNullOrWhiteSpace(resp?.NextLink))
				{
					nextTask = GetTracks(resp?.DeltaToken);
				}

				var tracks = resp?.Value.Where(x=> x.Audio != null).Select(x => new FullTrackData(x.Audio.Title, x.Audio.Artist,x.Audio.Artist,x.Audio.Album, x.Audio.Genre)
				{
					Id = x.Id,
					Duration = x.Audio.Duration,
					ArtistServerId = x.Audio.Artist,
					MediaType = MediaType.Audio,
					PlayCount = 0,
					ServiceId = Api.CurrentAccount.Identifier,
					ServiceType = this.ServiceType,
					FileExtension = System.IO.Path.GetExtension(x.Name),
					Rating = 5,
					Year = x.Audio.Year,
				}).ToList();
				if ((tracks?.Count ?? 0) == 0)
					return true;
				await MusicProvider.ProcessTracks(tracks);
				if (nextTask != null)
					return await nextTask;
				Api.ExtraData.LastSongSync = resp.DeltaToken;
				return true;

			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);	
			}
			return false;
		}


	    public override async Task<Uri> GetPlaybackUri(Track track)
	    {
			if (!Api.HasAuthenticated)
				await Api.Authenticate();
			var url = (await Api.GetItemDownloadUrl(track.Id)).Replace("/redir?","/download?");
			return new Uri(url);
	    }

	    public override async Task<DownloadUrlData> GetDownloadUri(Track track)
	    {
			if (!Api.HasAuthenticated)
				await Api.Authenticate();
			var auth = new System.Net.Http.Headers.AuthenticationHeaderValue(Api.CurrentOAuthAccount.TokenType,
			                                                                 Api.CurrentOAuthAccount.Token);

			var url = (await Api.GetShareUrl(track.Id)).Replace("/redir?","/download?");
			var data = new DownloadUrlData()
			{
				Headers = new Dictionary<string, string>{
					{"Authorization", auth.ToString()},
				},
				Url = url,
			};
			return data;
	    }

	    public override Task<bool> LoadRadioStation(RadioStation station, bool isContinuation)
	    {
		    throw new NotImplementedException();
	    }

	    public override Task<RadioStation> CreateRadioStation(string name, RadioStationSeed seed)
	    {
		    throw new NotImplementedException();
	    }

	    public override Task<RadioStation> CreateRadioStation(string name, Track track)
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

	    public override Task<bool> DeleteRadioStation(RadioStation station)
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

	    public override Task<bool> MoveSong(PlaylistSong song, string previousId, string nextId, int index)
	    {
		    throw new NotImplementedException();
	    }

	    public override Task<bool> AddToPlaylist(List<Track> songs, Playlist playlist)
	    {
		    throw new NotImplementedException();
	    }

	    public override Task<bool> AddToPlaylist(List<Track> songs, string playlistName)
	    {
		    throw new NotImplementedException();
	    }

	    public override async Task<bool> SetRating(Track track, int rating)
	    {
			return true;
	    }

	    public override Task<List<Song>> GetAlbumDetails(string id)
	    {
		    throw new NotImplementedException();
	    }

	    public override Task<SearchResults> GetArtistDetails(string id)
	    {
		    throw new NotImplementedException();
	    }

	    public override Task<List<OnlinePlaylistEntry>> GetPlaylistEntries(OnlinePlaylist playlist)
	    {
		    throw new NotImplementedException();
	    }

	    public override async Task<bool> RecordPlayack(PlaybackEndedEvent data)
	    {
			return true;
	    }

	    public override Task<SearchResults> Search(string query)
	    {
		    throw new NotImplementedException();
	    }

	    public override Task<bool> AddToLibrary(OnlinePlaylist playlist)
	    {
		    throw new NotImplementedException();
	    }

	    public override Task<bool> AddToLibrary(RadioStation station)
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

	    public override async Task<string> GetShareUrl(Song song)
	    {

			var track = (await MusicManager.Shared.GetTracks(song.Id, Id)).FirstOrDefault();
			if (track == null)
				return null;
			return await Api.GetShareUrl(track.Id);
	    }

		public override string Email
		{
			get
			{
				string email = "";
				Api?.CurrentOAuthAccount?.UserData?.TryGetValue("name", out email);
				return email;
			}
		}
    }
}
