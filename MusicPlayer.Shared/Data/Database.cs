using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using SimpleDatabase;

namespace MusicPlayer.Data
{
	internal class Database : SimpleDatabaseConnection
	{
		public static Database Main { get; set; } = new Database();
		static string dbPath => Path.Combine(Locations.LibDir, "db.db");

		public Database() : base(dbPath)
		{
			CreateTables(
				typeof (Album),
				typeof (AlbumArtwork),
				typeof (AlbumIds),
				typeof (Artist),
				typeof (ArtistArtwork),
				typeof (ArtistIds),
				typeof (Genre),
				typeof (Song),
				typeof (Track),
				typeof (Playlist),
				typeof (PlaylistSong),
				typeof (RadioStation),
				typeof (RadioStationSong),
				typeof (RadioStationSeed),
				typeof (RadioStationArtwork),
				typeof (ApiModel),
				typeof (PlaybackManager.SongsOrdered),
				typeof (EqualizerPreset),
				typeof (EqualizerPresetValue),
				//Temp stuff
				typeof (TempAlbum),
				typeof (TempArtist),
				typeof (TempSong),
				typeof (TempTrack),
				typeof (TempGenre),
				typeof (TempPlaylistEntry),
				typeof (TempAlbumArtwork),
				typeof (TempAlbumIds),
				typeof (TempArtistArtwork),
				typeof (TempRadioStationSong),
				typeof (TempArtistIds),
				typeof (TempPlaylist),
				typeof (TempPlaylistSong),
				// Offline Stuff
				typeof (SongOfflineClass),
				typeof (ArtistOfflineClass),
				typeof (PlaylistOfflineClass),
				typeof (GenreOfflineClass),
				typeof (AlbumOfflineClass)
				);

			this.MakeClassInstant<Song>();
		}

		public T GetObject<T, T1>(object id) where T1 : T, new() where T : new()
		{
			var obj = GetObject<T>(id);
			return EqualityComparer<T>.Default.Equals(obj, default(T)) ? GetObject<T1>(id) : obj;
		}
	}
}