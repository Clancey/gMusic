using System;
using MusicPlayer.ViewModels;
using MusicPlayer.Models;
using AppKit;

namespace MusicPlayer
{
	public class ArtistListView : BaseTableView<ArtistViewModel,Artist>
	{
		public ArtistListView ()
		{
			Frame = new CoreGraphics.CGRect (0, 0, 150, 1000);
			TableView.AddColumn (new NSTableColumn ("Artist"){ Title = "Artist"});
			TableView.HeaderView = null;
			TableView.UsesAlternatingRowBackgroundColors = false;
			TableView.RowHeight = 55;
			TableView.SizeLastColumnToFit ();
			Model = new ArtistViewModel ();
		}
	}
}

