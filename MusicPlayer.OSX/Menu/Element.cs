using System;
using Foundation;
using AppKit;

namespace MusicPlayer
{
	public class Element : NSCopying
	{
		public string Text { get; set; }

		public Action Tapped { get; set;}

		public bool ShouldOutline { get; set; }

		public bool ShouldDeselect { get; set; }

		public Element ()
		{
			
		}

		public virtual NSView GetView(NSTableView tableView,NSObject sender)
		{
			var field = tableView.MakeView ("HeaderTextField", sender) as NSTextField;// ?? new NSTextField();
			field.StyleAsMainText();
			field.StringValue = Text;
			return field;
		}


		#region implemented abstract members of NSCopying
		public override NSObject Copy (NSZone zone)
		{
			return new NSString(Text);
		}
		#endregion
	}
}

