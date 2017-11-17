using System;
using MusicPlayer.Managers;
using UIKit;
using MusicPlayer.ViewModels;
using MusicPlayer.Models;

namespace MusicPlayer.iOS.ViewControllers
{
	public class PlaylistViewController : BaseTableViewController
	{
		PlaylistViewModel model;

		public PlaylistViewController()
		{
			model = new PlaylistViewModel();
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
			model.ItemSelected +=
				     (object sender, SimpleTables.EventArgs<MusicPlayer.Models.Playlist> e) =>
					 {
						if(e.Data is AutoPlaylist a)
							 this.NavigationController.PushViewController(new AutoPlaylistSongsViewController(a), true);
						 else
					
						 this.NavigationController.PushViewController(new PlaylistSongsViewController(e.Data), true);
					 };
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