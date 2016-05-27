using System;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using AppKit;

namespace MusicPlayer
{
	public class GenreListView : BaseTableView<GenreViewModel,Genre>
	{
		public GenreListView ()
		{
			Frame = new CoreGraphics.CGRect (0, 0, 150, 1000);
			TableView.AddColumn (new NSTableColumn ("Artist"){ Title = "Artist" });
			TableView.HeaderView = null;
			TableView.UsesAlternatingRowBackgroundColors = false;
			TableView.RowHeight = 55;
			TableView.SizeLastColumnToFit ();
			Model = new GenreViewModel ();
		}
	}
}

