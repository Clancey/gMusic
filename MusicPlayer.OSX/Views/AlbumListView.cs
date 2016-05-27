using System;
using MusicPlayer.ViewModels;
using AppKit;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class AlbumListView : BaseTableView<AlbumViewModel,Album>
	{
		public AlbumListView ()
		{
			Frame = new CoreGraphics.CGRect (0, 0, 150, 1000);
			TableView.AddColumn (new NSTableColumn ("Artist"){ Title = "Artist"});
			TableView.HeaderView = null;
			TableView.UsesAlternatingRowBackgroundColors = false;
			TableView.RowHeight = 55;
			TableView.SizeLastColumnToFit ();
			Model = new AlbumViewModel{
				ShowHeaders = true,
			};
		}
	}
}

