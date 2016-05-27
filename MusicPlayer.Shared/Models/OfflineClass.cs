using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace MusicPlayer.Models
{
	public abstract class OfflineClass
	{
		[PrimaryKey]
		public string Id { get; set; }

		[Indexed]
		public bool ShouldBeLocal { get; set; }
	}

	public class AlbumOfflineClass : OfflineClass
	{
	}

	public class ArtistOfflineClass : OfflineClass
	{
	}

	public class PlaylistOfflineClass : OfflineClass
	{
	}

	public class SongOfflineClass : OfflineClass
	{
	}

	public class GenreOfflineClass : OfflineClass
	{
	}
}