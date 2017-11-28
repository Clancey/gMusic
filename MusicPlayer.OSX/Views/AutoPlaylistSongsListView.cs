using System;
using MusicPlayer.ViewModels;
using AppKit;
using MusicPlayer.Models;
namespace MusicPlayer
{
	public class AutoPlaylistSongsListView : BaseTableView<AutoPlaylistSongViewModel, Song>
	{
		public AutoPlaylistSongsListView(AutoPlaylist playlist)
		{
			//BackgroundColor = NSColor.Blue;
			TableView.AddColumn(new NSTableColumn("Title") { Title = "Title" });
			TableView.AddColumn(new NSTableColumn("Artist") { Title = "Artist" });
			TableView.AddColumn(new NSTableColumn("Album") { Title = "Album" });
			TableView.DoubleClick += (object sender, EventArgs e) => {
				var item = Model.GetItem(TableView.SelectedRow);
				Model.PlayItem(item);
			};

			Model = new AutoPlaylistSongViewModel
			{
				Playlist = playlist,
				AutoPlay = false,
			};
		}
	}
}

