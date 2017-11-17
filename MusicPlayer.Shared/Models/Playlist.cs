using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Api;
using MusicPlayer.Data;
using SQLite;
using System.Threading.Tasks;
using Localizations;

namespace MusicPlayer.Models
{

	public class OnlinePlaylist : Playlist
	{
		public List<OnlinePlaylistEntry> Entries { get; set; }
		public override string DetailText => "";
	}

	public class TempPlaylist : Playlist
	{
		
	}
	public class Playlist : MediaItemBase
	{
		public Playlist()
		{
		}

		public Playlist(string name) : base(name)
		{
		}

		public bool IsAutoPlaylist { get; set; }

		[Indexed]
		public string ServiceId { get; set; }

		public ServiceType ServiceType { get; set; }

		public string Description { get; set; }

		[Indexed]
		public bool Deleted { get; set; }

		public string Owner { get; set; }
		public string OwnerImage { get; set; }
		public bool AccessControlled { get; set; }
		public string ShareToken { get; set; }

		[Indexed]
		public string PlaylistType { get; set; }
		public override string ToString()
		{
			return this.Name;
		}

		public virtual int SongCount { get; set; }

		public long LastSync { get; set; }

		string SongString => SongCount == 1 ? Strings.Song : Strings.Songs;
		public override string DetailText => $"{SongCount} {SongString}";

		public override bool ShouldBeLocal()
		{
			return Database.Main.GetObject<PlaylistOfflineClass>(Id)?.ShouldBeLocal == true;
		}

		AlbumArtwork[] allArtwork;

		[Ignore]
		public AlbumArtwork[] AllArtwork
		{
			set { allArtwork = value; }
		}

		public virtual async Task<AlbumArtwork[]> GetAllArtwork()
		{
			if (allArtwork != null)
				return allArtwork;

			var art = await Database.Main.QueryAsync<AlbumArtwork>("select distinct ar.* from AlbumArtwork ar inner join Song s on s.AlbumId = ar.AlbumId inner join PlaylistSong pl on pl.SongID = s.Id where pl.PlaylistId = ? limit 4", Id) ??
				new List<AlbumArtwork>();
			var tempArtwork =
				await Database.Main.QueryAsync<TempAlbumArtwork>(
					"select distinct ar.* from TempAlbumArtwork ar inner join TempSong s on s.AlbumId = ar.AlbumId inner join PlaylistSong pl on pl.SongID = s.Id where pl.PlaylistId = ? limit 4",
					Id);

			if (tempArtwork != null)
				art.AddRange(tempArtwork);
			return allArtwork = art.ToArray();
		}

		public string ServiceExtra { get; set; }
	}
}