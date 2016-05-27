using System;
using AppKit;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class PlaylistView : BaseSplitView<PlaylistListView>
	{
		
		public PlaylistView () : base( new PlaylistListView())
		{
			
		}

		public override void ViewWillAppear ()
		{
			base.ViewWillAppear ();
			SideBar.Model.ItemSelected += SideBar_Model_ItemSelected;
			NotificationManager.Shared.PlaylistsDatabaseUpdated += NotificationManager_Shared_PlaylistsDatabaseUpdated;
		}

		void NotificationManager_Shared_PlaylistsDatabaseUpdated (object sender, EventArgs e)
		{
			SideBar.TableView.ReloadData ();
		}

		public override void ViewWillDissapear ()
		{
			base.ViewWillDissapear ();
			SideBar.Model.ItemSelected -= SideBar_Model_ItemSelected;
			NotificationManager.Shared.PlaylistsDatabaseUpdated -= NotificationManager_Shared_PlaylistsDatabaseUpdated;
		}

		void SideBar_Model_ItemSelected (object sender, SimpleTables.EventArgs<MusicPlayer.Models.Playlist> e)
		{
			CurrentView = new PlaylistSongsListView (e.Data);
		}
	}
}

