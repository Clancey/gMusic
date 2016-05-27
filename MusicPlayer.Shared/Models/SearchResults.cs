using System;
using System.Collections.Generic;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class SearchResults
	{
		public List<Song> Songs {get;set;} = new List<Song>();
		public List<Song> Videos { get; set; } = new List<Song>();

		public List<Artist> Artist {get;set;} = new List<Artist>();

		public List<Album> Albums {get;set;} = new List<Album>();

		public List<Genre> Genres {get;set;} = new List<Genre>();

		public List<Playlist> Playlists {get;set;} = new List<Playlist>();

		public List<RadioStation> RadioStations { get; set; } = new List<RadioStation>();

		public string Query { get; set; }
	}
}

