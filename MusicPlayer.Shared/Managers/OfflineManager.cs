using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Api;
using MusicPlayer.Data;
using MusicPlayer.Models;
using SimpleAuth;

namespace MusicPlayer.Managers
{
	internal class OfflineManager : ManagerBase<OfflineManager>
	{
		public Task DownloadMissingStuff ()
		{
			return Task.Run (async () => {
				await Task.WhenAll (Directory.EnumerateFiles (Locations.TmpDownloadDir).Select (ProcessTempFile));

				await GetMissingSongs ();

				await GetMissingAlbums ();

				await GetMissingArtists ();

				await GetMissingGenres ();

				await GetMissingPlaylistSongs ();
			});
		}

		async Task GetMissingSongs ()
		{
			var songs = await Database.Main.QueryAsync<Song> ("select s.* from song s inner join SongOfflineClass so on s.Id = so.Id and s.OfflineCount = 0 and so.ShouldBeLocal = 1 and ExcludedOffline is not 1");

			songs?.ForEach (async (x) => await StartDownload (x));
		}


		async Task GetMissingAlbums ()
		{
			var songs = await Database.Main.QueryAsync<Song> ("select s.* from song s inner join AlbumOfflineClass so on s.AlbumId = so.Id and s.OfflineCount = 0 and so.ShouldBeLocal = 1 and ExcludedOffline is not 1");

			songs?.ForEach (async (x) => await StartDownload (x));
		}

		async Task GetMissingArtists ()
		{
			var songs = await Database.Main.QueryAsync<Song> ("select s.* from song s inner join ArtistOfflineClass so on s.ArtistId = so.Id and s.OfflineCount = 0 and so.ShouldBeLocal = 1 and ExcludedOffline is not 1");
			
			songs?.ForEach (async (x) => await StartDownload (x));
		}


		async Task GetMissingGenres ()
		{
			var songs = await Database.Main.QueryAsync<Song> ("select s.* from song s inner join GenreOfflineClass so on s.Genre = so.Id and s.OfflineCount = 0 and so.ShouldBeLocal = 1 and ExcludedOffline is not 1");

			songs?.ForEach (async (x) => await StartDownload (x));
		}

		async Task GetMissingPlaylistSongs ()
		{
			var songs = await Database.Main.QueryAsync<Song> ("select s.* from song s inner join PlaylistSong ps on s.Id = ps.SongId inner join PlaylistOfflineClass pso on ps.PlaylistId = pso.Id where s.OfflineCount = 0 and pso.ShouldBeLocal = 1 and s.ExcludedOffline is not 1");

			songs?.ForEach (async (x) => await StartDownload (x));
		}

		async Task StartDownload (Song song)
		{
			if(song == null)
				return;
			var track = await MusicManager.Shared.GetTrack (song.Id);
			if (track == null) {
				LogManager.Shared.Report (new Exception ("Track is null...."));
				return;
			}
			if (track.ServiceType == ServiceType.iPod || track.ServiceType == ServiceType.FileSystem) {
				await MusicProvider.SetOffline (track);
				return;
			}
			await BackgroundDownloadManager.Shared.Download (track);
		}

		public async void ToggleOffline (MediaItemBase item)
		{
			LogManager.Shared.Log ("Toggle Offline", item);
			var shouldBeLocal = !(item.ShouldBeLocal () || item.OfflineCount > 0);

			if (shouldBeLocal && (item is TempSong || item is TempAlbum || item is OnlineSong || item is OnlineAlbum)) {
				await MusicManager.Shared.AddToLibrary (item);
			}


			var song = item as Song;
			if (song != null) {
				await ToggleOffline (song, shouldBeLocal);
				return;
			}

			var album = item as Album;
			if (album != null) {
				await ToggleOffline (album, shouldBeLocal);
				return;
			}

			var artist = item as Artist;
			if (artist != null) {
				await ToggleOffline (artist, shouldBeLocal);
				return;
			}

			var playlist = item as Playlist;
			if (playlist != null) {
				await ToggleOffline (playlist, shouldBeLocal);
			}
		}


		public async Task ToggleOffline (Playlist playlist, bool shouldBeLocal)
		{
			Database.Main.InsertOrReplace (new PlaylistOfflineClass { Id = playlist.Id, ShouldBeLocal = shouldBeLocal });
			var songs = await Database.Main.QueryAsync <Song> ("select s.* from song s inner join PlaylistSong ps on s.Id = ps.SongId where ps.PlaylistId = ?", playlist.Id);

			await ToggleOffline (songs, shouldBeLocal);
		}

		public async Task ToggleOffline (Genre genre, bool shouldBeLocal)
		{
			Database.Main.InsertOrReplace (new GenreOfflineClass () { Id = genre.Id, ShouldBeLocal = shouldBeLocal });
			var songs = await Database.Main.TablesAsync<Song> ().Where (x => x.Genre == genre.Id).ToListAsync ();

			await ToggleOffline (songs, shouldBeLocal);
		}

		public async Task ToggleOffline (Artist artist, bool shouldBeLocal)
		{
			Database.Main.InsertOrReplace (new ArtistOfflineClass { Id = artist.Id, ShouldBeLocal = shouldBeLocal });
			var songs = await Database.Main.TablesAsync<Song> ().Where (x => x.ArtistId == artist.Id).ToListAsync ();

			await ToggleOffline (songs, shouldBeLocal);
		}

		public async Task ToggleOffline (Album album, bool shouldBeLocal)
		{
			Database.Main.InsertOrReplace (new AlbumOfflineClass { Id = album.Id, ShouldBeLocal = shouldBeLocal });
			var songs = await Database.Main.TablesAsync<Song> ().Where (x => x.AlbumId == album.Id).ToListAsync ();
			await ToggleOffline (songs, shouldBeLocal);
		}

		async Task ToggleOffline (IEnumerable<Song> songs, bool shouldBeLocal)
		{
			if (shouldBeLocal) {
				foreach (var song in songs.Where(x => x.OfflineCount == 0)) {
					await StartDownload (song);
				}
			} else {
				await MusicManager.Shared.DeleteOfflineTracks (songs.Where (x => !x.ShouldBeLocal ()).ToList ());
			}
		}

		public async Task ToggleOffline (Song song, bool shouldBeLocal)
		{
			if (song.ServiceTypes.Contains (ServiceType.iPod))
				return;
			LogManager.Shared.Log ("Toggle Offline", song);
			Database.Main.InsertOrReplace (new SongOfflineClass { Id = song.Id, ShouldBeLocal = shouldBeLocal });

			
			if (shouldBeLocal) {
				await StartDownload (song);
			} else {
				await MusicManager.Shared.DeleteOfflineTrack (song);
			}
		}

		async Task ProcessTempFile (string fileName)
		{
			var dest = Path.Combine (Locations.MusicDir, Path.GetFileName (fileName));
			if (File.Exists (dest)) {
				File.Delete (dest);
				return;
			}
			File.Move (fileName, dest);
			var id = Path.GetFileNameWithoutExtension (fileName);

			var track = Database.Main.GetObject<Track, TempTrack> (id);
			try {
				if (track != null) {
					var song = Database.Main.GetObject<Song,TempSong> (track.SongId);
					using (var file = TagLib.File.Create (dest)) {
						file.Tag.Title = song.Name;
						file.Tag.Album = song.Album;
						file.Tag.AlbumArtists = new string[]{ song.Artist };
						file.Tag.Disc = (uint)song.Disc;
						file.Tag.Track = (uint)song.Track;
						file.Tag.TrackCount = (uint)song.TrackCount;
						file.Tag.Year = (uint)song.Year;
						file.Tag.Genres = new string[]{ song.Genre };
						file.Save ();
					}
				}
			} catch (Exception ex) {
				LogManager.Shared.Report (ex);
			}

			await TrackDownloaded (id);
		}

		public async Task TrackDownloaded (string trackId)
		{
			var track = Database.Main.GetObject<Track, TempTrack> (trackId);
			if (track == null)
				return;

			var filePath = Path.Combine (Locations.MusicDir, track.FileName);
			var song = Database.Main.GetObject<Song,TempSong> (track.SongId);
			try {
				using (var file = TagLib.File.Create (filePath)) {
					file.Tag.Title = song.Name;
					file.Tag.Album = song.Album;
					file.Tag.Performers = file.Tag.AlbumArtists = new string[]{ song.Artist };
					file.Tag.Disc = (uint)song.Disc;
					file.Tag.Track = (uint)song.Track;
					file.Tag.TrackCount = (uint)song.TrackCount;
					file.Tag.Year = (uint)song.Year;
					file.Tag.Genres = new string[]{ song.Genre };
					file.Save ();
				}
			} catch (Exception ex) {

				LogManager.Shared.Report (ex);
			}

			var newTrack = new Track {
				Id = track.FileName,
				Duration = track.Duration,
				Genre = track.Genre,
				SongId = track.SongId,
				ServiceType = ServiceType.FileSystem,
				AlbumId = track.AlbumId,
				ArtistId = track.ArtistId,
				Deleted = false,
				ServiceId = track.ServiceId,
				MediaType = track.MediaType,
				FileExtension = track.FileExtension,
			};
			Database.Main.InsertOrReplace (newTrack);
			await MusicProvider.SetOffline (newTrack);
		}
	}
}