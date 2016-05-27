using System;
using MusicPlayer.ViewModels;
using AppKit;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class PlaylistSongsListView :  BaseTableView<PlaylistSongViewModel,PlaylistSong>
	{
		public PlaylistSongsListView (Playlist playlist)
		{
			//BackgroundColor = NSColor.Blue;
			TableView.AddColumn (new NSTableColumn ("Title"){ Title = "Title" });
			TableView.AddColumn (new NSTableColumn ("Artist"){ Title = "Artist" });
			TableView.AddColumn (new NSTableColumn ("Album"){ Title = "Album" });
			TableView.DoubleClick += (object sender, EventArgs e) => {
				var item = Model.GetItem (TableView.SelectedRow);
				Model.PlayItem (item);
			};
			Model = new PlaylistSongViewModel {
				AutoPlayOnSelect = false,
				Playlist = playlist,
			};
		}
	}
}

