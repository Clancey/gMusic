using System;
using MusicPlayer.Data;
using MusicPlayer.ViewModels;
using System.Linq;
using Localizations;

namespace MusicPlayer.Models
{
	public class AutoPlaylist : Playlist
	{
		public AutoPlaylist()
		{
			IsAutoPlaylist = true;
		}
		public string OrderByClause { get; set; }
		public string WhereClause { get; set; }
		public int Limit { get; set; }

		public override int SongCount
		{
			get
			{
				return Database.Main.GetObjectCount<Song>(AutoPlaylistSongViewModel.CreateGroupInfo(this, true));
			}
		}

		public static AutoPlaylist[] AutoPlaylists = {
			//new AutoPlaylist()
				//{
				//	Id = "latest",
				//	Name = "Last added",
				//	OrderByClause = " DateCreated Desc",
				//	Limit = 100,
				//},
			new AutoPlaylist()
				{
					Id = "recentlyPlayed",
				Name = Strings.RecentlyPlayed,
					OrderByClause = " LastPlayed Desc",
					Limit = 100,
				},
			new AutoPlaylist()
				{
					Id = "mostPlayed",
					Name = Strings.MostPlayed,
					OrderByClause = " PlayedCount Desc",
					Limit = 100,
				},
			new AutoPlaylist()
				{
					Id = "thumbsUp",
					Name = Strings.ThumbsUp,
					OrderByClause = " PlayedCount Desc",
					WhereClause = "Rating > 4",
				},
		};

	}
}
