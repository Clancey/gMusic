using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicPlayer.Api;
using MusicPlayer.Data;
using SQLite;

namespace MusicPlayer.Models
{
	public class OnlineSong : Song
	{
		public OnlineSong()
		{
			
		}
		public OnlineSong(string name, string namenorm) : base(name, namenorm)
		{
			
		}
		public FullTrackData TrackData { get; set; }

	}
	public class TempSong : Song
	{
		public TempSong()
		{
		}

		public TempSong(string name, string nameNorm) : base(name, nameNorm)
		{
		}

		[Indexed]
		public string ParentId { get; set; }
	}

	public class Song : MediaItemBase
	{
		string serviceTypesString;

		public Song()
		{
		}

		public Song(string name, string nameNorm) : base(name, nameNorm)
		{
		}

		[Indexed]
		public string ArtistId { get; set; }

		public string Artist { get; set; }

		[Indexed]
		public string AlbumId { get; set; }

		public string Album { get; set; }

		[Indexed]
		public string Genre { get; set; }

		public int PlayedCount { get; set; }

		[Indexed]
		public long LastPlayed { get; set; }

		[Indexed]
		public int Track { get; set; }


		[Indexed]
		public int TrackCount { get; set; }

		[Indexed]
		public int Disc { get; set; }

		[Indexed]
		public int Rating { get; set; }

		[Indexed]
		public int Year { get; set; }

		public void SetId()
		{
			Id = $"{Name}-{ArtistId}-{AlbumId}";
		}
		[Indexed]
		public bool ExcludedOffline { get; set; }
		public override bool ShouldBeLocal()
		{
			return Database.Main.GetObject<SongOfflineClass>(Id)?.ShouldBeLocal == true;
		}

		public string ServiceTypesString
		{
			get { return serviceTypesString; }
			set
			{
				serviceTypesString = value;
				ServiceTypes =
					value?.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
						.Distinct()
						.Select(x => (ServiceType) int.Parse(x))
						.ToArray() ?? new ServiceType[0];
			}
		}

		[Ignore]
		public ServiceType[] ServiceTypes { get; private set; } = new ServiceType[0];

		[Ignore]
		public MediaType[] MediaTypes { get; private set; } = new MediaType[0];


		string mediaTypesString;
		public string MediaTypesString
		{
			get { return mediaTypesString; }
			set
			{
				mediaTypesString = value;
				MediaTypes =
					value?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
						.Distinct()
						.Select(x => (MediaType)int.Parse(x))
						.ToArray() ?? new MediaType[0];
			}
		}

		[Ignore]
		public bool HasVideo => MediaTypes.Contains(MediaType.Video);

		public override string ToString()
		{
			return $"{Name} - {Artist}";
		}

		public string ToString (int max)
		{
			var s = this.ToString ();
			if (s.Length <= max)
				return s;
			return s.Substring (0, Math.Max (max - 3, 3)) + "...";
		}
		public override string DetailText
			=>
				string.IsNullOrWhiteSpace(Artist) || string.IsNullOrWhiteSpace(Album)
					? $"{Artist}{Album}"
					: $"{Artist} - {Album}";
	}
}