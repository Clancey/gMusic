using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace MusicPlayer.Models
{
	internal class TempArtistArtwork : ArtistArtwork
	{
	}

	public class ArtistArtwork : Artwork
	{
		[Indexed]
		public string ArtistId { get; set; }

		public override void SetId()
		{
			Id = $"{ArtistId} - {Url}";
		}
	}
}