// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace MusicPlayer
{
	[Register ("VideoPlaybackWindowController")]
	partial class VideoPlaybackWindowController
	{
		[Outlet]
		AppKit.NSView MainView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MainView != null) {
				MainView.Dispose ();
				MainView = null;
			}
		}
	}
}
