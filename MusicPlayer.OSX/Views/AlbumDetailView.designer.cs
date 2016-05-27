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
	[Register ("AlbumDetailView")]
	partial class AlbumDetailView
	{
		[Outlet]
		AppKit.NSImageView BackgroundImageView { get; set; }

		[Outlet]
		AppKit.NSTextField DetailLabel { get; set; }

		[Outlet]
		AppKit.NSImageView ImageView { get; set; }

		[Outlet]
		AppKit.NSColorView LineView { get; set; }

		[Outlet]
		AppKit.NSButton MoreButton { get; set; }

		[Outlet]
		AppKit.NSButton PlayButton { get; set; }

		[Outlet]
		AppKit.NSButton ShuffleButton { get; set; }

		[Outlet]
		AppKit.NSTableView TableView { get; set; }

		[Outlet]
		AppKit.NSTextField TitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TableView != null) {
				TableView.Dispose ();
				TableView = null;
			}

			if (BackgroundImageView != null) {
				BackgroundImageView.Dispose ();
				BackgroundImageView = null;
			}

			if (DetailLabel != null) {
				DetailLabel.Dispose ();
				DetailLabel = null;
			}

			if (ImageView != null) {
				ImageView.Dispose ();
				ImageView = null;
			}

			if (LineView != null) {
				LineView.Dispose ();
				LineView = null;
			}

			if (MoreButton != null) {
				MoreButton.Dispose ();
				MoreButton = null;
			}

			if (PlayButton != null) {
				PlayButton.Dispose ();
				PlayButton = null;
			}

			if (ShuffleButton != null) {
				ShuffleButton.Dispose ();
				ShuffleButton = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}
		}
	}
}
