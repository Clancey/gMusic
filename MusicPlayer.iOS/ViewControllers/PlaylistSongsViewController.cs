using System;
using MusicPlayer.iOS.ViewControllers;
using MusicPlayer.Managers;
using UIKit;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer.iOS
{
	public class PlaylistSongsViewController : BaseEditTableViewController
	{
		PlaylistSongViewModel model;

		public PlaylistSongsViewController(Playlist playlist)
		{
			model = new PlaylistSongViewModel
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
			NotificationManager.Shared.PlaylistsDatabaseUpdated += PlaylistDatabaseUpdated;
		}

		void PlaylistDatabaseUpdated(object sender, EventArgs eventArgs)
		{
			TableView.ReloadData();
		}

		public override void TeardownEvents()
		{
			NotificationManager.Shared.PlaylistsDatabaseUpdated -= PlaylistDatabaseUpdated;
		}
	}
}