using System;
using MusicPlayer.Models;
using AppKit;
using MusicPlayer.Managers;
using CoreGraphics;

namespace MusicPlayer
{
	public class SongSearchCell : BaseCell
	{
		public SongSearchCell ()
		{
		}


		public override AppKit.NSView GetCell (AppKit.NSTableView tableView, AppKit.NSTableColumn tableColumn, Foundation.NSObject owner)
		{
			var cell = tableView.MakeView (MediaCellView.Key, owner) as MediaCellView ?? new MediaCellView ();
			cell.UpdateValues (BindingContext as Song);
			return cell;
		}

		public override string GetCellText (AppKit.NSTableColumn tableColumn)
		{
			var album = BindingContext as Song;
			return album.ToString ();
		}
	}
}