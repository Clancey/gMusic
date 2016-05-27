using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Api;
using SQLite;

namespace MusicPlayer.Models
{
	public class Artwork
	{
		[PrimaryKey, MaxLength(250)]
		public string Id { get; set; }

		public int Width { get; set; }
		public int Height { get; set; }
		public int Ratio { get; set; }

		[MaxLength(500)]
		public string Url { get; set; }

		public ServiceType ServiceType { get; set; }

		public virtual void SetId()
		{
			Id = $"{ServiceType} - {Url}";
		}
	}
}