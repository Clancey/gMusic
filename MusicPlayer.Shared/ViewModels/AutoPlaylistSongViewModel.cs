using System;
using SimpleDatabase;
using MusicPlayer.Models;
using MusicPlayer.Data;
using MusicPlayer.Managers;
namespace MusicPlayer.ViewModels
{
	public class AutoPlaylistSongViewModel : BaseViewModel<Song>
	{

		public override GroupInfo OfflineGroupInfo { get; set; }
		public bool AutoPlay { get; set; } = true;
		AutoPlaylist playlist;

		public AutoPlaylist Playlist
		{
			get { return playlist; }
			set
			{
				if (playlist == value)
					return;
				playlist = value;

				GroupInfo = AutoPlaylistSongViewModel.CreateGroupInfo(playlist, false);
				OfflineGroupInfo = AutoPlaylistSongViewModel.CreateGroupInfo(playlist, true);
				Title = Playlist.Name ?? "";
			}
		}

		public override void RowSelected(Song item)
		{
			if(AutoPlay)
				PlayItem(item);
		}

		public async void PlayItem(Song item)
		{
			await PlaybackManager.Shared.PlayAutoPlaylist(playlist, item, CurrentGroupInfo);
		}

		public static GroupInfo CreateGroupInfo(AutoPlaylist playlist, bool offlineOnly = false)
		{

			var gi = new GroupInfo { Filter = playlist.WhereClause, OrderBy = playlist.OrderByClause, Limit = playlist.Limit };
			if (offlineOnly && Settings.ShowOfflineOnly)
			{
				gi.Filter = gi.Filter + (string.IsNullOrEmpty(gi.Filter) ? " " : " and ") + "OfflineCount > 0";
			}
			return gi;
		}
	}
}
