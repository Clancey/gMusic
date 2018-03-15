using System;
using AppKit;
using Foundation;
namespace MusicPlayer
{
	public class DetatchableNSPopoover : NSPopover, INSPopoverDelegate
	{
		public DetatchableNSPopoover()
		{
			this.Delegate = this;
		}

		[ExportAttribute("popoverShouldDetach:")]
		public bool ShouldDetach(NSPopover popover) => true;
	}

}
