using System;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using MusicPlayer.Managers;
namespace MusicPlayer.iOS.ViewControllers
{
	public class AutoPlaylistSongsViewController : BaseTableViewController
	{
		AutoPlaylistSongViewModel model;

		public AutoPlaylistSongsViewController(AutoPlaylist playlist)
		{
			model = new AutoPlaylistSongViewModel
			{
				Playlist = playlist,
			};
			Title = model.Title;
		}

		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = model;
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			model.ClearEvents();
		}

		public override void SetupEvents()
		{
			NotificationManager.Shared.SongDatabaseUpdated += PlaylistDatabaseUpdated;
		}

		void PlaylistDatabaseUpdated(object sender, EventArgs eventArgs)
		{
			TableView.ReloadData();
		}

		public override void TeardownEvents()
		{
			NotificationManager.Shared.SongDatabaseUpdated -= PlaylistDatabaseUpdated;
		}
	}
}
