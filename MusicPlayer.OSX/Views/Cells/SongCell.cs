using System;
using AppKit;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class SongCell : BaseCell
	{
		public SongCell ()
		{
			
		}

		public override AppKit.NSView GetCell (AppKit.NSTableView tableView, AppKit.NSTableColumn tableColumn, Foundation.NSObject owner)
		{
			var textField = tableView.MakeView ("Text", owner) as NSTextField ?? new NSTextField ().StyleAsLabel();
			textField.StringValue = GetCellText (tableColumn);
			return textField;
		}

		public override string GetCellText (NSTableColumn tableColumn)
		{
			var song = BindingContext as Song;
			switch (tableColumn.Identifier.ToLower ()) {
			case "title":
				return song?.Name ?? "";
			case "artist":
				return song?.Artist ?? "";
			case "album":
				return song?.Album ?? "";
			default:
				return song?.ToString () ?? "";
			}
		}
	}
}

