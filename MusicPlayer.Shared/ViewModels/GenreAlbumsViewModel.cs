using System;
using MusicPlayer.ViewModels;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class GenreAlbumsViewModel : AlbumViewModel
	{
		Genre genre;

		public Genre Genre
		{
			set
			{
				var group = new SimpleDatabase.GroupInfo
				{
					From = "Album",
					Params = { { "@GenreId", value.Id } },
					Filter = "Id in (select distinct AlbumId from song where Genre = @GenreId )",
					OrderBy = "Year, NameNorm"
				};
				Title = value.Name;
				GroupInfo = group;
				genre = value;
			}
			get { return genre; }
		}
	}
}