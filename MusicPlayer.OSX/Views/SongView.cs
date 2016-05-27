using System;
using MusicPlayer.ViewModels;
using MusicPlayer.Models;
using AppKit;
using System.Linq;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class SongView :  BaseTableView<SongViewModel,Song>, ILifeCycleView
	{
		public SongView ()
		{
			//BackgroundColor = NSColor.Blue;
			TableView.AddColumn (new NSTableColumn ("Title"){ Title = "Title" });
			TableView.AddColumn (new NSTableColumn ("Artist"){ Title = "Artist" });
			TableView.AddColumn (new NSTableColumn ("Album"){ Title = "Album" });
			//TableView.RowHeight = 30;
			TableView.DoubleClick += (object sender, EventArgs e) => {
				var item = Model.GetItem(TableView.SelectedRow);
				Model.PlayItem(item);
			};
			Model = new SongViewModel {AutoPlayOnSelect = false};
		}

		#region ILifeCycleView implementation
		public void ViewWillAppear ()
		{
			NotificationManager.Shared.SongDatabaseUpdated += MusicPlayer_Managers_NotificationManager_Shared_SongDatabaseUpdated;
		}

		void MusicPlayer_Managers_NotificationManager_Shared_SongDatabaseUpdated (object sender, EventArgs e)
		{
			TableView.ReloadData ();
		}
		public void ViewWillDissapear ()
		{
			NotificationManager.Shared.SongDatabaseUpdated -= MusicPlayer_Managers_NotificationManager_Shared_SongDatabaseUpdated;
		}
		#endregion
	}
}

