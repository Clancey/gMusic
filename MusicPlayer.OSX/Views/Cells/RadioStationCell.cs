using System;
using MusicPlayer.Models;
using SimpleTables;
using AppKit;
using CoreGraphics;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class RadioStationCell : BaseCell,ICollectionCell
	{
		public RadioStationCell ()
		{
		}

		public override AppKit.NSView GetCell (AppKit.NSTableView tableView, AppKit.NSTableColumn tableColumn, Foundation.NSObject owner)
		{
			var cell = tableView.MakeView (MediaCellView.Key, owner) as MediaCellView ?? new MediaCellView ();
			cell.UpdateValues (BindingContext as RadioStation);
			return cell;
		}

		public override string GetCellText (AppKit.NSTableColumn tableColumn)
		{
			var radioStation = BindingContext as RadioStation;
			return radioStation.ToString ();
		}

		#region ICollectionCell implementation

		public AppKit.NSCollectionViewItem GetCollectionCell (AppKit.NSCollectionView collectionView, Foundation.NSIndexPath indexPath)
		{
			var cell = collectionView.MakeItem ("RadioStationCollectionItem", indexPath);
			if (cell == null) {
				cell = new NSCollectionViewItem () {
					Identifier = "RadioStationCollectionItem",
				};
				cell.View = new RadioStationCollectionItem ();
			}
			var view = cell.View as RadioStationCollectionItem;
			view.Update (BindingContext as RadioStation);
			return cell;
		}

		#endregion

		class RadioStationCollectionItem : NSColorView
		{
			NSImageView ImageView;
			NSTextField Label;

			public RadioStationCollectionItem ()
			{
				AddSubview (ImageView = new NSImageView {
					ImageFrameStyle = NSImageFrameStyle.Photo,
					ImageAlignment = NSImageAlignment.Top,
					ImageScaling = NSImageScale.ProportionallyUpOrDown,
				});
				AddSubview (Label = new NSTextField {
					BackgroundColor = NSColor.White.ColorWithAlphaComponent (.5f),
					Alignment = NSTextAlignment.Center,
				}.StyleAsMainText());
				this.AddGestureRecognizer(new NSClickGestureRecognizer(()=>{
					var station = Station;
					if(station == null)
						return;
					PlaybackManager.Shared.Play(station);
				}){NumberOfClicksRequired = 2});
			}

			public RadioStationCollectionItem (IntPtr handle) : base (handle)
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
				Label.SizeToFit ();

				frame = Label.Frame;
				frame.X = 0;
				frame.Y = y + (height - frame.Height) / 2;
				frame.Width = bounds.Width;
				Label.Frame = frame;
			}
			WeakReference _station;
			RadioStation Station 
			{
				get{ return _station?.Target as RadioStation; }
				set { _station = value == null ? null : new WeakReference (value); }
			}

			public void Update (RadioStation station)
			{
				Station = station;
				Label.StringValue = station?.Name ?? "";

				ImageView.LoadFromItem (station);
			}
		}
	}
}