using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Data;
using SQLite;
using System.Threading.Tasks;
using Localizations;

namespace MusicPlayer.Models
{
	public class OnlineArtist : Artist
	{
		public OnlineArtist()
		{
			
		}
		public OnlineArtist(string name, string nameNorm) : base(name, nameNorm)
		{

		}
		public string OnlineId { get; set; }
		public override string DetailText => "";
	}

	public class TempArtist : Artist
	{
	}

	public class Artist : MediaItemBase
	{
		public Artist()
		{
		}

		public Artist(string name) : base(name)
		{
		}

		public Artist(string name, string nameNorm) : base(name, nameNorm)
		{
			Id = nameNorm;
		}

		int songCount;

		public int SongCount
		{
			get { return songCount; }
			set { ProcPropertyChanged(ref songCount, value); }
		}

		int albumCount;

		public int AlbumCount
		{
			get { return albumCount; }
			set { ProcPropertyChanged(ref albumCount, value); }
		}

		public override string ToString()
		{
			return $"{Name}";
		}

		string AlbumString => AlbumCount > 1 ? Strings.Albums : Strings.Album;
		string SongString => SongCount == 1 ? Strings.Song : Strings.Songs;

		public override string DetailText => SongCount == 0 ? "" : $"{AlbumCount} {AlbumString} • {SongCount} {SongString}";

		ArtistArtwork[] allArtwork;

		[Ignore]
		public ArtistArtwork[] AllArtwork
		{
			set { allArtwork = value; }
		}

		public async Task<ArtistArtwork[]> GetAllArtwork()
		{
			if (allArtwork != null)
				return allArtwork;

			var art = await Database.Main.TablesAsync<ArtistArtwork>().Where(x => x.ArtistId == Id).ToListAsync() ??
				new List<ArtistArtwork>();
			var tempArtwork = await
				Database.Main.TablesAsync<TempArtistArtwork>().Where(x => x.ArtistId == Id).ToListAsync();
			if (tempArtwork != null)
				art.AddRange(tempArtwork);
			return allArtwork = art.ToArray();
		}

		public override bool ShouldBeLocal()
		{
			return Database.Main.GetObject<ArtistOfflineClass>(Id)?.ShouldBeLocal == true;
		}
	}
}