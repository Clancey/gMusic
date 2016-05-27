using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Data;
using SQLite;
using SimpleDatabase;

namespace MusicPlayer.Models
{
	public class TempRadioStationSong
	{
		[PrimaryKey]
		public string PlaylistEntryId { get; set; }

		[Indexed]
		public string TrackId { get; set; }

		public string PlaylistId { get; set; }

		public long SOrder { get; set; }
	}

	public class RadioStationSong
	{
		public RadioStationSong()
		{
		}

		[Indexed]
		public string StationId { get; set; }

		[PrimaryKey]
		public string Id { get; set; }

		[Indexed]
		public string SongId { get; set; }

		[OrderBy]
		public long SOrder { get; set; }

		[Ignore]
		public Song Song
		{
			get { return Database.Main.GetObject<Song, TempSong>(SongId); }
		}

		public override string ToString()
		{
			return Song?.ToString() ?? base.ToString();
		}
	}
}