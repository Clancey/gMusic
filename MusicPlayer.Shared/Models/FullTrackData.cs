using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MusicPlayer.Models
{
	public class FullPlaylistTrackData : FullTrackData
	{
		public FullPlaylistTrackData(string title, string artist, string albumArtist, string album, string genre)
			: base(title, artist, albumArtist, album, genre)
		{
		}

		protected FullPlaylistTrackData()
		{

		}

		public string PlaylistEntryId { get; set; }

		public string TrackId { get; set; }

		public string PlaylistId { get; set; }

		public long SOrder { get; set; }

		public new FullPlaylistTrackData Clone()
		{
			return new FullPlaylistTrackData
			{
				Title = this.Title,
				Artist = this.Artist,
				Album = this.Album,
				AlbumArtist = AlbumArtist,
				AlbumArtwork = AlbumArtwork,
				AlbumServerId = AlbumServerId,
				AlbumId = AlbumId,
				ArtistArtwork = ArtistArtwork,
				ArtistServerId = ArtistServerId,
				Deleted = Deleted,
				NormalizedAlbum = NormalizedAlbum,
				NormalizedAlbumArtist = NormalizedAlbumArtist,
				NormalizedTitle = NormalizedTitle,
				PlayCount = PlayCount,
				LastPlayed = LastPlayed,
				Rating = Rating,
				Id = Id,
				ArtistId = ArtistId,
				ServiceType = ServiceType,
				SongId = SongId,
				MediaType = MediaType,
				Genre = Genre,
				Duration = Duration,
				ServiceId = ServiceId,
				Priority = Priority,
				Year = Year,
				PlaylistEntryId = PlaylistEntryId,
				TrackId = TrackId,
				PlaylistId = PlaylistId,
				SOrder = SOrder,
				Track = Track,
				Disc =  Disc,
				LastUpdated = LastUpdated,
				ParentId = ParentId,
				DisplayArtist = DisplayArtist,
				FileLocation = FileLocation,
			};
		}

		public string RealArtist => string.IsNullOrWhiteSpace(AlbumArtist) ? Artist : AlbumArtist;
	}

	public class FullTrackData : TempTrack
	{
		public static string GetTitleFromFileName(string fileName)
		{
			var fName = Path.GetFileNameWithoutExtension(fileName);
			//First try with spaces for artist like blink-182
			var parts = fName.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 1)
			{
				parts = fName.Split('-');
			}
			if (parts.Length == 1)
				return fName;
			if (parts.Length == 2)
				return parts[1];
			if (parts.Length == 3)
			{
				var mid = parts[1];
				int track;
				if (int.TryParse(mid, out track))
					return parts[2];
			}
			return fName.Replace(parts[0], "");
		}

		public static string GetArtistFromFileName(string fileName)
		{
			var fName = Path.GetFileNameWithoutExtension(fileName);
			//First try with spaces for artist like blink-182
			var parts = fName.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length <= 1)
			{
				parts = fName.Split('-');
				if (parts.Length <= 1)
					return "Unknown Artist";
			}
			return parts[0];
		}

		public FullTrackData(string title, string artist, string albumArtist, string album, string genre)
		{
			var realArtist = string.IsNullOrWhiteSpace(albumArtist) ? artist : albumArtist;
			try{
				if (string.IsNullOrWhiteSpace(realArtist) || realArtist == "Unknown Artist" )
				{
					realArtist = GetArtistFromFileName(title);
					if(realArtist != "Unknown Artist")
						title = GetTitleFromFileName(title);
				}
			}
			catch(Exception ex) {
				Console.WriteLine (title);
				Console.WriteLine (ex);
			}
			this.Title = title;
			this.NormalizedTitle = MediaItemBase.Normalize(title);
			this.Artist = realArtist;
			this.ArtistId = MediaItemBase.Normalize(realArtist);
			this.DisplayArtist = string.IsNullOrWhiteSpace(artist) ? realArtist : artist;
			if (string.IsNullOrWhiteSpace(DisplayArtist))
				Console.WriteLine ("WTF?");
			this.AlbumArtist = albumArtist;
			this.NormalizedAlbumArtist = MediaItemBase.Normalize(string.IsNullOrEmpty(album) && string.IsNullOrEmpty(albumArtist) ? realArtist : albumArtist);

			this.Album = string.IsNullOrWhiteSpace(album) ? "Unknown Album" : album;
			this.NormalizedAlbum = MediaItemBase.Normalize(Album);

			this.AlbumId = $"{NormalizedAlbumArtist} - {NormalizedAlbum}";

			this.Genre = genre;

			this.SongId = $"{ArtistId} - {AlbumId} - {NormalizedTitle}";
		}

		protected FullTrackData()
		{

		}

		public string Title { get; set; }

		public string NormalizedTitle { get; protected set; }

		public string Artist { get; set; }

		public string DisplayArtist { get; protected set; }

		public string ArtistServerId { get; set; }

		public string AlbumArtist { get; set; }

		public string NormalizedAlbumArtist { get; protected set; }

		public string Album { get; set; }

		public string NormalizedAlbum { get; protected set; }

		public string AlbumServerId { get; set; }

		public List<ArtistArtwork> ArtistArtwork { get; set; } = new List<ArtistArtwork>();

		public List<AlbumArtwork> AlbumArtwork { get; set; } = new List<AlbumArtwork>();

		public int Rating { get; set; }

		public int Year { get; set; }

		public int Disc { get; set; }

		public int Track { get; set; }

		public FullTrackData Clone()
		{
			return new FullTrackData
			{
				Title = this.Title,
				Artist = this.Artist,
				DisplayArtist = this.DisplayArtist,
				Album = this.Album,
				AlbumArtist = AlbumArtist,
				AlbumArtwork = AlbumArtwork,
				AlbumServerId = AlbumServerId,
				AlbumId = AlbumId,
				ArtistArtwork = ArtistArtwork,
				ArtistServerId = ArtistServerId,
				Deleted = Deleted,
				NormalizedAlbum = NormalizedAlbum,
				NormalizedAlbumArtist = NormalizedAlbumArtist,
				NormalizedTitle = NormalizedTitle,
				PlayCount = PlayCount,
				LastPlayed = LastPlayed,
				Rating = Rating,
				Id = Id,
				ArtistId = ArtistId,
				ServiceType = ServiceType,
				SongId = SongId,
				MediaType = MediaType,
				Genre = Genre,
				Duration = Duration,
				ServiceId = ServiceId,
				Priority = Priority,
				Year = Year,
				ParentId = ParentId,
				LastUpdated = LastUpdated,
				FileExtension = FileExtension,
				Track = Track,
				Disc = Disc
			};
		}
	}
}