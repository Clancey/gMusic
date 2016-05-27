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
	[Register ("AppDelegate")]
	partial class AppDelegate
	{
		[Outlet]
		AppKit.NSView _mainContentView { get; set; }

		[Outlet]
		AppKit.NSOutlineView _sidebarOutlineView { get; set; }

		[Outlet]
		AppKit.NSWindow window { get; set; }

		[Action ("sidebarMenuDidChange:")]
		partial void sidebarMenuDidChange (Foundation.NSObject notification);
		
		void ReleaseDesignerOutlets ()
		{
			if (_mainContentView != null) {
				_mainContentView.Dispose ();
				_mainContentView = null;
			}

			if (_sidebarOutlineView != null) {
				_sidebarOutlineView.Dispose ();
				_sidebarOutlineView = null;
			}

			if (window != null) {
				window.Dispose ();
				window = null;
			}
		}
	}
}
