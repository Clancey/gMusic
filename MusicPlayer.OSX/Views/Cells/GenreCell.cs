using System;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class GenreCell : BaseCell
	{
		public GenreCell ()
		{
		}


		public override AppKit.NSView GetCell (AppKit.NSTableView tableView, AppKit.NSTableColumn tableColumn, Foundation.NSObject owner)
		{
			var cell = tableView.MakeView (MutliImageMediaCellView.Key, owner) as MutliImageMediaCellView ?? new MutliImageMediaCellView ();
			cell.UpdateValues (BindingContext as Genre);
			return cell;
		}

		public override string GetCellText (AppKit.NSTableColumn tableColumn)
		{
			var genre = BindingContext as Genre;
			return genre.ToString ();
		}
	}
}
