using System;
using AppKit;
using Foundation;

namespace MusicPlayer
{
	public class MenuImageElement : MenuElement
	{
		public string Svg { get; set; }

		public MenuImageElement ()
		{
		}
		public override AppKit.NSView GetView (NSTableView tableView,NSObject sender)
		{
			var cell = tableView.MakeView ("MainCell", sender) as  SidebarTableCellView ?? new SidebarTableCellView();
			cell.TextField.StringValue = Text;
			if(!string.IsNullOrWhiteSpace(Svg))
				cell.ImageView.LoadSvg (Svg, NSColor.ControlText);
			return cell;
		}
		public override NSObject Copy ()
		{
			return base.Copy ();
		}
	}
}

