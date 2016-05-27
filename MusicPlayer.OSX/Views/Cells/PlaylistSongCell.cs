using System;
using MusicPlayer.Models;
using AppKit;

namespace MusicPlayer
{
	public class PlaylistSongCell : BaseCell
	{
		public PlaylistSongCell ()
		{
		}
		public override AppKit.NSView GetCell (AppKit.NSTableView tableView, AppKit.NSTableColumn tableColumn, Foundation.NSObject owner)
		{
			var textField = tableView.MakeView ("Text", owner) as NSTextField ?? new NSTextField ().StyleAsMainText ();
			textField.StringValue = GetCellText (tableColumn);
			return textField;
		}

		public override string GetCellText (NSTableColumn tableColumn)
		{
			var psong = BindingContext as PlaylistSong;
			var song = psong?.Song;
			switch (tableColumn.Identifier.ToLower ()) {
			case "title":
				return song?.Name??"";
			case "artist":
				return song?.Artist??"";
			case "album":
				return song?.Album??"";
			default:
				return song?.ToString ()??"";
			}
		}
	}
}
