using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using MusicPlayer.Models.Scrobbling;
using Punchclock;
using Xamarin;
using System.Net.Http;
using System.Threading;
using System.IO;
using Localizations;

namespace MusicPlayer.Api
{

	[Flags]
	public enum MediaProviderCapabilities
	{
		None,
		Searchable,
		Radio,
		NewReleases,
		Trending,
		Playlists,
	}

	public abstract class MusicProvider
	{
		public SimpleAuth.Api Api { get; }

		public MusicProvider(SimpleAuth.Api api)
		{
			Api = api;
		}

		public abstract ServiceType ServiceType { get; }

		public abstract bool RequiresAuthentication {get;}

		public abstract string Id { get; }

		public abstract MediaProviderCapabilities[] Capabilities { get; }

		Task<bool> syncTask;
		object loacker = new object();
		public Task<bool> SyncDatabase()
		{
			lock (loacker) {
				if (syncTask == null || syncTask.IsCompleted)
					syncTask = Task.Run (async () =>{
						try{
							return await Sync ();
						}
						catch(Exception ex)
						{
							LogManager.Shared.Report(ex);
							return false;
						}
					});
			}
			return syncTask;
		}

		protected abstract Task<bool> Sync();

		public abstract Task<bool> Resync();
		public abstract Task<Uri> GetPlaybackUri(Track track);


		public abstract Task<DownloadUrlData> GetDownloadUri(Track track);

		public abstract Task<bool> LoadRadioStation(RadioStation station, bool isContinuation);

		public abstract Task<RadioStation> CreateRadioStation(string name, RadioStationSeed seed);
		public abstract Task<RadioStation> CreateRadioStation(string name, Track track);
		public abstract Task<RadioStation> CreateRadioStation(string name, AlbumIds track);
		public abstract Task<RadioStation> CreateRadioStation(string name, ArtistIds track);

		public abstract Task<bool> DeleteRadioStation(RadioStation station);

		public abstract Task<bool> DeletePlaylist(Playlist playlist);

		public abstract Task<bool> DeletePlaylistSong(PlaylistSong song);

		public abstract Task<bool> MoveSong(PlaylistSong song, string previousId, string nextId, int index);

		public abstract Task<bool> AddToPlaylist(List<Track> songs, Playlist playlist);

		public abstract Task<bool> AddToPlaylist(List<Track> songs, string playlistName);

		public abstract Task<bool>  SetRating(Track track, int rating);

		public abstract Task<List<Song>> GetAlbumDetails(string id);

		public abstract Task<SearchResults> GetArtistDetails(string id);

		public abstract Task<List<OnlinePlaylistEntry>>  GetPlaylistEntries(OnlinePlaylist playlist);

		public abstract Task<bool> RecordPlayack(PlaybackEndedEvent data);

		public abstract Task<SearchResults> Search(string query);

		public abstract Task<bool> AddToLibrary(OnlinePlaylist playlist);

		public abstract Task<bool> AddToLibrary(RadioStation station);

		public abstract Task<bool> AddToLibrary(OnlineSong song);

		public abstract Task<bool> AddToLibrary(OnlineAlbum album);
		public abstract Task<bool> AddToLibrary(Track track);

		public abstract Task<string> GetShareUrl (Song song);

		public abstract string Email { get; }

		public virtual Task Logout ()
		{
			Api.ResetData ();
			return RemoveApi (Id);
		}

		//Static stuff used for processing data

		public static OperationQueue SyncRequestQueue = new OperationQueue(4);

		static readonly OperationQueue ProcessQueue = new OperationQueue(2);

		protected static async Task<bool> ProcessTracks(List<FullTrackData> tracks)
		{
			if (tracks?.Any() != true)
				return true;
			return await ProcessQueue.Enqueue(1, () => Task.Run(() =>
			{
				var artists = new Dictionary<string, Artist>();
				var artistIDs = new Dictionary<string, ArtistIds>();
				var artistArtworks = new Dictionary<string, ArtistArtwork>();
				var albums = new Dictionary<string, Album>();
				var albumIds = new Dictionary<string, AlbumIds>();
				var albumsArtworks = new Dictionary<string, AlbumArtwork>();
				var songs = new Dictionary<string, Song>();
				var genres = new Dictionary<string, Genre>();

				tracks.ForEach(track =>
				{
					try{
						var artist = new Artist(track.Artist, track.ArtistId);
						artists[artist.Id] = artist;
						if (!string.IsNullOrWhiteSpace(track.ArtistServerId))
							artistIDs[track.ArtistServerId] =
								new ArtistIds
								{
									Id = track.ArtistServerId,
									ArtistId = track.ArtistId,
									ServiceType = track.ServiceType,
								};

						track.ArtistArtwork.ForEach(x =>
						{
							x.ArtistId = artist.Id;
							x.ServiceType = track.ServiceType;
							x.SetId();
							artistArtworks[x.Id] = x;
						});
						var album = new Album(track.Album, track.NormalizedAlbum)
						{
							Id = track.AlbumId,
							Year = track.Year,
							IsCompilation = false,
							ArtistId = track.ArtistId,
							AlbumArtist = track.AlbumArtist,
							Artist = artist.Name,
						};
						albums[album.Id] = album;
						track.AlbumArtwork.ForEach(x =>
						{
							x.AlbumId = album.Id;
							x.ServiceType = track.ServiceType;
							x.SetId();
							albumsArtworks[x.Id] = x;
						});

						if (!string.IsNullOrWhiteSpace(track.AlbumServerId))
							albumIds[track.AlbumServerId] = new AlbumIds
							{
								ServiceType = track.ServiceType,
								Id = track.AlbumServerId,
								AlbumId = track.AlbumId,
							};


						var song = new Song(track.Title, track.NormalizedTitle)
						{
							AlbumId = track.AlbumId,
							Id = track.SongId,
							ArtistId = track.ArtistId,
							Artist = track.DisplayArtist,
							Album = track.Album,
							Genre = track.Genre,
							Rating = track.Rating,
							PlayedCount = track.PlayCount,
							Disc = track.Disc,
							Track = track.Track,
							Year = track.Year,
						};
						songs[song.Id] = song;

						var genre = new Genre(track.Genre);
						if (!string.IsNullOrWhiteSpace(genre.Id))
							genres[genre.Id] = genre;
					}
					catch(Exception e){
						Console.WriteLine(e);
					}
				});

				Database.Main.InsertOrReplaceAll(artists.Values);
				Database.Main.InsertOrReplaceAll(artistIDs.Values);
				Database.Main.InsertOrReplaceAll(artistArtworks.Values);
				Database.Main.InsertOrReplaceAll(albums.Values);
				Database.Main.InsertOrReplaceAll(albumIds.Values);
				Database.Main.InsertOrReplaceAll(albumsArtworks.Values);
				Database.Main.InsertOrReplaceAll(songs.Values);
				Database.Main.InsertOrReplaceAll(genres.Values);
				Database.Main.InsertOrReplaceAll(tracks, typeof (Track));
				#if __IOS__
				NativeIndexer.Shared.Index(songs.Values);
				#endif
				return true;
			}));
		}

		public static async Task SetOffline(Track track)
		{
			//await SetOfflineEverything();
			await Task.Run(() =>
			{
				Database.Main.RunInTransaction((connection) =>
				{
					var start = DateTime.Now;

					connection.Execute(@"
Update Song
Set TrackCount = (select count(*) from Track where SongId = Song.Id),
OfflineCount = (select count(id) from Track where SongId = Song.Id and ServiceType  in ('6','2')),
ServiceTypesString = (select group_concat(ServiceType, ',') from Track where SongId = Song.Id),
MediaTypesString = (select group_concat(MediaType, ',') from Track where SongId = Song.Id)
Where Id = ?
", track.SongId);


					connection.Execute(@"
Update Artist
set SongCount = (select count(*) from Song where ArtistId = Artist.Id),
AlbumCount = (select count(distinct Id) from Album where ArtistId = Artist.Id),
OfflineCount = (select count(id) from Track where ArtistId = Artist.Id and ServiceType  in ('6','2'))
Where Id = ?
", track.ArtistId);

					connection.Execute(@"
Update Album
set TrackCount = (select count(*) from Song where AlbumId = Album.Id),
IsCompilation = (select count(distinct ArtistID) from Song where AlbumId = Album.Id) > 1,
OfflineCount = (select count(id) from Track where AlbumId = Album.Id and ServiceType  in ('6','2'))
Where Id = ?
", track.AlbumId);


					connection.Execute(@"
Update Genre
set SongCount = (select count(*) from Song where Genre = Genre.Id),
AlbumCount = (select count(distinct AlbumId) from Song where Genre = Genre.Id),
OfflineCount = (select count(id) from Track where Genre = Genre.Id and ServiceType  in ('6','2'))
where Id = ?
", track.Genre);


					connection.Execute(@"Delete From Artist where SongCount = 0");

					connection.Execute(@"Delete From Album where TrackCount = 0");

					connection.Execute(@"Delete From Genre where SongCount = 0");

					connection.Execute(@"Delete From Song where TrackCount = 0");

					var end = DateTime.Now - start;
					Debug.WriteLine($"grouping in database took {end.TotalMilliseconds}");
				});

				Database.Main.ClearMemoryStore();
				NotificationManager.Shared.ProcSongDatabaseUpdated();
			});
		}

		/// <summary>
		/// Deletes all the tracks associated from the database which are associated with this service
		/// </summary>
		/// <returns>The tracks.</returns>
		/// <param name="id">The ID of the Api whose tracks you want to delete.</param>
		internal static async Task RemoveTracks(string id)
		{
			await Database.Main.ExecuteAsync("delete from track where ServiceId = ?", id);
			await FinalizeProcessing(id);
		}

//		internal static async void RemoveApi(ServiceType serviceType)
//		{
//			using (new Spinner("Updating Database")) {
//				await Database.Main.ExecuteAsync("delete from track where ServiceType = ?",(int)serviceType);
//				await FinalizeProcessing();
//			}
//		}
		internal static async Task RemoveApi(string  id)
		{
			using (new Spinner(Strings.LoggingOut))
				await RemoveTracks(id);
		}

		public static Task SetOfflineEverything()
		{
			Database.Main.Execute(@"Delete From Track where Deleted = 1");

			return FinalizeProcessing("");
		}
		protected static async Task FinalizeProcessing(string id)
		{
			await Task.Run(() =>
			{
				Database.Main.RunInTransaction((connection) =>
				{
					try{
					var start = DateTime.Now;
					
					connection.Execute(@"Delete From Track where Deleted = 1 and ServiceId = ?",id);
					connection.Execute(@"
Update Song
Set TrackCount = (select count(*) from Track where SongId = Song.Id),
OfflineCount = (select count(id) from Track where SongId = Song.Id and ServiceType  in ('6','2')),
ServiceTypesString = (select group_concat(ServiceType, ',') from Track where SongId = Song.Id),
MediaTypesString = (select group_concat(MediaType, ',') from Track where SongId = Song.Id)");

					connection.Execute(@"Delete From Song where TrackCount = 0");

					connection.Execute(@"
Update Artist
set SongCount = (select count(*) from Song where ArtistId = Artist.Id),
AlbumCount = (select count(distinct Id) from Album where ArtistId = Artist.Id),
OfflineCount = (select count(id) from Track where ArtistId = Artist.Id and ServiceType  in ('6','2'))");

					connection.Execute(@"
Update Album
set TrackCount = (select count(*) from Song where AlbumId = Album.Id),
IsCompilation = (select count(distinct ArtistID) from Song where AlbumId = Album.Id) > 1,
OfflineCount = (select count(id) from Track where AlbumId = Album.Id and ServiceType in ('6','2'))");


					connection.Execute(@"
Update Genre
set SongCount = (select count(*) from Song where Genre = Genre.Id),
AlbumCount = (select count(distinct AlbumId) from Song where Genre = Genre.Id),
OfflineCount = (select count(id) from Track where Genre = Genre.Id and ServiceType  in ('6','2'))");


					connection.Execute(@"Delete From Artist where SongCount = 0");

					connection.Execute(@"Delete From Album where TrackCount = 0");

					connection.Execute(@"Delete From Genre where SongCount = 0");

					connection.Execute(@"Delete From Song where TrackCount = 0");

					var end = DateTime.Now - start;
					Debug.WriteLine($"grouping in database took {end.TotalMilliseconds}");
							}
							catch(Exception ex)
							{
								Console.WriteLine(ex);
							}
				});

				Database.Main.ClearMemoryStore();
				NotificationManager.Shared.ProcSongDatabaseUpdated();
			});
		}

		protected static async Task<bool> ProcessPlaylists(List<Playlist> playlists)
		{
			if (playlists?.Any() != true)
				return true;
			return await ProcessQueue.Enqueue(1, () => Task.Run(() =>
			{
				try
				{
					Database.Main.InsertOrReplaceAll(playlists);
					return true;
				}
				catch (Exception ex)
				{
					LogManager.Shared.Report(ex);
				}
				return false;
			}));
		}

		protected static async Task<bool> ProcessPlaylistTracks(List<FullPlaylistTrackData> tracks,
			List<TempPlaylistEntry> playlistEntries)
		{
			if (tracks?.Any() != true && playlistEntries?.Any() != true)
				return true;
			return await ProcessQueue.Enqueue(1, () => Task.Run(async() =>
			{
				var artists = new Dictionary<string, Artist>();
				var artistIDs = new Dictionary<string, ArtistIds>();
				var artistArtworks = new Dictionary<string, ArtistArtwork>();
				var albums = new Dictionary<string, Album>();
				var albumIds = new Dictionary<string, AlbumIds>();
				var albumsArtworks = new Dictionary<string, AlbumArtwork>();
				var songs = new Dictionary<string, TempSong>();
				var genres = new Dictionary<string, Genre>();
				var playlistSongs = new List<PlaylistSong>();

				tracks?.ForEach(track =>
				{
					var artist = new Artist(track.Artist, track.ArtistId);
					if (!string.IsNullOrWhiteSpace(track.ArtistServerId))
						artistIDs[track.ArtistServerId] =
							new ArtistIds
							{
								Id = track.ArtistServerId,
								ArtistId = track.ArtistId,
								ServiceType = track.ServiceType,
							};

					track.ArtistArtwork.ForEach(x =>
					{
						x.ArtistId = artist.Id;
						x.ServiceType = track.ServiceType;
						x.SetId();
						artistArtworks[x.Id] = x;
					});
					var album = new Album(track.Album, track.NormalizedAlbum)
					{
						Id = track.AlbumId,
						Year = track.Year,
						IsCompilation = false,
						ArtistId = track.ArtistId,
						AlbumArtist = track.AlbumArtist,
					};
					albums[album.Id] = album;
					track.AlbumArtwork.ForEach(x =>
					{
						x.AlbumId = album.Id;
						x.ServiceType = track.ServiceType;
						x.SetId();
						albumsArtworks[x.Id] = x;
					});

					if (!string.IsNullOrWhiteSpace(track.AlbumServerId))
						albumIds[track.AlbumServerId] = new AlbumIds
						{
							ServiceType = track.ServiceType,
							Id = track.AlbumServerId,
							AlbumId = track.AlbumId,
						};

					playlistSongs.Add(new PlaylistSong
					{
						SongId = track.SongId,
						SOrder = track.SOrder,
						PlaylistId = track.PlaylistId,
						Id = track.PlaylistEntryId,
						Deleted = track.Deleted,
						LastUpdate = track.LastUpdated,
						ServiceId = track.ServiceId,
					});

					var song = new TempSong(track.Title, track.NormalizedTitle)
					{
						ParentId = track.ParentId,
						AlbumId = track.AlbumId,
						Id = track.SongId,
						ArtistId = track.ArtistId,
						Artist = track.DisplayArtist,
						Album = track.Album,
						Genre = track.Genre,
						Rating = track.Rating,
						PlayedCount = track.PlayCount,
					};
					songs[song.Id] = song;

					var genre = new Genre(track.Genre);
					if (!string.IsNullOrWhiteSpace(genre.Id))
						genres[genre.Id] = genre;
				});

				Database.Main.InsertOrReplaceAll(artists.Values, typeof (TempArtist));
				Database.Main.InsertOrReplaceAll(artistIDs.Values, typeof (TempArtistIds));
				Database.Main.InsertOrReplaceAll(artistArtworks.Values, typeof (TempArtistArtwork));
				Database.Main.InsertOrReplaceAll(albums.Values, typeof (TempAlbum));
				Database.Main.InsertOrReplaceAll(albumIds.Values, typeof (TempAlbumIds));
				Database.Main.InsertOrReplaceAll(albumsArtworks.Values, typeof (TempAlbumArtwork));
				Database.Main.InsertOrReplaceAll(songs.Values, typeof (TempSong));
				Database.Main.InsertOrReplaceAll(genres.Values, typeof (TempGenre));
				Database.Main.InsertOrReplaceAll(tracks, typeof (TempTrack));
				Database.Main.InsertOrReplaceAll(playlistSongs);

				//Now Handle items in Playlist
				if(playlistEntries?.Any() == true)
					Database.Main.InsertOrReplaceAll(playlistEntries);
				Database.Main.RunInTransaction((connection) =>
				{
					connection.Execute(@"insert or replace into PlaylistSong (Id,PlaylistId,SOrder,SongId,LastUpdate)
select PlaylistEntryId,
PlaylistId,
SOrder,
(select SongId from Track where Id = TrackId) as SongId,
LastUpdate
from TempPlaylistEntry
where (select SongId from Track where Id = TrackId) is not null");
					connection.Execute("delete from TempPlaylistEntry");
				});

				await ProcessTempData();
                Database.Main.ClearMemory<PlaylistSong>();
				Database.Main.ClearMemoryStore();
				return true;
			}));
		}


		internal static async Task FinalizePlaylists(string  id)
		{
			await Task.Run(() =>
			{
				Database.Main.RunInTransaction((connection) =>
				{
					var start = DateTime.Now;


					connection.Execute(@"Delete From PlaylistSong where Deleted = 1 and ServiceId = ?",id);
					connection.Execute(@"Delete From Playlist where Deleted = 1 and ServiceId = ?",id);
					connection.Execute(@"
Update PlaylistSong
Set OfflineCount = (select count(id) from Track where SongId = PlaylistSong.SongId and ServiceType in ('6','2'))");
					connection.Execute(@"
Update Playlist
Set SongCount = (select count(*) from PlaylistSong where PlaylistId = Playlist.Id),
LastSync = (select Max(LastUpdate) from PlaylistSong where PlaylistId = Playlist.Id),
OfflineCount = (select count(id) from PlaylistSong where PlaylistId = Playlist.Id and OfflineCount > 0)");

					var end = DateTime.Now - start;
					Debug.WriteLine($"grouping playlist in database took {end.TotalMilliseconds}");
				});

				Database.Main.ClearMemory<Playlist>();
				Database.Main.ClearMemory<PlaylistSong>();
				Database.Main.ClearMemoryStore();
				NotificationManager.Shared.ProcPlaylistDatabaseUpdated();
			});
		}

		protected static async Task ProcessTempData()
		{
			await Task.Run(() =>
			{
				Database.Main.RunInTransaction((connection) =>
				{
					var start = DateTime.Now;

					connection.Execute(@"Delete From TempTrack where Deleted = 1");
					connection.Execute(@"
Update TempSong
Set TrackCount = (select count(*) from TempTrack where SongId = TempSong.Id),
ServiceTypesString = (select group_concat(ServiceType, ',') from TempTrack where SongId = TempSong.Id),
MediaTypesString = (select group_concat(MediaType, ',') from TempTrack where SongId = TempSong.Id)");

					connection.Execute(@"Delete From TempSong where TrackCount = 0");

					connection.Execute(@"
Update TempArtist
set SongCount = (select count(*) from TempSong where ArtistId = TempArtist.Id),
AlbumCount = (select count(distinct Id) from TempAlbum where ArtistId = TempArtist.Id)");

					connection.Execute(@"
Update TempAlbum
set TrackCount = (select count(*) from TempSong where AlbumId = TempAlbum.Id),
IsCompilation = (select count(distinct ArtistID) from TempSong where AlbumId = TempAlbum.Id) > 1");


					connection.Execute(@"
Update TempGenre
set SongCount = (select count(*) from TempSong where Genre = TempGenre.Id),
AlbumCount = (select count(distinct AlbumId) from TempSong where Genre = TempGenre.Id)");


					connection.Execute(@"Delete From TempArtist where SongCount = 0");

					connection.Execute(@"Delete From TempAlbum where TrackCount = 0");

					connection.Execute(@"Delete From TempGenre where SongCount = 0");

					connection.Execute(@"Delete From TempSong where TrackCount = 0");

					var end = DateTime.Now - start;
					Debug.WriteLine($"grouping Temp in database took {end.TotalMilliseconds}");
				});

				Database.Main.ClearMemoryStore();
				NotificationManager.Shared.ProcSongDatabaseUpdated();
			});

		}

		protected static async Task<bool> ProcessRadioStations(List<RadioStation> stations, List<RadioStationArtwork> artwork)
		{
			if (stations?.Any() != true)
				return true;
			return await ProcessQueue.Enqueue(1, () => Task.Run(() =>
			{
				try
				{
					if(artwork?.Any() == true)
						Database.Main.InsertOrReplaceAll(artwork);
					if(stations?.Any() == true)
						Database.Main.InsertOrReplaceAll(stations);
					var seeds = stations?.SelectMany(x => x.StationSeeds).ToArray();
					if(seeds?.Any() == true)
						Database.Main.InsertOrReplaceAll(seeds);

					Database.Main.ClearMemory<RadioStation>();
					NotificationManager.Shared.ProcRadioDatabaseUpdated();
					return true;
					//TODO: Delete old crap!
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
				}
				return false;
			}));
		}

		protected static async Task<bool> ProcessRadioStationTracks(List<FullPlaylistTrackData> tracks,
			List<TempRadioStationSong> playlistEntries)
		{
			if (tracks?.Any() != true && playlistEntries.Any() != true)
				return true;
			return await ProcessQueue.Enqueue(1, () => Task.Run(() =>
			{
				var artists = new Dictionary<string, Artist>();
				var artistIDs = new Dictionary<string, ArtistIds>();
				var artistArtworks = new Dictionary<string, ArtistArtwork>();
				var albums = new Dictionary<string, Album>();
				var albumIds = new Dictionary<string, AlbumIds>();
				var albumsArtworks = new Dictionary<string, AlbumArtwork>();
				var songs = new Dictionary<string, TempSong>();
				var genres = new Dictionary<string, Genre>();
				var playlistSongs = new List<RadioStationSong>();

				tracks.ForEach(track =>
				{
					var artist = new Artist(track.Artist, track.ArtistId);
					if (!string.IsNullOrWhiteSpace(track.ArtistServerId))
						artistIDs[track.ArtistServerId] =
							new ArtistIds
							{
								Id = track.ArtistServerId,
								ArtistId = track.ArtistId,
								ServiceType = track.ServiceType,
							};

					track.ArtistArtwork.ForEach(x =>
					{
						x.ArtistId = artist.Id;
						x.ServiceType = track.ServiceType;
						x.SetId();
						artistArtworks[x.Id] = x;
					});
					var album = new Album(track.Album, track.NormalizedAlbum)
					{
						Id = track.AlbumId,
						Year = track.Year,
						IsCompilation = false,
						ArtistId = track.ArtistId,
						AlbumArtist = track.AlbumArtist,
					};
					albums[album.Id] = album;
					track.AlbumArtwork.ForEach(x =>
					{
						x.AlbumId = album.Id;
						x.ServiceType = track.ServiceType;
						x.SetId();
						albumsArtworks[x.Id] = x;
					});

					if (!string.IsNullOrWhiteSpace(track.AlbumServerId))
						albumIds[track.AlbumServerId] = new AlbumIds
						{
							ServiceType = track.ServiceType,
							Id = track.AlbumServerId,
							AlbumId = track.AlbumId,
						};

					playlistSongs.Add(new RadioStationSong
					{
						SongId = track.SongId,
						SOrder = track.SOrder,
						StationId = track.PlaylistId,
						Id = track.PlaylistEntryId,
					});

					var song = new TempSong(track.Title, track.NormalizedTitle)
					{
						ParentId = track.ParentId,
						AlbumId = track.AlbumId,
						Id = track.SongId,
						ArtistId = track.ArtistId,
						Artist = track.DisplayArtist,
						Album = track.Album,
						Genre = track.Genre,
						Rating = track.Rating,
						PlayedCount = track.PlayCount,
						Track = track.Track,
						Year = track.Year,
						Disc = track.Disc,
					};
					songs[song.Id] = song;

					var genre = new Genre(track.Genre);
					if (!string.IsNullOrWhiteSpace(genre.Id))
						genres[genre.Id] = genre;
				});

				Database.Main.InsertOrReplaceAll(artists.Values, typeof (TempArtist));
				Database.Main.InsertOrReplaceAll(artistIDs.Values, typeof (TempArtistIds));
				Database.Main.InsertOrReplaceAll(artistArtworks.Values, typeof (TempArtistArtwork));
				Database.Main.InsertOrReplaceAll(albums.Values, typeof (TempAlbum));
				Database.Main.InsertOrReplaceAll(albumIds.Values, typeof (TempAlbumIds));
				Database.Main.InsertOrReplaceAll(albumsArtworks.Values, typeof (TempAlbumArtwork));
				Database.Main.InsertOrReplaceAll(songs.Values, typeof (TempSong));
				Database.Main.InsertOrReplaceAll(genres.Values, typeof (TempGenre));
				if(tracks?.Any() == true)
					Database.Main.InsertOrReplaceAll(tracks, typeof (TempTrack));
				Database.Main.InsertOrReplaceAll(playlistSongs);

				//Now Handle items in Playlist
				Database.Main.InsertOrReplaceAll(playlistEntries);
				Database.Main.RunInTransaction((connection) =>
				{
					connection.Execute(@"insert or replace into RadioStationSong (Id,StationId,SOrder,SongId)
select PlaylistEntryId,
PlaylistId,
SOrder,
(select SongId from Track where Id = TrackId) as SongId
from TempRadioStationSong
where (select SongId from Track where Id = TrackId) is not null");
					connection.Execute("delete from TempRadioStationSong");
				});

				ProcessTempData();
				return true;
			}));
		}



		protected static async Task<bool> ProcessOnlinePlaylistTracks(List<OnlinePlaylistEntry> entries,string parentId)
		{
			if (entries?.Any() != true)
				return true;
			return await ProcessQueue.Enqueue(1, () => Task.Run(() =>
			{
				var artists = new Dictionary<string, Artist>();
				var artistIDs = new Dictionary<string, ArtistIds>();
				var artistArtworks = new Dictionary<string, ArtistArtwork>();
				var albums = new Dictionary<string, Album>();
				var albumIds = new Dictionary<string, AlbumIds>();
				var albumsArtworks = new Dictionary<string, AlbumArtwork>();
				var songs = new Dictionary<string, TempSong>();
				var genres = new Dictionary<string, Genre>();
				var tracks = new List<FullTrackData>();
				entries.ForEach(p =>
				{

					p.PlaylistId = parentId;
					var track = p.OnlineSong.TrackData;
					tracks.Add(track);
					var artist = new Artist(track.Artist, track.ArtistId);
					if (!string.IsNullOrWhiteSpace(track.ArtistServerId))
						artistIDs[track.ArtistServerId] =
							new ArtistIds
							{
								Id = track.ArtistServerId,
								ArtistId = track.ArtistId,
								ServiceType = track.ServiceType,
							};

					track.ArtistArtwork.ForEach(x =>
					{
						x.ArtistId = artist.Id;
						x.ServiceType = track.ServiceType;
						x.SetId();
						artistArtworks[x.Id] = x;
					});
					var album = new Album(track.Album, track.NormalizedAlbum)
					{
						Id = track.AlbumId,
						Year = track.Year,
						IsCompilation = false,
						ArtistId = track.ArtistId,
						AlbumArtist = track.AlbumArtist,
					};
					albums[album.Id] = album;
					track.AlbumArtwork.ForEach(x =>
					{
						x.AlbumId = album.Id;
						x.ServiceType = track.ServiceType;
						x.SetId();
						albumsArtworks[x.Id] = x;
					});

					if (!string.IsNullOrWhiteSpace(track.AlbumServerId))
						albumIds[track.AlbumServerId] = new AlbumIds
						{
							ServiceType = track.ServiceType,
							Id = track.AlbumServerId,
							AlbumId = track.AlbumId,
						};
					

					var song = new TempSong(track.Title, track.NormalizedTitle)
					{
						ParentId = track.ParentId,
						AlbumId = track.AlbumId,
						Id = track.SongId,
						ArtistId = track.ArtistId,
						Artist = track.DisplayArtist,
						Album = track.Album,
						Genre = track.Genre,
						Rating = track.Rating,
						PlayedCount = track.PlayCount,
						Track = track.Track,
						Year = track.Year,
						Disc = track.Disc,
					};
					songs[song.Id] = song;

					var genre = new Genre(track.Genre);
					if (!string.IsNullOrWhiteSpace(genre.Id))
						genres[genre.Id] = genre;
				});

				Database.Main.InsertOrReplaceAll(artists.Values, typeof(TempArtist));
				Database.Main.InsertOrReplaceAll(artistIDs.Values, typeof(TempArtistIds));
				Database.Main.InsertOrReplaceAll(artistArtworks.Values, typeof(TempArtistArtwork));
				Database.Main.InsertOrReplaceAll(albums.Values, typeof(TempAlbum));
				Database.Main.InsertOrReplaceAll(albumIds.Values, typeof(TempAlbumIds));
				Database.Main.InsertOrReplaceAll(albumsArtworks.Values, typeof(TempAlbumArtwork));
				Database.Main.InsertOrReplaceAll(songs.Values, typeof(TempSong));
				Database.Main.InsertOrReplaceAll(genres.Values, typeof(TempGenre));
				Database.Main.InsertOrReplaceAll(tracks, typeof(TempTrack));
				Database.Main.InsertOrReplaceAll(entries,typeof(TempPlaylistSong));

				//Now Handle items in Playlist

				ProcessTempData();
				return true;
			}));
		}
		protected static async Task<bool> ProcessAlbumTracks(List<FullTrackData> tracks)
		{
			if (tracks?.Any() != true)
				return true;
			return await ProcessQueue.Enqueue(1, () => Task.Run(async () =>
			{
				var artists = new Dictionary<string, Artist>();
				var artistIDs = new Dictionary<string, ArtistIds>();
				var artistArtworks = new Dictionary<string, ArtistArtwork>();
				var albums = new Dictionary<string, Album>();
				var albumIds = new Dictionary<string, AlbumIds>();
				var albumsArtworks = new Dictionary<string, AlbumArtwork>();
				var songs = new Dictionary<string, TempSong>();
				var genres = new Dictionary<string, Genre>();

				tracks.ForEach(track =>
				{
					var artist = new Artist(track.Artist, track.ArtistId);
					if (!string.IsNullOrWhiteSpace(track.ArtistServerId))
						artistIDs[track.ArtistServerId] =
							new ArtistIds
							{
								Id = track.ArtistServerId,
								ArtistId = track.ArtistId,
								ServiceType = track.ServiceType,
							};

					track.ArtistArtwork.ForEach(x =>
					{
						x.ArtistId = artist.Id;
						x.ServiceType = track.ServiceType;
						x.SetId();
						artistArtworks[x.Id] = x;
					});
					var album = new Album(track.Album, track.NormalizedAlbum)
					{
						Id = track.AlbumId,
						Year = track.Year,
						IsCompilation = false,
						ArtistId = track.ArtistId,
						AlbumArtist = track.AlbumArtist,
					};
					albums[album.Id] = album;
					track.AlbumArtwork.ForEach(x =>
					{
						x.AlbumId = album.Id;
						x.ServiceType = track.ServiceType;
						x.SetId();
						albumsArtworks[x.Id] = x;
					});

					if (!string.IsNullOrWhiteSpace(track.AlbumServerId))
						albumIds[track.AlbumServerId] = new AlbumIds
						{
							ServiceType = track.ServiceType,
							Id = track.AlbumServerId,
							AlbumId = track.AlbumId,
						};

					var song = new TempSong(track.Title, track.NormalizedTitle)
					{
						ParentId = track.ParentId,
						AlbumId = track.AlbumId,
						Id = track.SongId,
						ArtistId = track.ArtistId,
						Artist = track.DisplayArtist,
						Album = track.Album,
						Genre = track.Genre,
						Rating = track.Rating,
						PlayedCount = track.PlayCount,
						Track = track.Track,
						Year = track.Year,
						Disc = track.Disc,
					};
					songs[song.Id] = song;

					var genre = new Genre(track.Genre);
					if (!string.IsNullOrWhiteSpace(genre.Id))
						genres[genre.Id] = genre;
				});

				Database.Main.InsertOrReplaceAll(artists.Values, typeof(TempArtist));
				Database.Main.InsertOrReplaceAll(artistIDs.Values, typeof(TempArtistIds));
				Database.Main.InsertOrReplaceAll(artistArtworks.Values, typeof(TempArtistArtwork));
				Database.Main.InsertOrReplaceAll(albums.Values, typeof(TempAlbum));
				Database.Main.InsertOrReplaceAll(albumIds.Values, typeof(TempAlbumIds));
				Database.Main.InsertOrReplaceAll(albumsArtworks.Values, typeof(TempAlbumArtwork));
				Database.Main.InsertOrReplaceAll(songs.Values, typeof(TempSong));
				Database.Main.InsertOrReplaceAll(genres.Values, typeof(TempGenre));
				Database.Main.InsertOrReplaceAll(tracks, typeof(TempTrack));

				await ProcessTempData();
				return true;
			}));
		}

		public static Task<bool> AddTemp(OnlineSong onlineSong)
		{
			return AddTemp(new List<OnlineSong> {onlineSong});
		}

		public static Task<bool> AddTemp(OnlineRadioStation onlineSong)
		{
			return Task.Run(() => Database.Main.InsertOrReplace(onlineSong) > 0);
		}

		public static async Task<bool> AddTemp(OnlinePlaylist playlist)
		{
			if (string.IsNullOrWhiteSpace(playlist.Id))
				playlist.Id = playlist.ShareToken;
			await Task.Run(()=>Database.Main.InsertOrReplace(playlist,typeof(TempPlaylist)));
			return await ProcessOnlinePlaylistTracks(playlist.Entries,playlist.Id);
		}
		public static async Task<bool> AddTemp(List<OnlineSong> onlineSongs)
		{
			if (onlineSongs?.Any() != true)
				return true;
			return await ProcessQueue.Enqueue(1, () => Task.Run(async() =>
			{
				var artists = new Dictionary<string, Artist>();
				var artistIDs = new Dictionary<string, ArtistIds>();
				var artistArtworks = new Dictionary<string, ArtistArtwork>();
				var albums = new Dictionary<string, Album>();
				var albumIds = new Dictionary<string, AlbumIds>();
				var albumsArtworks = new Dictionary<string, AlbumArtwork>();
				var songs = new Dictionary<string, TempSong>();
				var genres = new Dictionary<string, Genre>();
				var tracks = new List<FullTrackData>();

				onlineSongs.ForEach(onlineSong =>
				{
					var track = onlineSong.TrackData;
					tracks.Add(track);
					var artist = new Artist(track.Artist, track.ArtistId);
					if (!string.IsNullOrWhiteSpace(track.ArtistServerId))
						artistIDs[track.ArtistServerId] =
							new ArtistIds
							{
								Id = track.ArtistServerId,
								ArtistId = track.ArtistId,
								ServiceType = track.ServiceType,
							};

					track.ArtistArtwork.ForEach(x =>
					{
						x.ArtistId = artist.Id;
						x.ServiceType = track.ServiceType;
						x.SetId();
						artistArtworks[x.Id] = x;
					});
					var album = new Album(track.Album, track.NormalizedAlbum)
					{
						Id = track.AlbumId,
						Year = track.Year,
						IsCompilation = false,
						ArtistId = track.ArtistId,
						AlbumArtist = track.AlbumArtist,
					};
					albums[album.Id] = album;
					track.AlbumArtwork.ForEach(x =>
					{
						x.AlbumId = album.Id;
						x.ServiceType = track.ServiceType;
						x.SetId();
						albumsArtworks[x.Id] = x;
					});

					if (!string.IsNullOrWhiteSpace(track.AlbumServerId))
						albumIds[track.AlbumServerId] = new AlbumIds
						{
							ServiceType = track.ServiceType,
							Id = track.AlbumServerId,
							AlbumId = track.AlbumId,
						};
					
					var song = new TempSong(track.Title, track.NormalizedTitle)
					{
						ParentId = track.ParentId,
						AlbumId = track.AlbumId,
						Id = track.SongId,
						ArtistId = track.ArtistId,
						Artist = track.DisplayArtist,
						Album = track.Album,
						Genre = track.Genre,
						Rating = track.Rating,
						PlayedCount = track.PlayCount,
					};
					songs[song.Id] = song;

					var genre = new Genre(track.Genre);
					if (!string.IsNullOrWhiteSpace(genre.Id))
						genres[genre.Id] = genre;
				});

				Database.Main.InsertOrReplaceAll(artists.Values, typeof(TempArtist));
				Database.Main.InsertOrReplaceAll(artistIDs.Values, typeof(TempArtistIds));
				Database.Main.InsertOrReplaceAll(artistArtworks.Values, typeof(TempArtistArtwork));
				Database.Main.InsertOrReplaceAll(albums.Values, typeof(TempAlbum));
				Database.Main.InsertOrReplaceAll(albumIds.Values, typeof(TempAlbumIds));
				Database.Main.InsertOrReplaceAll(albumsArtworks.Values, typeof(TempAlbumArtwork));
				Database.Main.InsertOrReplaceAll(songs.Values, typeof(TempSong));
				Database.Main.InsertOrReplaceAll(genres.Values, typeof(TempGenre));
				Database.Main.InsertOrReplaceAll(tracks, typeof(TempTrack));
				
				await ProcessTempData();
				return true;
			}));
		}

		class MusicID3Tag
		{

			public byte[] TAGID = new byte[3];      //  3
			public byte[] Title = new byte[30];     //  30
			public byte[] Artist = new byte[30];    //  30 
			public byte[] Album = new byte[30];     //  30 
			public byte[] Year = new byte[4];       //  4 
			public byte[] Comment = new byte[30];   //  30 
			public byte[] Genre = new byte[1];      //  1

		}

		public static async Task<FullTrackData> GetTrackDataFromWebServer(SimpleAuth.Api api, string url)
		{
			

			var time = TimeSpan.FromSeconds(30);
			var resp = await api.SendMessage(url, null, HttpMethod.Get,completionOption: HttpCompletionOption.ResponseHeadersRead,authenticated: false);

			var length = resp?.Content?.Headers?.ContentLength ?? 0;
			if (length == 0)
			{
				resp.Content.Dispose();
				resp.Dispose();
				return null;
			}
			long range = 0;
			var contentRange = resp.Content.Headers.ContentRange;
			if (length == 128 && contentRange.HasRange && contentRange.HasLength)
			{
				range = contentRange.From.Value;
			}
			else {
				range = length - 128;
			}
			resp.Content.Dispose();
			resp.Dispose();
			if (range <= 0)
				return null;
			resp = await api.SendMessage(url, null, HttpMethod.Get, new Dictionary<string, string>
			{
				{"Range",$"bytes={range}-"}
			},authenticated: false);
			using (var fs = await resp.Content.ReadAsStreamAsync())
			{
				MusicID3Tag tag = new MusicID3Tag();
				//fs.Seek(-128, SeekOrigin.End);
				fs.Read(tag.TAGID, 0, tag.TAGID.Length);
				fs.Read(tag.Title, 0, tag.Title.Length);
				fs.Read(tag.Artist, 0, tag.Artist.Length);
				fs.Read(tag.Album, 0, tag.Album.Length);
				fs.Read(tag.Year, 0, tag.Year.Length);
				fs.Read(tag.Comment, 0, tag.Comment.Length);
				fs.Read(tag.Genre, 0, tag.Genre.Length);
				string theTAGID = Encoding.Default.GetString(tag.TAGID);

				if (theTAGID.Equals("TAG"))
				{
					string Title = Encoding.Default.GetString(tag.Title)?.Trim()?.Trim('\u0000');
					string Artist = Encoding.Default.GetString(tag.Artist)?.Trim().Trim('\u0000');
					string Album = Encoding.Default.GetString(tag.Album)?.Trim().Trim('\u0000');
					string year = Encoding.Default.GetString(tag.Year)?.Trim().Trim('\u0000');
					int Year;
					int.TryParse(year, out Year);
					string Comment = Encoding.Default.GetString(tag.Comment);
					string Genre = Encoding.Default.GetString(tag.Genre)?.Trim();

					Console.WriteLine(Title);
					Console.WriteLine(Artist);
					Console.WriteLine(Album);
					Console.WriteLine(Year);
					Console.WriteLine(Comment);
					Console.WriteLine(Genre);
					Console.WriteLine();

					resp.Content.Dispose();
					resp.Dispose();
					return new FullTrackData(Title, Artist, "", Album, Genre)
					{
						Year = Year,
					};
				}
			}
			resp.Content.Dispose();
			resp.Dispose();
			return null;
		}

	}
}