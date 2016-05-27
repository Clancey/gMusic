using System;
using System.Threading.Tasks;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using SimpleDatabase;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer.ViewModels
{
	public partial class PlaylistSongViewModel : BaseViewModel<PlaylistSong>
	{
		public PlaylistSongViewModel()
		{
		}

		#region implemented abstract members of BaseViewModel

		public override GroupInfo OfflineGroupInfo { get; set; }

		#endregion

		public static GroupInfo CreateGroupInfo(Playlist playlist)
		{
			return new GroupInfo
			{
				Filter = $"PlaylistId = \"{playlist.Id}\"",
				OrderBy = "SOrder"
			};
		}

		public static GroupInfo CreateOfflineGroupInfo(GroupInfo groupInfo)
		{
			var offlineGroupInfo = groupInfo.Clone();
			//TODO: Fix this
			offlineGroupInfo.From = "s inner join Song o on o.Id = s.SongId and o.OfflineCount > 0";
			return offlineGroupInfo;
		}


		Playlist playlist;

		public Playlist Playlist
		{
			get { return playlist; }
			set
			{
				if (playlist == value)
					return;
				playlist = value;
				GroupInfo = CreateGroupInfo(playlist);
				OfflineGroupInfo = CreateOfflineGroupInfo(GroupInfo);
				Title = Playlist.Name ?? "";
			}
		}
		public bool AutoPlayOnSelect {get;set;} = true;
		public override async void RowSelected(PlaylistSong item)
		{
			if (AutoPlayOnSelect)
				PlayItem (item);
		}

		public async void PlayItem(PlaylistSong item)
		{
			await PlaybackManager.Shared.PlayPlaylist(item, CurrentGroupInfo);
		}

		async void MoveSong(PlaylistSong song, string prev, string next, int position)
		{
			var success = await MusicManager.Shared.MoveSong(song, prev, next, position);
			if (success)
			{
				Database.Main.ClearMemory<PlaylistSong>();
				this.ReloadData();
			}
			//else
			//	App.ShowMessage("Error moving song.", "Please try again later", "Ok");
		}


		async Task<bool> DeleteSong(PlaylistSong song)
		{
			var success = await MusicManager.Shared.Delete(song);
			if (success)
			{
				Database.Main.ClearMemory<PlaylistSong>();
				Database.Main.ClearMemory<Playlist>();
				return true;
			}

			//App.ShowMessage("Error removing song.", "Please try again later", "Ok");
			return false;
		}
	}
}