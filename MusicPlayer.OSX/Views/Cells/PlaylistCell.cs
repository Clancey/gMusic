using System;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class PlaylistCell : BaseCell
	{
		public PlaylistCell ()
		{
		}


		public override AppKit.NSView GetCell (AppKit.NSTableView tableView, AppKit.NSTableColumn tableColumn, Foundation.NSObject owner)
		{
			var cell = tableView.MakeView (MutliImageMediaCellView.Key, owner) as MutliImageMediaCellView ?? new MutliImageMediaCellView ();

			cell.UpdateValues (BindingContext as Playlist);
			return cell;
		}

		public override string GetCellText (AppKit.NSTableColumn tableColumn)
		{
			var playlist = BindingContext as Playlist;
			return playlist.ToString ();
		}
	}
}
