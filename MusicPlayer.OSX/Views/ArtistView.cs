using System;
using AppKit;
using MusicPlayer.Models;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class ArtistView : BaseSplitView<ArtistListView>
	{

		public ArtistView (): base(new ArtistListView())
		{
			
		}
		public override void ViewWillAppear ()
		{
			base.ViewWillAppear ();
			SideBar.Model.ItemSelected += SideBar_Model_ItemSelected;

			NotificationManager.Shared.SongDatabaseUpdated += MusicPlayer_Managers_NotificationManager_Shared_SongDatabaseUpdated;
		}

		void MusicPlayer_Managers_NotificationManager_Shared_SongDatabaseUpdated (object sender, EventArgs e)
		{
			SideBar.TableView.ReloadData ();
		}

		public override void ViewWillDissapear ()
		{
			base.ViewWillDissapear ();
			SideBar.Model.ItemSelected -= SideBar_Model_ItemSelected;

			NotificationManager.Shared.SongDatabaseUpdated -= MusicPlayer_Managers_NotificationManager_Shared_SongDatabaseUpdated;
		}

		void SideBar_Model_ItemSelected (object sender, SimpleTables.EventArgs<Artist> e)
		{
			this.CurrentView = new ArtistAlbumsListView (e.Data);
		}
	}
}

