using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using UIKit;
using SimpleTables;

namespace MusicPlayer.iOS.ViewControllers
{
	internal class CurrentPlaylistViewController : BaseEditTableViewController
	{
		CurrentPlaylistViewModel model;

		public CurrentPlaylistViewController()
		{
			ShouldHideMenu = true;
		}

		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = model = new CurrentPlaylistViewModel();
			Title = model.Title;

			this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.GetCloseImage(15), UIBarButtonItemStyle.Plain,
				(s, e) => { this.DismissModalViewController(true); });
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			if (model.RowsInSection(0) > PlaybackManager.Shared.CurrentSongIndex)
				TableView.ScrollToRow(NSIndexPath.FromItemSection(PlaybackManager.Shared.CurrentSongIndex, 0),
					UITableViewScrollPosition.Top, true);
		}

		public override void SetupEvents()
		{
			NotificationManager.Shared.CurrentSongChanged += HandleCurrentSongChanged;
		}

		public override void TeardownEvents()
		{
			NotificationManager.Shared.CurrentSongChanged -= HandleCurrentSongChanged;
		}

		void HandleCurrentSongChanged(object sender, EventArgs<Song> e)
		{
			TableView.ReloadData();
			if (model.RowsInSection(0) > PlaybackManager.Shared.CurrentSongIndex)
				TableView.ScrollToRow(NSIndexPath.FromItemSection(PlaybackManager.Shared.CurrentSongIndex, 0),
					UITableViewScrollPosition.Top, true);
		}
	}
}