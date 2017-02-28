using System;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer
{
	public partial class ArtistAlbumsViewModel : AlbumViewModel
	{
		Artist artist;

		public Artist Artist
		{
			set
			{
				var group = new SimpleDatabase.GroupInfo
				{
					From = "Album",
					Params = { 
						{ "@ArtistId", value.Id }
					},
					Filter = "Id in (select distinct AlbumId from song where ArtistId = @ArtistId )",
					OrderBy = "Year, NameNorm"
				};
				Title = value.Name;
				GroupInfo = group;
				artist = value;
			}
			get { return artist; }
		}
	}
}