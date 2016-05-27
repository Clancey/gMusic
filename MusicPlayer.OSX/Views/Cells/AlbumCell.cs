using System;
using MusicPlayer.Models;
using SimpleTables;
using AppKit;
using MusicPlayer.Managers;
using CoreGraphics;

namespace MusicPlayer
{
	public class AlbumCell : BaseCell,ICollectionCell
	{
		public AlbumCell ()
		{
		}


		public override AppKit.NSView GetCell (AppKit.NSTableView tableView, AppKit.NSTableColumn tableColumn, Foundation.NSObject owner)
		{
			var cell = tableView.MakeView (MediaCellView.Key, owner) as MediaCellView ?? new MediaCellView ();
			cell.UpdateValues (BindingContext as Album);
			return cell;
		}

		public override string GetCellText (AppKit.NSTableColumn tableColumn)
		{
			var album = BindingContext as Album;
			return album.ToString ();
		}
		#region ICollectionCell implementation

		public AppKit.NSCollectionViewItem GetCollectionCell (AppKit.NSCollectionView collectionView, Foundation.NSIndexPath indexPath)
		{
			var cell = collectionView.MakeItem ("AlbumCollectionItem", indexPath);
			if (cell == null) {
				cell = new NSCollectionViewItem () {
					Identifier = "AlbumCollectionItem",
				};
				cell.View = new AlbumCollectionItem ();
			}
			var view = cell.View as AlbumCollectionItem;
			view.Update (BindingContext as Album);
			return cell;
		}

		#endregion

		class AlbumCollectionItem : NSColorView
		{
			NSImageView ImageView;
			TwoLabelView Label;

			public AlbumCollectionItem ()
			{
				AddSubview (ImageView = new NSImageView {
					ImageFrameStyle = NSImageFrameStyle.Photo,
					ImageAlignment = NSImageAlignment.Top,
					ImageScaling = NSImageScale.ProportionallyUpOrDown,
				});
				AddSubview (Label = new TwoLabelView {IsCentered = true});
				this.AddGestureRecognizer(new NSClickGestureRecognizer(()=>{
					var station = Album;
					if(station == null)
						return;
					PlaybackManager.Shared.Play(station);
				}){NumberOfClicksRequired = 2});
			}

			public AlbumCollectionItem (IntPtr handle) : base (handle)
			{

			}


			public override bool IsFlipped {
				get {
					return true;
				}
			}

			public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
			{
				base.ResizeSubviewsWithOldSize (oldSize);
				var bounds = Bounds;
				var frame = new CGRect (0, 0, bounds.Width, bounds.Width);
				ImageView.Frame = frame;
				var y = frame.Bottom;
				var height = bounds.Height - y;
				frame.Y = y;
				frame.Height = height;
				frame.Width = bounds.Width;
				Label.Frame = frame;
			}

			WeakReference _album;
			Album Album 
			{
				get{ return _album?.Target as Album; }
				set { _album = value == null ? null : new WeakReference (value); }
			}

			public void Update (Album album)
			{
				Album = album;
				Label.TopLabel.StringValue = album?.Name ?? "";
				Label.BottomLabel.StringValue = album?.DetailText ?? "";

				ImageView.LoadFromItem (album);
			}
		}
	}
}