using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicPlayer.Api;
using SQLite;
using MusicPlayer.Data;
using System.Threading.Tasks;
using Localizations;

namespace MusicPlayer.Models
{
	public class OnlineAlbum : Album
	{
		public OnlineAlbum()
		{
			
		}
		public OnlineAlbum(string name, string nameNorm) : base(name, nameNorm)
		{

		}
		public string AlbumId { get; set; }

		public ServiceType ServiceType { get; set; }
		public override string DetailText => ArtistString;
	}
	public class TempAlbum : Album
	{
	}

	public class Album : MediaItemBase
	{
		public Album()
		{
		}

		public Album(string name) : base(name)
		{
		}

		public Album(string name, string nameNorm) : base(name, nameNorm)
		{
		}

		int year;

		public int Year
		{
			get { return year; }
			set { ProcPropertyChanged(ref year, value); }
		}

		int trackCount;

		public int TrackCount
		{
			get { return trackCount; }
			set { ProcPropertyChanged(ref trackCount, value); }
		}

		string albumArtist;

		public string AlbumArtist
		{
			get { return albumArtist; }
			set { ProcPropertyChanged(ref albumArtist, value); }
		}

		public string Artist { get; set; }

		string artistId;

		[Indexed]
		public string ArtistId
		{
			get { return artistId; }
			set { ProcPropertyChanged(ref artistId, value); }
		}

		bool isCompilation;

		public bool IsCompilation
		{
			get { return isCompilation; }
			set { ProcPropertyChanged(ref isCompilation, value); }
		}

		AlbumArtwork[] allArtwork;

		[Ignore]
		public AlbumArtwork[] AllArtwork
		{
			set { allArtwork = value; }
		}
		public async Task<AlbumArtwork[]> GetAllArtwork()
		{

			if (allArtwork != null)
				return allArtwork;

			var art = await Database.Main.TablesAsync<AlbumArtwork>().Where(x => x.AlbumId == Id).ToListAsync() ??
				new List<AlbumArtwork>();
			var tempArtwork = await
				Database.Main.TablesAsync<TempAlbumArtwork>().Where(x => x.AlbumId == Id).ToListAsync();
			if (tempArtwork != null)
				art.AddRange(tempArtwork);
			return allArtwork = art.ToArray();
		}

		public override string ToString()
		{
			return $"{Name} - {ArtistId} - {AlbumArtist}";
		}

		string SongString => TrackCount > 1 ? Strings.Songs : Strings.Song;

		public string ArtistString
			=> IsCompilation ? "Various Artists" : string.IsNullOrWhiteSpace(AlbumArtist) ? Artist : AlbumArtist;

		public override string DetailText 
		{
			get{ return TrackCount == 0 ? ArtistString : $"{ArtistString} • {TrackCount} {SongString}"; }
		}


		public override bool ShouldBeLocal()
		{
			return Database.Main.GetObject<AlbumOfflineClass>(Id)?.ShouldBeLocal == true;
		}
	}
}