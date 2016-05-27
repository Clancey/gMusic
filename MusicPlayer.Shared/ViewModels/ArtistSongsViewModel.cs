using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Data;
using MusicPlayer.Models;

namespace MusicPlayer.ViewModels
{
    class ArtistSongsViewModel : SongViewModel
    {

		Artist artist;

		public Artist Artist
		{
			set
			{
				var group = Database.Main.GetGroupInfo<Song>().Clone();
				group.Filter = "ArtistId = ?";
				group.Params = value.Id;
				group.From = "Song";
				Title = value.Name;
				GroupInfo = group;
				artist = value;
			}
			get { return artist; }
		}
    }
}
