using System;
using MusicPlayer.Models;
using AppKit;

namespace MusicPlayer
{
	public class AlbumDetailsCell: BaseCell
	{
		public AlbumDetailsCell ()
		{
		}


		public override AppKit.NSView GetCell (AppKit.NSTableView tableView, AppKit.NSTableColumn tableColumn, Foundation.NSObject owner)
		{
			var cell = tableView.MakeView (AlbumDetailsCellView.Key, owner) as AlbumDetailsCellView ?? new AlbumDetailsCellView ();
			cell.Album = BindingContext as Album;
			return cell;
		}

		public override string GetCellText (AppKit.NSTableColumn tableColumn)
		{
			var album = BindingContext as Album;
			return album.ToString ();
		}

		const float BaseHeight = 125;
		const float MinHeight = 270;
		public float GetHeight ()
		{
			var album = BindingContext as Album;
			if (album == null)
				return 0;
			var rows = (int)(album.TrackCount / 2);
			var tableHeight = rows * 46;
			return Math.Max (MinHeight, tableHeight + BaseHeight);

		}

		class AlbumDetailsCellView : NSTableCellView
		{
			public const string Key = "AlbumDetailsCell";
//			NSImageView AlbumArt;
//			NSTextField TitleLabel;
//			NSTextField YearLabel;
//			NSButton playButton;
//			NSButton shuffleButton;
			AlbumDetailView view;

			public AlbumDetailsCellView()
			{
				var vc = new AlbumDetailViewController();
				view = vc.View;
				view.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
				AddSubview(view);
			}
			public Album Album {
				get {
					return view.Album;
				}
				set {
					view.Album = value;
				}
			}

			public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
			{
				base.ResizeSubviewsWithOldSize (oldSize);
				view.Frame = Bounds;
			}
		}

	}
}