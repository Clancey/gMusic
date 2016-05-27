using System;
using AppKit;
using MusicPlayer.Models;
using MusicPlayer.Managers;


namespace MusicPlayer
{
	public class GenreView : BaseSplitView<GenreListView>
	{
		public GenreView () : base (new GenreListView ())
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
			SideBar.Model.ClearEvents ();
			SideBar.Model.GoToArtist = null;
			SideBar.Model.ItemSelected -= SideBar_Model_ItemSelected;

			NotificationManager.Shared.SongDatabaseUpdated -= MusicPlayer_Managers_NotificationManager_Shared_SongDatabaseUpdated;
		}

		void SideBar_Model_ItemSelected (object sender, SimpleTables.EventArgs<Genre> e)
		{
			CurrentView = new GenreDetailsListView (e.Data);	
		}
	}
}

