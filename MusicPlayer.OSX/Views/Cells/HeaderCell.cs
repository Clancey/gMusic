using System;
using SimpleTables;
using AppKit;

namespace MusicPlayer
{
	public class HeaderCell : ICell
	{
		public HeaderCell ()
		{
			
		}

		public string Title { get; set; }

		#region ICell implementation

		public AppKit.NSView GetCell (AppKit.NSTableView tableView, AppKit.NSTableColumn tableColumn, Foundation.NSObject owner)
		{
			var textField = tableView.MakeView ("Header", owner) as NSTextField ?? new NSTextField ().StyleAsHeaderText ();
			textField.Identifier = "Header";
			textField.StringValue = GetCellText (tableColumn);
			return textField;
		}

		public string GetCellText (AppKit.NSTableColumn tableColumn)
		{
			return Title;
		}

		#endregion
	}
}

