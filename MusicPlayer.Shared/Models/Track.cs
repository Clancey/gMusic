using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Api;
using SQLite;

namespace MusicPlayer.Models
{
	public class TempTrack : Track
	{
		[Indexed]
		public string ParentId { get; set; }
	}

	public class Track
	{
		/// <summary>
		/// ID from the Api/service
		/// </summary>
		[PrimaryKey, Indexed]
		public string Id { get; set; }

		[Indexed]
		public string SongId { get; set; }

		[Indexed]
		public string ArtistId { get; set; }

		[Indexed]
		public string AlbumId { get; set; }

		[Indexed]
		public string Genre { get; set; }

		[Indexed]
		public ServiceType ServiceType { get; set; }

		[Indexed]
		public string ServiceId { get; set; }

		public MediaType MediaType { get; set; }

		/// <summary>
		/// Gets or sets the duration in Seconds.
		/// </summary>
		/// <value>The duration in Seconds.</value>
		public double Duration { get; set; }

		public int Priority { get; set; }

		public long LastPlayed { get; set; }

		public int PlayCount { get; set; }

		[Indexed]
		public long LastUpdated { get; set; }

		[Indexed]
		public bool Deleted { get; set; }

		public string FileExtension { get; set; }

		[Ignore]
		public string FileName => ServiceType == ServiceType.FileSystem ? Id : $"{Id}.{FileExtension}";

		public string ServiceExtra { get; set; }

		public string ServiceExtra2 { get; set; }

		[MaxLength(520)]
		public string FileLocation {get;set;}
	}
}