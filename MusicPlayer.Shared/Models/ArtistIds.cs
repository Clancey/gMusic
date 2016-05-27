using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Api;
using SQLite;

namespace MusicPlayer.Models
{
	internal class TempArtistIds : ArtistIds
	{
	}

	public class ArtistIds : BaseModel
	{
		[PrimaryKey]
		public string Id { get; set; }

		[Indexed]
		public string ArtistId { get; set; }

		[Indexed]
		public ServiceType ServiceType { get; set; }
	}
}