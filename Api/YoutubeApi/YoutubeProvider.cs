using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using MusicPlayer;
using MusicPlayer.Api;
using MusicPlayer.Models;
using System.Linq;

using Google.Apis.Youtube.v3.Data;
using Playlist = MusicPlayer.Models.Playlist;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using SimpleAuth;
using System.Net;

namespace YoutubeApi
{
	public class YoutubeProvider : MusicProvider
	{
		public new YoutubeOauthApi Api {
			get {  return (YoutubeOauthApi) base.Api; }
		}

		public override ServiceType ServiceType => ServiceType.YouTube;

		public override bool RequiresAuthentication => Api.Identifier != "youtube";

		public override string Id => Api?.Identifier ?? "youtube";

		public override MediaProviderCapabilities[] Capabilities
		{
			get
			{
				//if(RequiresAuthentication)
					return new []{MediaProviderCapabilities.Searchable , MediaProviderCapabilities.NewReleases , MediaProviderCapabilities.Trending , MediaProviderCapabilities.Playlists};
				//return new[]{ MediaProviderCapabilities.Searchable ,  MediaProviderCapabilities.NewReleases , MediaProviderCapabilities.Trending };
			}
		}

		public YoutubeProvider (YoutubeOauthApi api) : base(api)
		{
			
        }

		public const string DefaultId = "youtube";
		public YoutubeProvider(HttpMessageHandler handler = null) : this(new YoutubeOauthApi(DefaultId,handler))
		{
			
		}
		public override string Email
		{
			get
			{
				return Api?.Email ?? "";
			}
		}
		#region implemented abstract members of MusicProvider

		protected override async Task<bool> Sync ()
		{
			try{
				if (Id == DefaultId)
					return true;
				if (!RequiresAuthentication)
					return true;
				if (Api.CurrentAccount == null)
					await Api.Authenticate();
				await SetupLibraryPlaylist ();
				var s = await SyncPlaylists();
				if(s)
					await FinalizePlaylists(Id);
				return s;
			}
			catch(Exception ex) {
				LogManager.Shared.Report (ex);
				return false;
			}
		}

		public override async Task<bool> Resync ()
		{
			if (!RequiresAuthentication)
				return true;
			Api.ExtraData.LastSongSyncTag = null;
			Api.ExtraData.LastPlaylistTag = null;
			//TODO clar playist tags
			return await SyncDatabase();
		}
		public override Task Logout ()
		{
			Settings.AutoAddYoutube = false;
			return base.Logout ();
		}
		public override Task<Uri> GetPlaybackUri (Track track)
		{
			return Task.Run(() => {
				var url = $"https://www.youtube.com/watch?v={track.Id}";
				var videoInfos = YoutubeExtractor.DownloadUrlResolver.GetDownloadUrls(url);
				var video = YouTubeHelper.GetVideoInfo(videoInfos, true);
				return new Uri(video.DownloadUrl);
			});
		}
		public override async Task<string> GetShareUrl (Song song)
		{
			var track = await Database.Main.TablesAsync<Track>().Where(x => x.SongId == song.Id && x.ServiceType == ServiceType).FirstOrDefaultAsync()  
				?? await Database.Main.TablesAsync<TempTrack>().Where(x => x.SongId == song.Id && x.ServiceType == ServiceType).FirstOrDefaultAsync();
			if (track == null) {
				track = (song as OnlineSong)?.TrackData;
			}
			if (track == null)
				return null;
			var url = $"https://www.youtube.com/watch?v={track.Id}";
			return url;
		}

		async Task SetupLibraryPlaylist()
		{
			if (!string.IsNullOrWhiteSpace (Api.ExtraData.PlaylistId))
				return;
			var id = await GetOrCreatePlaylistId ();
			Api.ExtraData.PlaylistId = id;
		
		}

		object taskLocker = new object();

		TaskCompletionSource<string> libraryPlaylistSource = new TaskCompletionSource<string>();
		Task<string> getOrCreateTask;
		const string OfflinePlaylistId = "Offline";
		async Task<string> GetOrCreatePlaylistId()
		{
			if (!string.IsNullOrWhiteSpace (Api.ExtraData.PlaylistId))
				return Api.ExtraData.PlaylistId;
			if (!RequiresAuthentication)
				return OfflinePlaylistId;
			lock (taskLocker) {
				if (getOrCreateTask?.IsCompleted != false) {
					getOrCreateTask = getOrCreatePlaylistId ();
				}
			}
			return await getOrCreateTask;
		}
		const string LibraryPlaylistName = "gMusic Library";
		async Task<string> getOrCreatePlaylistId()
		{
			try
			{
				var playlistSyncTask = SyncPlaylists();
				var finishedTask = await Task.WhenAny(playlistSyncTask, libraryPlaylistSource.Task);
				if (finishedTask == libraryPlaylistSource.Task)
				{
					var playlistId = libraryPlaylistSource.Task.Result;
					if (!string.IsNullOrWhiteSpace(playlistId))
						return playlistId;
				}

			}
			catch (WebException ex)
			{
				Console.WriteLine(ex);
			}
			var playlist = new Playlist(LibraryPlaylistName)
			{
				Description = "All tracks in this playlist will sync to gMusic",
				ServiceId = Api.Identifier,
				ServiceType = ServiceType,
			};
			if (await CreatePlaylist(playlist, false))
				return playlist.Id;
			return null;

		}

		Task<bool> syncPlaylistTask;
		public Task<bool> SyncPlaylists()
		{
			lock (taskLocker) {
				if (syncPlaylistTask?.IsCompleted != false) {
					syncPlaylistTask = syncPlaylists ();
				}
			}
			return syncPlaylistTask;
		}

		async Task<bool> syncPlaylists()
		{
			try{
				var playlists = await GetPlaylists();
				foreach (var playlist in playlists)
				{
					Playlist plist;
					var oldPlist = await Database.Main.TablesAsync<Playlist>().Where(x => x.Id == playlist.Id).FirstOrDefaultAsync();
					if (oldPlist != null)
						plist = oldPlist;
					else {
						plist = playlist;
						Database.Main.InsertOrReplace(playlist);
					}
					if (!await ProcessPlaylist(plist))
						return false;
				}
				ApiManager.Shared.SaveApi(Api);
				return true;
			}
			catch(Exception ex) {
				LogManager.Shared.Report (ex);
				return false;
			}
		}



		async Task<bool> ProcessPlaylist(Playlist playlist)
		{
			var tracks = await getPlaylistEntries(playlist);
			if (playlist.Id == Api.ExtraData.PlaylistId)
			{
				if (await ProcessTracks(tracks.OfType<FullTrackData>().ToList()))
				{
					Api.ExtraData.LastSongSyncTag = playlist.ServiceExtra;
					await FinalizeProcessing(Id);
					return true;
				}
				return false;
				
			}
			return await ProcessPlaylistTracks(tracks, new List<TempPlaylistEntry>());
		}


		async Task<List<Playlist>> GetPlaylists(string pageToken = "")
		{
			List<Playlist> playlists = new List<Playlist>();
			//var path = "playlists?part=id%2CcontentDetails%2Csnippet&channelId=UC-9-kyTW8ZkZNDHQJ6FgpwQ";
			var path = "playlists?maxResults=50&part=id%2CcontentDetails%2Csnippet&mine=true";
			if(!string.IsNullOrEmpty(pageToken))
				path = $"{path}&pageToken={pageToken}";
			Dictionary<string, string> headers = null;
			if (!string.IsNullOrWhiteSpace(Api.ExtraData.LastPlaylistTag))
				headers = new Dictionary<string, string>{{"ETag",Api.ExtraData.LastPlaylistTag}};
			var resp = await Api.Get<PlaylistListResponse> (path,headers: headers);
			Api.ExtraData.LastPlaylistTag = resp.ETag;
			foreach (var x in resp.Items) {
				if (x.Snippet.Title == LibraryPlaylistName)
				{
					libraryPlaylistSource?.TrySetResult(x.Id);
				}
				playlists.Add(new Playlist (x.Snippet.Title) {
					Description = x.Snippet.Description,
					ServiceId = Api.Identifier,
					ServiceType = ServiceType,
					Id = x.Id,
				});
			}

			if (!string.IsNullOrWhiteSpace (resp.NextPageToken)) {
				playlists.AddRange(await GetPlaylists (resp.NextPageToken));
			}

			return playlists;
		}
		public async Task<bool> CreatePlaylist(Playlist playlist, bool include = true)
		{
			try{
				var body = new Google.Apis.Youtube.v3.Data.Playlist {
					Snippet = new PlaylistSnippet {
						Title = playlist.Name,
						Description = playlist.Description,
					}
				};
				var resp = await Api.Post<Google.Apis.Youtube.v3.Data.Playlist> (body, "https://www.googleapis.com/youtube/v3/playlists?part=snippet");
				if (string.IsNullOrWhiteSpace (body.Id))
					return false;
				playlist.Id = body.Id;
				if(include)
					Database.Main.InsertOrReplace (playlist);
				return true;
			}
			catch(Exception ex) {
				LogManager.Shared.Report (ex);
				return false;
			}
		}

		public override async Task<DownloadUrlData> GetDownloadUri (Track track)
		{
			var url = await GetPlaybackUri(track);
			return new DownloadUrlData
			{
				Url = url?.AbsoluteUri
			};
		}

		public override async Task<bool> LoadRadioStation (RadioStation station, bool isContinuation)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override async Task<RadioStation> CreateRadioStation (string name, RadioStationSeed seed)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<RadioStation> CreateRadioStation (string name, Track track)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<RadioStation> CreateRadioStation (string name, AlbumIds track)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<RadioStation> CreateRadioStation (string name, ArtistIds track)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override async Task<bool> DeleteRadioStation (RadioStation station)
		{
			App.ShowNotImplmented();
			return false ;
		}

		public override async Task<bool> DeletePlaylist (Playlist playlist)
		{
			try
			{
				if (!RequiresAuthentication)
				{
					Database.Main.Delete(playlist);
					return true;
				}
				var resp = await Api.Delete(path: "playlists", queryParameters: new Dictionary<string, string> { { "id", playlist.Id } });

				var success = string.IsNullOrWhiteSpace(resp) || resp.Contains("playlistNotFound");
				if (success)
					Database.Main.Delete(playlist);

				return success;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public override async Task<bool> DeletePlaylistSong (PlaylistSong song)
		{
			try
			{
				if (!RequiresAuthentication)
				{
					Database.Main.Delete(song);
					return true;
				}
				var resp = await Api.Delete(path: "playlistItems", queryParameters: new Dictionary<string, string> { { "id", song.Id } });

				var success = string.IsNullOrWhiteSpace(resp);
				if (success)
					Database.Main.Delete(song);
				
				return success;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}


		public override async Task<bool> MoveSong (PlaylistSong song, string previousId, string nextId, int index)
		{
			try
			{
				if (!RequiresAuthentication)
				{
					App.ShowNotImplmented();
					return false;
				}
				var body = new Google.Apis.Youtube.v3.Data.PlaylistItem
				{
					Snippet = new PlaylistItemSnippet
					{
						PlaylistId = song.PlaylistId,
						ResourceId = new ResourceId
						{
							Kind = "youtube#video",
							VideoId = song.SongId,
						},
						Position = index,
					},
					Id = song.Id
				};
				var resp = await Api.Put<Google.Apis.Youtube.v3.Data.PlaylistItem>(body, "playlistItems?part=snippet");
				SyncPlaylists();
				return resp.Snippet.Position == index;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);

			}
			return false;
		}

		public override async Task<bool> AddToPlaylist (System.Collections.Generic.List<Track> songs, Playlist playlist)
		{
			foreach (var track in songs) {
				if (!await AddToPlaylist (track, playlist))
					return false;
			}

			await FinalizePlaylists(Id);
			return true;
		}

		public override async Task<bool> AddToPlaylist (System.Collections.Generic.List<Track> songs, string playlistName)
		{
			var playlist = new Playlist (playlistName) {
				ServiceId = Api.Identifier,
				ServiceType = ServiceType,
			};
			if (!await CreatePlaylist (playlist))
				return false;
			return await AddToPlaylist(songs,playlist);
		}

		public async Task<bool> AddToPlaylist (Track song, Playlist playlist, string notes = "")
		{
			try
			{

				if (string.IsNullOrWhiteSpace(playlist.Id))
				{
					playlist.ServiceType = ServiceType;
					await CreatePlaylist(playlist);
				}
				var content = !string.IsNullOrWhiteSpace(notes) ? new PlaylistItemContentDetails
				{
					Note = notes
				} : null;
				var body = new Google.Apis.Youtube.v3.Data.PlaylistItem {
					Snippet = new PlaylistItemSnippet{
						PlaylistId = playlist.Id,
						ResourceId =  new ResourceId{
							Kind = "youtube#video",
							VideoId = song.Id,
						}
					},
					ContentDetails = content
				};
				var resp = await Api.Post<Google.Apis.Youtube.v3.Data.PlaylistItem> (body, "playlistItems?part=id%2CcontentDetails%2Csnippet");
				if (string.IsNullOrWhiteSpace (body.Id))
					return false;
				var id = body.ContentDetails.VideoId;
				var note = GetNotes(body.ContentDetails?.Note) ?? new Notes
				{
					Title = body.Snippet.Title,
				};
				var t = new FullPlaylistTrackData(note.Title,note.Artist,note.AlbumArtist,note.Album, note.Genre)
				{
					AlbumArtwork = new List<AlbumArtwork> { new AlbumArtwork { Url = string.Format("http://img.youtube.com/vi/{0}/0.jpg", id) }},
					MediaType = MediaType.Video,
					ServiceId = Id,
					Id = id,
					ServiceType = ServiceType,
					FileExtension = "mp4",
					TrackId = id,
					PlaylistId = playlist.Id,
					PlaylistEntryId = body.Id,
					SOrder = body.Snippet.Position ?? 1000,
					Disc = note.Disc,
					Year = note.Year,
					Track = note.Track,
				};
				await ProcessPlaylistTracks(new List<FullPlaylistTrackData> { t }, new List<TempPlaylistEntry>());
				await FinalizePlaylists(Id);
				return true;
			}
			catch(Exception ex) {
				LogManager.Shared.Report (ex);
				return false;
			}
		}

		public override Task<bool> SetRating (Track track, int rating)
		{
			//TODO: Support Online rating
			//App.ShowNotImplmented();
			return Task.FromResult(true);
		}

		public override async Task<System.Collections.Generic.List<Song>> GetAlbumDetails (string id)
		{
			App.ShowNotImplmented();
			return null;
		}

		public override Task<MusicPlayer.SearchResults> GetArtistDetails (string id)
		{
			App.ShowNotImplmented();
			return null;
		}
		public override async Task<List<OnlinePlaylistEntry>> GetPlaylistEntries(OnlinePlaylist playlist)
		{
			var tracks = await getPlaylistEntries(playlist);
			return tracks.Select((t) =>
			{
				return new OnlinePlaylistEntry
				{
					OnlineSong = new OnlineSong(t.Title, t.NormalizedTitle)
					{
						Id = t.SongId,
						Artist = t.Artist,
						Album = t.Album,
						AlbumId = t.AlbumId,
						ArtistId = t.Artist,
						Disc = t.Disc,
						Genre = t.Genre,
						Rating = t.Rating,
						TrackCount = t.Track,
						Year = t.Year,
						TrackData = t,
					},
					Id = t.PlaylistEntryId,
					PlaylistId = playlist.Id,
					SOrder = t.SOrder,
					SongId = t.SongId,
				};
			}).ToList();
		}

		public class Notes
		{
			public string Title { get; set; }
			public string Artist { get; set; } = "";
			public string AlbumArtist { get; set; } = "";
			public string Album { get; set; } = "";
			public int Disc { get; set; }
			public int Track { get; set; }
			public int Year { get; set; }
			public string Genre { get; set; } = "";
		}
		public async Task<List<FullPlaylistTrackData>> getPlaylistEntries(Playlist playlist, string nextToken = "")
		{

			var entries = new List<FullPlaylistTrackData> ();
			try{
				var path = "playlistItems?part=id,contentDetails,snippet&maxResults=50&playlistId=" + playlist.Id;
				if (!string.IsNullOrWhiteSpace(nextToken))
				{
					path = $"{path}&pageToken={nextToken}";
				}
				Dictionary<string, string> headers = null;
				if (!string.IsNullOrWhiteSpace(playlist.ServiceExtra))
					headers = new Dictionary<string, string>{{"ETag",playlist.ServiceExtra}};
				int order = 0;
				var playlistResponse = await Api.Get<PlaylistItemListResponse>(path, headers: headers, authenticated: RequiresAuthentication);
				if (playlistResponse.ETag == playlist.ServiceExtra)
					return entries;
				foreach(var item in playlistResponse.Items)
				{
					order ++;
					var id = item.ContentDetails.VideoId;

					var notes = GetNotes(item.ContentDetails.Note) ?? new Notes
					{
						Title = item.Snippet.Title,
					};
					var t = new FullPlaylistTrackData(notes.Title,notes.Artist,notes.AlbumArtist,notes.Album, notes.Genre)
					{
						AlbumArtwork = new List<AlbumArtwork> { new AlbumArtwork { Url = string.Format("http://img.youtube.com/vi/{0}/0.jpg", id) }},
						MediaType = MediaType.Video,
						ServiceId = Id,
						Id = id,
						ServiceType = ServiceType,
						FileExtension = "mp4",
						TrackId = id,
						PlaylistId = playlist.Id,
						PlaylistEntryId = item.Id,
						SOrder = item.Snippet.Position ?? order,
						Disc = notes.Disc,
						Track = notes.Track,
						Year = notes.Year,
					};
					entries.Add(t);
				}
				playlist.ServiceExtra = playlistResponse.ETag;
				if (!string.IsNullOrWhiteSpace(playlistResponse.NextPageToken))
				{
					entries.AddRange(await getPlaylistEntries(playlist, playlistResponse.NextPageToken));
				}
			}

			catch(Exception ex) {
				LogManager.Shared.Report (ex);
			}
			return entries;
		}

		Notes GetNotes(string note)
		{
			try
			{
				return note.ToObject<Notes>();
			}
			catch
			{
				return null;
			}
		}
		public override async Task<bool> RecordPlayack (MusicPlayer.Models.Scrobbling.PlaybackEndedEvent data)
		{
			//throw new NotImplementedException ();
			return true;
		}

		public override async Task<SearchResults> Search (string query)
		{
			var result = new SearchResults();
			try{
	            var path = "search?part=snippet&maxResults=50&q=" + HttpUtility.UrlEncode(query);
				var searchListResponse = await Api.Get<SearchListResponse>(path, authenticated: RequiresAuthentication);

				foreach (var searchResult in searchListResponse.Items)
				{
					switch (searchResult.Id.Kind)
					{
						case "youtube#video":
							var id = searchResult.Id.VideoId;
							var t = new FullTrackData(searchResult.Snippet.Title,"","", "", "")
							{
								AlbumArtwork = new List<AlbumArtwork> { new AlbumArtwork { Url = string.Format("http://img.youtube.com/vi/{0}/0.jpg", id) }},
								MediaType = MediaType.Video,
								ServiceId = Id,
								Id = id,
								ServiceType = ServiceType,
								FileExtension = "mp4"
							};
							result.Videos.Add(new OnlineSong(t.Title, t.NormalizedTitle)
							{
								Id = t.SongId,
								Artist = t.Artist,
								Album = t.Album,
								AlbumId = t.AlbumId,
								ArtistId = t.Artist,
								Disc = t.Disc,
								Genre = t.Genre,
								Rating = t.Rating,
								TrackCount = t.Track,
								Year = t.Year,
								TrackData = t,
							});
							//items.Add(new MediaItem
							//{
							//	Id = id,
							//	MediaType = MediaType.Youtube,
							//	Title = searchResult.Snippet.Title,
							//	DownloadUrl = string.Format("http://www.youtube.com/watch?v={0}", id),
							//	OrigionalUrl = string.Format("http://www.youtube.com/watch?v={0}", id),
							//	ImageUrl = string.Format("http://img.youtube.com/vi/{0}/0.jpg", id),
							//});
							break;

						case "youtube#channel":
							//channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
							break;

						case "youtube#playlist":
							result.Playlists.Add (new OnlinePlaylist {
								Id = searchResult.Id.PlaylistId,
								Name = searchResult.Snippet.Title,
								ServiceId = Id,
								Description = searchResult.Snippet.Description,
								AllArtwork = searchResult.Snippet.Thumbnails?.Select(x=> new AlbumArtwork{Url = x.Value.Url}).ToArray() ?? new AlbumArtwork[0],
							});	
							break;
					}
				}
			}
			catch(Exception ex) {
				LogManager.Shared.Report (ex);
			}
			return result;
		}

		public async override Task<bool> AddToLibrary (OnlinePlaylist onlinePlaylist)
		{
			var tracks = await getPlaylistEntries(onlinePlaylist);
			if (!RequiresAuthentication)
			{
				var playlist = new Playlist(onlinePlaylist.Name)
				{
					ServiceId = Api.Identifier,
					ServiceType = ServiceType,
					Id = onlinePlaylist.Id,
				};
				Database.Main.InsertOrReplace(playlist);
				//TODO: Decide what to do here. If you uncomment this line, all tracks show up in the library. Need to determine if you are adding the playlist or the songs.
				//await ProcessTracks(tracks.OfType<FullTrackData>().ToList());
				await ProcessPlaylistTracks(tracks, new List<TempPlaylistEntry>());
				await FinalizePlaylists(Id);
				return true;
			}
			return await AddToPlaylist(tracks.OfType<Track>().ToList(), onlinePlaylist.Name);
		}

		public override async Task<bool> AddToLibrary (RadioStation station)
		{
			App.ShowNotImplmented();
			return false;
		}


		public override async Task<bool> AddToLibrary (OnlineSong song)
		{
			if (!RequiresAuthentication)
			{
				return await MusicProvider.ProcessTracks(new List<FullTrackData> { 
					new FullTrackData(song.Name,song.TrackData.Artist,song.TrackData.AlbumArtist,song.TrackData.Album,song.TrackData.Genre){
						AlbumArtwork = new List<AlbumArtwork> { new AlbumArtwork { Url = string.Format("http://img.youtube.com/vi/{0}/0.jpg", song.TrackData.Id) }},
						MediaType = MediaType.Video,
						ServiceId = Api.Identifier,
						Id = song.TrackData.Id,
						ServiceType = ServiceType.YouTube,
						FileExtension = "mp3",
						Disc = song.TrackData.Disc,
						Track = song.TrackData.Track,
						Year = song.TrackData.Year,
					}});
			}
			var notes = new Notes
			{
				Album = song.TrackData.Album,
				AlbumArtist = song.TrackData.AlbumArtist,
				Artist = song.TrackData.Artist,
				Disc = song.TrackData.Disc,
				Title = song.Name,
				Genre = song.TrackData.Genre,
				Track = song.TrackData.Track,
				Year = song.TrackData.Year,
			};

			var plistId = await GetOrCreatePlaylistId();
			var playlist = new Playlist () {
				Id = plistId
			};
			return await AddToPlaylist (song.TrackData,playlist , notes.ToJson());
		}

		public override async Task<bool> AddToLibrary (OnlineAlbum album)
		{
			App.ShowNotImplmented();
			return false;
		}

		public override Task<bool> AddToLibrary (Track track)
		{
			return AddToLibraryPlaylist (new List<Track>{ track });
		}


		async Task<bool> AddToLibraryPlaylist (System.Collections.Generic.List<Track> songs)
		{
			var plistId = await GetOrCreatePlaylistId();
			var playlist = new Playlist () {
				Id = plistId
			};
			return await AddToPlaylist (songs, playlist);
		}



		#endregion
	}
}

