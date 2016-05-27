using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using SimpleDatabase;
using MusicPlayer.Data;

namespace MusicPlayer.Models
{
	public class OnlinePlaylistEntry : PlaylistSong
	{
		Song song;
		public override Song Song => song ?? (song = base.Song);

		public OnlineSong OnlineSong
		{
			get {return song as OnlineSong;}
			set { song = value; }
		}
	}
	public class TempPlaylistEntry
	{
		[PrimaryKey]
		public string PlaylistEntryId { get; set; }

		[Indexed]
		public string TrackId { get; set; }

		public string PlaylistId { get; set; }

		public long LastUpdate { get; set; }

		public long SOrder { get; set; }
	}

	public class TempPlaylistSong : PlaylistSong
	{
		
	}
	public class PlaylistSong
	{
		public PlaylistSong()
		{
		}

		[Indexed]
		public string PlaylistId { get; set; }

		[PrimaryKey]
		public string Id { get; set; }

		[Indexed]
		public string SongId { get; set; }

		[OrderBy]
		public long SOrder { get; set; }

		[Ignore]
		public virtual Song Song => Database.Main.GetObject<Song, TempSong>(SongId);

		[Indexed]
		public int OfflineCount { get; set; }


		[Indexed]
		public long LastUpdate { get; set; }

		[Indexed]
		public bool Deleted { get; set; }

		[Indexed]
		public string ServiceId { get; set; }

		public override string ToString()
		{
			return Song?.ToString() ?? base.ToString();
		}
	}
}