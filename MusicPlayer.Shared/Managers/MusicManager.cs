using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Api;
using MusicPlayer.Api.GoogleMusic;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;
using MusicPlayer.Models.Scrobbling;
using Newtonsoft.Json.Converters;
using Xamarin;

namespace MusicPlayer.Managers
{
	internal class MusicManager : ManagerBase<MusicManager>
	{
		public async Task<SongPlaybackData> GetPlaybackData(Song song, bool playVideo = false)
		{
			var tracks = (await GetTracks(song.Id)).SortByPriority();
			var track = playVideo ? tracks.FirstOrDefault(x=> x.MediaType == MediaType.Video) : tracks.FirstOrDefault();
			if (track == null)
				return null;
			Uri url = null;
			if(track.ServiceType == ServiceType.iPod)
			{
				url = await GetTrackUrl(track);
				if(url == null && tracks.Count > 1)
					track = tracks[1];
				
			}

			if (track.ServiceType == ServiceType.FileSystem)
			{
				var path = Path.Combine(Locations.MusicDir, track.FileName);
				if (!string.IsNullOrWhiteSpace (track.FileLocation) && File.Exists (track.FileLocation))
					url = new Uri (track.FileLocation);
				else if(File.Exists(path))
					url = new Uri(path);
				else if(tracks.Count > 1)
				{
					track = tracks[1];
				}
				
			}
			if (url == null)
			{
				var temp = TempFileManager.Shared.GetTempFile(track.Id);
				url = temp.Item1 ? new Uri(temp.Item2) : await GetTrackUrl(track);

			}
			return new SongPlaybackData
			{
				Tracks = tracks,
				CurrentTrack = track,
				CurrentTrackIndex = 0,
				Uri = url,
			};
		}

		public async Task<Uri> GeTrackPlaybackUri(string trackId)
		{
			var song = Database.Main.GetObject<Track, TempTrack>(trackId);
			return await GetTrackUrl(song);
		}

		public async Task<Uri> GetPlaybackUri(Song song)
		{
			var tracks = await GetTracks(song.Id);
			if (tracks.Count == 1)
				return await GetTrackUrl(tracks[0]);
			foreach (var track in tracks)
			{
				var url = await GetTrackUrl(track);
				if (url != null)
					return url;
			}
			return null;
		}

		async Task<Uri> GetTrackUrl(Track track)
		{
			try
			{
				var temp = TempFileManager.Shared.GetTempFile(track.Id);
				if (temp.Item1 && System.IO.File.Exists(temp.Item2))
				{
					return new Uri(temp.Item2);
				}
				var path = Path.Combine(Locations.MusicDir, track.FileName);
				if (File.Exists(path))
				{
					return new Uri(path);
				}
				var provider = ApiManager.Shared.GetMusicProvider(track.ServiceId);
				return await provider.GetPlaybackUri(track);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				return null;
			}
		}


		public async Task<DownloadUrlData> GetDownloadUrl(Track track)
		{
			try
			{
				var provider = ApiManager.Shared.GetMusicProvider(track.ServiceId);
				return await provider.GetDownloadUri(track);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				return null;
			}
		}

		public async Task<List<Track>> GetTracks(string songId)
		{
			var tracks = await Database.Main.TablesAsync<Track>().Where(x => x.SongId == songId).ToListAsync() ??
						new List<Track>();
			var tempTracks = await Database.Main.TablesAsync<TempTrack>().Where(x => x.SongId == songId).ToListAsync();
			if (tempTracks?.Count > 0)
				tracks.AddRange(tempTracks);
			if (tracks.Count == 1)
				return tracks;
			var sorted = tracks.SortByPriority();
			return sorted;
		}
		public async Task<List<Track>> GetTracks(string songId, string serviceId)
		{
			var tracks = await Database.Main.TablesAsync<Track>().Where(x => x.SongId == songId && x.ServiceId == serviceId).ToListAsync() ??
			                           new List<Track>();
			var tempTracks = await Database.Main.TablesAsync<TempTrack>().Where(x => x.SongId == songId).ToListAsync();
			if (tempTracks?.Count > 0)
				tracks.AddRange(tempTracks);
			return tracks;
		}

		public async Task<Track> GetTrack(string songId, int skip = 0)
		{
			var tracks = await GetTracks(songId);
			return skip >= tracks.Count ? null : tracks[skip];
		}

		public string GetSongId(string trackId)
		{
			var track = Database.Main.GetObject<Track, TempTrack>(trackId);
			return track.SongId;
		}

		public async Task<bool> LoadRadioStationTracks(RadioStation station)
		{
			return await LoadRadioStation(station, false);
		}

		public async Task<bool> LoadMoreRadioStationTracks(RadioStation station)
		{
			return await LoadRadioStation(station, true);
		}

		async Task<bool> LoadRadioStation(RadioStation station, bool isContinuation)
		{
			try
			{
				var provider = ApiManager.Shared.GetMusicProvider(station.ServiceId);
				if (!isContinuation)
					await Database.Main.ExecuteAsync("delete from RadioStationSong where StationId = ?", station.Id);
				var success = await provider.LoadRadioStation(station, isContinuation);
				return success;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public async Task<bool> Delete(MediaItemBase item)
		{
			var radio = item as RadioStation;
			if (radio != null)
				return await Delete(radio);

			var playlist = item as Playlist;
			if (playlist != null)
				return await Delete(playlist);

			return false;
		}

		public async Task<bool> Delete(Playlist item)
		{
			try
			{
				var provider = ApiManager.Shared.GetMusicProvider(item.ServiceId);
				return await provider.DeletePlaylist(item);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public async Task<bool> Delete(RadioStation item)
		{
			try
			{
				var provider = ApiManager.Shared.GetMusicProvider(item.ServiceId);
				var success = await provider.DeleteRadioStation(item);
				if (success)
					provider.SyncDatabase();
				return success;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public async Task<bool> Delete(PlaylistSong song)
		{
			var playlist = Database.Main.GetObject<Playlist>(song.PlaylistId);
			var provider = ApiManager.Shared.GetMusicProvider(playlist.ServiceId);
			var success = await provider.DeletePlaylistSong(song);
			if (success)
				provider.SyncDatabase();
			return success;
		}

		public async Task<bool> RecordPlayback(PlaybackEndedEvent data)
		{
			try{
				var track = Database.Main.GetObject<Track, TempTrack>(data.TrackId);
				var provider = ApiManager.Shared.GetMusicProvider(track.ServiceId);
				var success = await provider.RecordPlayack(data);
				return success;
			}
			catch(Exception ex){
				LogManager.Shared.Report(ex);
				return false;
			}
		}

		public async Task<bool> MoveSong(PlaylistSong song, string previousId, string nextId, int index)
		{
			var playlist = Database.Main.GetObject<Playlist>(song.PlaylistId);
			var provider = ApiManager.Shared.GetMusicProvider(playlist.ServiceId);
			var success = await provider.MoveSong(song, previousId, nextId, index);
			if (success)
				provider.SyncDatabase();
			return success;
		}

		public string[] GetServiceTypes(MediaItemBase item)
		{
			var onlineSong = item as OnlineSong;
			if (onlineSong != null)
			{
				return new[] { onlineSong.TrackData.ServiceId };
			}

			var onlinePlaylist = item as OnlinePlaylist;
			if (onlinePlaylist != null)
			{
				return new[] { onlinePlaylist.ServiceId };
			}

			string column = "";
			if (item is Song)
				column = "SongId";
			else if (item is Album)
				column = "AlbumId";
			else if (item is Artist)
				column = "ArtistId";

			if (!string.IsNullOrWhiteSpace(column))
			{
				var items =
					Database.Main.Query<ServiceIdHolder>($"select distinct ServiceId from Track where {column} = ? union select distinct ServiceId from TempTrack where {column} = ?", item.Id, item.Id)
						.ToArray();
				return items?.Select(x => x.ServiceId).ToArray() ?? new string[0];
			}
			var playlist = item as Playlist;
			if (playlist != null)
				return new[] {playlist.ServiceId};

			var radio = item as RadioStation;
			if (radio != null)
				return new[] {radio.ServiceId};

			return new string[0];
		}

		class ServiceIdHolder
		{
			public string ServiceId { get; set; }
		}

		public async Task<bool> AddToPlaylist(MediaItemBase item, Playlist playlist)
		{
			try
			{
				List<Track> songs = new List<Track>();
				var provider = string.IsNullOrWhiteSpace(playlist.ServiceId) ? ApiManager.Shared.GetMusicProvider(playlist.ServiceType): ApiManager.Shared.GetMusicProvider(playlist.ServiceId);

				if (string.IsNullOrWhiteSpace(playlist.ServiceId))
				{
					playlist.ServiceId = provider.Id;
				}
				var onlineSong = item as OnlineSong;
				if (onlineSong != null)
					songs.Add(onlineSong.TrackData);
				else {
					var song = item as Song;
					if (song != null)
					{
						var tracks = await GetTracks(song.Id);
						var track = tracks.FirstOrDefault(x => x.ServiceId == playlist.ServiceId);
						if (track != null)
							songs.Add(track);
					}
				}
				//TODO: albums
				var album = item as Album;
				if (album != null)
				{
					var tracks =
						await
							Database.Main.TablesAsync<Track>()
								.Where(x => x.AlbumId == album.Id && x.ServiceId == playlist.ServiceId)
								.ToListAsync();
					songs.AddRange(tracks);
				}
				var artist = item as Artist;
				if (artist != null)
				{
					var tracks =
						   await
							   Database.Main.TablesAsync<Track>()
								   .Where(x => x.ArtistId == artist.Id && x.ServiceId == playlist.ServiceId)
								   .ToListAsync();
					songs.AddRange(tracks);

				}
				return await provider.AddToPlaylist(songs, playlist);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public async Task<RadioStation> CreateRadioStation(MediaItemBase item)
		{
			try
			{
				var song = item as Song;
				if (song != null)
					return await CreateRadioStation(song);
				var album = item as Album;
				if (album != null)
					return await CreateRadioStation(album);
				var artist = item as Artist;
				if (artist != null)
					return await CreateRadioStation(artist);
				var station = item as OnlineRadioStation;
				if (station != null)
				{
					var provider = ApiManager.Shared.GetMusicProvider<GoogleMusicProvider>(ServiceType.Google);
					return await provider.CreateRadioStation(station.Name, station.StationSeeds.FirstOrDefault());
				}
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return null;
		}

		public Task<RadioStation> CreateRadioStation(Song song)
		{
			return CreateRadioStation(song, song.Name);
		}

		protected async Task<RadioStation> CreateRadioStation(Song song, string name)
		{
			var provider = ApiManager.Shared.GetMusicProvider<GoogleMusicProvider>(ServiceType.Google);

			var track =
				await
					Database.Main.TablesAsync<Track>()
						.Where(x => x.SongId == song.Id && x.ServiceType == ServiceType.Google)
						.FirstOrDefaultAsync();
			if (track != null)
				return await provider.CreateRadioStation(name, track);

			track =
				await
					Database.Main.TablesAsync<TempTrack>()
						.Where(x => x.SongId == song.Id && x.ServiceType == ServiceType.Google)
						.FirstOrDefaultAsync();
			if (track != null)
				return await provider.CreateRadioStation(name, track);

			var albumId =
				await
					Database.Main.TablesAsync<AlbumIds>()
						.Where(x => x.AlbumId == song.AlbumId && x.ServiceType == ServiceType.Google)
						.FirstOrDefaultAsync();
			if (albumId != null)
				return await provider.CreateRadioStation(name, albumId);

			albumId =
				await
					Database.Main.TablesAsync<TempAlbumIds>()
						.Where(x => x.AlbumId == song.AlbumId && x.ServiceType == ServiceType.Google)
						.FirstOrDefaultAsync();
			if (albumId != null)
				return await provider.CreateRadioStation(name, albumId);

			var artist =
				await
					Database.Main.TablesAsync<ArtistIds>()
						.Where(x => x.ArtistId == song.ArtistId && x.ServiceType == ServiceType.Google)
						.FirstOrDefaultAsync();
			if (artist != null)
				return await provider.CreateRadioStation(name, artist);

			artist =
				await
					Database.Main.TablesAsync<TempArtistIds>()
						.Where(x => x.ArtistId == song.ArtistId && x.ServiceType == ServiceType.Google)
						.FirstOrDefaultAsync();
			if (artist != null)
				return await provider.CreateRadioStation(name, artist);

			return null;
		}

		public async Task<RadioStation> CreateRadioStation(Artist artist)
		{
			var name = artist.Name;
			var provider = ApiManager.Shared.GetMusicProvider<GoogleMusicProvider>(ServiceType.Google);

			var artistId =
				await
					Database.Main.TablesAsync<ArtistIds>()
						.Where(x => x.ArtistId == artist.Id && x.ServiceType == ServiceType.Google)
						.FirstOrDefaultAsync();
			if (artistId != null)
				return await provider.CreateRadioStation(name, artistId);

			artistId =
				await
					Database.Main.TablesAsync<TempArtistIds>()
						.Where(x => x.ArtistId == artist.Id && x.ServiceType == ServiceType.Google)
						.FirstOrDefaultAsync();
			if (artistId != null)
				return await provider.CreateRadioStation(name, artistId);

			var songs = await Database.Main.TablesAsync<Song>().Where(x => x.ArtistId == artist.Id).ToListAsync();
			foreach (var song in songs)
			{
				var station = await CreateRadioStation(song, name);
				if (station != null)
					return station;
			}

			var tempSongs = await Database.Main.TablesAsync<TempSong>().Where(x => x.ArtistId == artist.Id).ToListAsync();
			foreach (var song in tempSongs)
			{
				var station = await CreateRadioStation(song, name);
				if (station != null)
					return station;
			}

			return null;
		}

		public async Task<RadioStation> CreateRadioStation(Album album)
		{
			var name = $"{album.Name} - {album.DetailText}";
			var provider = ApiManager.Shared.GetMusicProvider<GoogleMusicProvider>(ServiceType.Google);
			var albumId =
				await
					Database.Main.TablesAsync<AlbumIds>()
						.Where(x => x.AlbumId == album.Id && x.ServiceType == ServiceType.Google)
						.FirstOrDefaultAsync();
			if (albumId != null)
				return await provider.CreateRadioStation(name, albumId);

			albumId =
				await
					Database.Main.TablesAsync<TempAlbumIds>()
						.Where(x => x.AlbumId == album.Id && x.ServiceType == ServiceType.Google)
						.FirstOrDefaultAsync();
			if (albumId != null)
				return await provider.CreateRadioStation(name, albumId);
			var songs = await Database.Main.TablesAsync<Song>().Where(x => x.AlbumId == album.Id).ToListAsync();
			foreach (var song in songs)
			{
				var station = await CreateRadioStation(song, name);
				if (station != null)
					return station;
			}

			var tempSongs = await Database.Main.TablesAsync<TempSong>().Where(x => x.AlbumId == album.Id).ToListAsync();
			foreach (var song in tempSongs)
			{
				var station = await CreateRadioStation(song, name);
				if (station != null)
					return station;
			}

			return null;
		}

		public async Task<List<Song>> GetSongs(Artist artist)
		{
			return await
				Database.Main.TablesAsync<Song>()
					.Where(x => x.ArtistId == artist.Id)
					.OrderBy(x => x.Year)
					.OrderBy(x => x.Album)
					.OrderBy(x => x.Disc)
					.OrderBy(x => x.Track)
					.ToListAsync();
		}

		public async Task<List<Song>> GetSongs(Album album)
		{
			return
				await
					Database.Main.TablesAsync<Song>()
						.Where(x => x.AlbumId == album.Id)
						.OrderBy(x => x.Disc)
						.OrderBy(x => x.Track)
						.ToListAsync();
		}

		public async Task<List<Song>> GetSongs(Genre gener)
		{
			return
				await
					Database.Main.TablesAsync<Song>()
						.Where(x => x.Genre == gener.Id)
						.OrderBy(x => x.ArtistId)
						.OrderBy(x => x.NameNorm)
						.ToListAsync();
		}

		public Song GetCurrentSong()
		{
			return string.IsNullOrWhiteSpace(Settings.CurrentSong)
				? null
				: Database.Main.GetObject<Song, TempSong>(Settings.CurrentSong);
		}

		public async Task DeleteOfflineTracks(List<Song> songs)
		{
			foreach (var song in songs)
			{

				var track =
					await
						Database.Main.TablesAsync<Track>()
							.Where(x => x.SongId == song.Id && x.ServiceType == ServiceType.FileSystem).FirstOrDefaultAsync();
				if(track == null)
					continue;
				var file = Path.Combine(Locations.MusicDir, track.FileName);
				if(File.Exists(file))
					File.Delete(file);
				Database.Main.Delete(track);
			}
			await MusicProvider.SetOfflineEverything();
		}

		public async Task DeleteOfflineTrack(Song song)
		{
			song.ExcludedOffline = true;
			Database.Main.Update(song);
			var track =
				await
					Database.Main.TablesAsync<Track>()
						.Where(x => x.SongId == song.Id && x.ServiceType == ServiceType.FileSystem).FirstOrDefaultAsync();
			if (track == null)
				return;
			var file = Path.Combine(Locations.MusicDir, track.FileName);
			if (File.Exists(file))
				File.Delete(file);
			Database.Main.Delete(track);

			await MusicProvider.SetOffline(track);
		}

		public async Task<List<Song>> GetOnlineTracks(Album album)
		{
			var onlineAlbum = album as OnlineAlbum;
			var albumIds = onlineAlbum != null ? new List<AlbumIds> {new AlbumIds {Id = onlineAlbum.AlbumId,AlbumId = onlineAlbum.Id, ServiceType = onlineAlbum.ServiceType}} : await Database.Main.TablesAsync<AlbumIds>().Where(x => x.AlbumId == album.Id).ToListAsync() ?? new List<AlbumIds>();
			if(albumIds.Count == 0)
				albumIds.AddRange(await Database.Main.TablesAsync<TempAlbumIds>().Where(x => x.AlbumId == album.Id).ToListAsync() ?? new List<TempAlbumIds>());
			if (albumIds.Count == 0)
				return new List<Song>();

			foreach (var albumId in albumIds)
			{
				var provider = ApiManager.Shared.GetMusicProvider(albumId.ServiceType);
				return await provider.GetAlbumDetails(albumId.Id);
			}
			return new List<Song>();
		}

		public async Task<List<OnlinePlaylistEntry>> GetOnlineTracks(OnlinePlaylist playlist)
		{
			var provider = ApiManager.Shared.GetMusicProvider(playlist.ServiceId);
			return await provider.GetPlaylistEntries(playlist);
		}
		public async Task<bool> AddTemp(OnlinePlaylist playlist)
		{
			if (playlist.Entries == null || playlist.Entries.Count == 0)
				playlist.Entries = await GetOnlineTracks(playlist);

			return await  MusicProvider.AddTemp(playlist);
		}
		public Task<bool> AddTemp(OnlineSong onlineSong)
		{
			return MusicProvider.AddTemp(onlineSong);
		}

		public Task<bool> AddTemp(OnlineRadioStation station)
		{
			return MusicProvider.AddTemp(station);
		}

		public async Task<bool> ThumbsUp(Song song)
		{
			if (song == null)
				return true;
			return await SetRating(song, 5);
		}

		public async Task<bool> ThumbsDown(Song song)
		{
			if (song == null)
				return true;
			if (song.Id == Settings.CurrentSong)
				PlaybackManager.Shared.NextTrack();
			return await SetRating(song, 1);
		}

		public async Task<bool> Unrate(Song song)
		{
			if (song == null)
				return true;
			return await SetRating(song, 0);
		}

		public async Task<bool> SetRating(Song song, int rating)
		{
			try
			{
				song.Rating = rating;
				var tracks = await GetTracks(song.Id);
				var tasks = tracks.Select(t =>
				{
					var api = ApiManager.Shared.GetMusicProvider(t.ServiceId);
					return api.SetRating(t, rating);
				}).ToList();

				var s = await WaitForSuccess(tasks);
				if (s)
					Database.Main.Update(song);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}

			return false;
		}

		public async Task<bool> WaitForSuccess(List<Task<bool>> tasks)
		{
			while (true)
			{
				var task = await Task.WhenAny(tasks);
				if (task.Result)
					return true;
				tasks.Remove(task);
				if (tasks.Count >= 1)
					continue;
				return false;
			}
		}

		public async Task<bool> AddToLibrary(MediaItemBase item)
		{
			var onlineSong = item as OnlineSong;
			if (onlineSong != null)
				return await AddToLibrary(onlineSong);
			var tempSong = item as TempSong;
			if (tempSong != null)
				return await AddToLibrary(tempSong);

			var album = item as OnlineAlbum;
			if (album != null)
				return await AddToLibrary(album);

			var plist = item as OnlinePlaylist;
			if (plist != null)
				return await AddToLibrary(plist);

			var onlineState = item as OnlineRadioStation;
			if (onlineState != null)
				return await AddToLibrary(onlineState);
			
			var radio = item as RadioStation;
			if (radio != null)
				return await AddToLibrary(radio);
			App.ShowNotImplmented();
			return false;
		}

		public async Task<bool> AddToLibrary(OnlineAlbum album)
		{
			var api = ApiManager.Shared.GetMusicProvider(album.ServiceType);
			return await api.AddToLibrary(album);
		}

		public async Task<bool> AddToLibrary(OnlineSong  song)
		{
			var api = ApiManager.Shared.GetMusicProvider(song.TrackData.ServiceId);
			return await api.AddToLibrary(song);
		}

		public async Task<bool> AddToLibrary(OnlineArtist artist)
		{

			App.ShowNotImplmented();
			return false;
		}


		public async Task<bool> AddToLibrary(OnlinePlaylist playlist)
		{
			var api = ApiManager.Shared.GetMusicProvider(playlist.ServiceId);
			return await api.AddToLibrary(playlist);
		}

		public async Task<bool> AddToLibrary(OnlineRadioStation station)
		{
			var api = ApiManager.Shared.GetMusicProvider(station.ServiceId);
			return await api.AddToLibrary(station);
		}
		public async Task<bool> AddToLibrary(RadioStation station)
		{
			var api = ApiManager.Shared.GetMusicProvider(station.ServiceId);
			return await api.AddToLibrary(station);

		}

		public async Task<bool> AddToLibrary(TempSong  song)
		{
			var track = (await GetTracks(song.Id)).FirstOrDefault(x=> x.ServiceType == ServiceType.Google);
			if(track == null)
				return false;
			var api = ApiManager.Shared.GetMusicProvider(track.ServiceId);
			return await api.AddToLibrary(track);
		}


		public async Task<SearchResults> GetArtistDetails(Artist artist)
		{
			var provider = ApiManager.Shared.GetMusicProvider(ServiceType.Google);
			var onlineArtist = artist as OnlineArtist;
			if (onlineArtist != null)
			{
				return await provider.GetArtistDetails(onlineArtist.OnlineId);
			}
			var id = await Database.Main.TablesAsync<ArtistIds>().Where(x => x.ArtistId == artist.Id).FirstOrDefaultAsync();
			if (id == null)
				return null;
			return await provider.GetArtistDetails(id.Id);
		}
		public async Task<List<PlaylistSong>> GetTracks(OnlinePlaylist playlist)
		{
			return null;
		} 
	}
}