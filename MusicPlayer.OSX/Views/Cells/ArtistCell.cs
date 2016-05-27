using System;
using MusicPlayer.Models;
using AppKit;
using UIKit;
using CoreGraphics;

namespace MusicPlayer
{
	public class ArtistCell : BaseCell
	{
		public ArtistCell ()
		{
		}

		#region implemented abstract members of BaseCell

		public override AppKit.NSView GetCell (AppKit.NSTableView tableView, AppKit.NSTableColumn tableColumn, Foundation.NSObject owner)
		{
			var cell = tableView.MakeView (ArtistCellView.Key, owner) as ArtistCellView ?? new ArtistCellView ();
			cell.UpdateValues (BindingContext as Artist);
			return cell;
		}

		public override string GetCellText (AppKit.NSTableColumn tableColumn)
		{
			var artist = BindingContext as Artist;
			return artist.ToString ();
		}

		class ArtistCellView : NSTableCellView
		{
			public const string Key = "ArtistCellView";

			public TwoLabelView TextView {get;private set;}

			public NSImageView OfflineImageView { get; private set; }

			public ArtistCellView ()
			{
				Frame = new CoreGraphics.CGRect (0, 0, 250, 250);
				Identifier = Key;
				AddSubview(ImageView = new NSImageView(new CGRect(0,0,ImageWidth,ImageWidth)));
				AddSubview(TextView = new TwoLabelView() );
				AddSubview(OfflineImageView = new NSImageView(new CGRect(0,0,offlineIconWidth,offlineIconWidth)));
			}

			public void UpdateValues (Artist artist)
			{
				TextView.TopLabel.StringValue = artist?.Name ?? "";
				TextView.BottomLabel.StringValue = artist?.DetailText ?? "";
				//TODO: default iamge
				ImageView.LoadFromItem (artist);
			}

			protected const float Padding = 5f;
			protected const float TwicePadding = Padding * 2;
			protected const float ImageWidth = 35f;
			const float durationWidth = 30f;
			const float ratingWidth = 50f;
			const float minTitleWidth = 200f;
			const float accessoryWidth = 30f;
			const float offlineIconWidth = 13f;

			public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
			{
				base.ResizeSubviewsWithOldSize (oldSize);
				var bounds = Bounds;

				var frame = bounds;
				frame.Width = ImageWidth + TwicePadding;
				ImageView.Frame = new CoreGraphics.CGRect(Padding,(frame.Height - ImageWidth)/2,ImageWidth,ImageWidth);

				var x = frame.Right + Padding;

				var offIconW = NMath.Min (offlineIconWidth, OfflineImageView.Frame.Width);
				var right = bounds.Right;
				var width = right - x - offIconW;
				frame.Width = width;
				frame.X = x;
				TextView.Frame = frame;

				x = frame.Right;
				frame.Width = bounds.Width - frame.Right;
				frame.X = x;

				//OfflineImageView.Center = frame.GetCenter ();

				//MediaTypeImage.Frame = new CGRect (aLeft + Padding, bounds.Height - offlineIconWidth - Padding, offlineIconWidth, offlineIconWidth);

			}

			public override bool IsFlipped {
				get {
					return true;
				}
			}
		}

		#endregion
	}
}

