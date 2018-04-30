using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using SimpleDatabase;
using SQLite;

namespace MusicPlayer.Data
{
	internal class Database : SimpleDatabaseConnection
	{
		public static Database Main { get; set; } = setupDb();
		static Database setupDb(bool shouldDeleteOnFail = true)
		{
			try
			{
				return new Database(new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.Create, true));
			}
			catch (Exception ex)
			{
				if (shouldDeleteOnFail)
				{
					LogManager.Shared.Report(ex);
					File.Delete(dbPath);
				}
				else
					throw ex;
			}

			return setupDb(false);
		}
		static string dbPath => Path.Combine(Locations.LibDir, "db.db");
		SQLiteConnection connection;
		public Database(SQLiteConnection connection) : base(connection)
		{
			this.connection = connection;
			connection.ExecuteScalar<string>("PRAGMA journal_mode=WAL");

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

		public void ResetDatabase()
		{
			DropAndCreateTable<TempPlaylistSong>();
			DropAndCreateTable<TempPlaylist>();
			DropAndCreateTable<TempArtistIds>();
			DropAndCreateTable<TempRadioStationSong>();
			DropAndCreateTable<TempArtistArtwork>();
			DropAndCreateTable<TempAlbumIds>();
			DropAndCreateTable<TempAlbumArtwork>();
			DropAndCreateTable<TempPlaylistEntry>();
			DropAndCreateTable<TempGenre>();
			DropAndCreateTable<TempTrack>();
			DropAndCreateTable<TempSong>();
			DropAndCreateTable<TempArtist>();
			DropAndCreateTable<TempAlbum>();
			DropAndCreateTable<PlaybackManager.SongsOrdered>();
			DropAndCreateTable<RadioStationArtwork>();
			DropAndCreateTable<RadioStationSeed>();
			DropAndCreateTable<RadioStationSong>();
			DropAndCreateTable<RadioStation>();
			DropAndCreateTable<PlaylistSong>();
			DropAndCreateTable<Playlist>();
			DropAndCreateTable<Track>();
			DropAndCreateTable<Song>();
			DropAndCreateTable<Genre>();
			DropAndCreateTable<ArtistIds>();
			DropAndCreateTable<ArtistArtwork>();
			DropAndCreateTable<Artist>();
			DropAndCreateTable<AlbumIds>();
			DropAndCreateTable<AlbumArtwork>();
			DropAndCreateTable<Album>();

		}

		public void DropTable<T>()
		{
			var map = connection.GetMapping(typeof(T));
			this.Execute($"drop table if exists {map.TableName}");
		}
		public void DropAndCreateTable<T>()
		{
			DropTable<T>();
			connection.CreateTable<T>();
		}

		public T GetObject<T, T1>(object id) where T1 : T, new() where T : new()
		{
			var obj = GetObject<T>(id);
			return EqualityComparer<T>.Default.Equals(obj, default(T)) ? GetObject<T1>(id) : obj;
		}
	}
}