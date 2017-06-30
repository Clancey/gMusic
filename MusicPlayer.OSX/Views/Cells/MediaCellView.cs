using System;
using MusicPlayer.Models;
using AppKit;
using CoreGraphics;
using MusicPlayer.Managers;
using System.Reactive.Linq;
using SDWebImage;

namespace MusicPlayer
{
	public class MediaCellView : NSTableCellView
	{
		public const string Key = "MediaCellView";

		public TwoLabelView TextView {get;private set;}

		public NSImageView OfflineImageView { get; private set; }

		public MediaCellView ()
		{
			Frame = new CoreGraphics.CGRect (0, 0, 250, 250);
			Identifier = Key;
			AddSubview(ImageView = new NSImageView(new CGRect(0,0,ImageWidth,ImageWidth)));
			AddSubview(TextView = new TwoLabelView() );
			AddSubview(OfflineImageView = new NSImageView(new CGRect(0,0,offlineIconWidth,offlineIconWidth)));
		}

		public virtual async void UpdateValues (MediaItemBase item)
		{
			TextView.TopLabel.StringValue = item?.Name ?? "";
			if (item?.Name?.Contains ("Peggy ") == true) {
				Console.WriteLine ("foo");
			}
			TextView.BottomLabel.StringValue = item?.DetailText ?? "";
			var width = (float)ImageView.Bounds.Width;
			var image = await item.GetLocalImage (width);
			try{
				if (image != null) {
					ImageView.Image = image;
				} else {
					var artUrl = await ArtworkManager.Shared.GetArtwork (item);
					if (!string.IsNullOrWhiteSpace(artUrl))
						ImageView.SetImage(new Foundation.NSUrl(artUrl));
					else
						ImageView.Image = Images.GetDefaultAlbumArt (width);
				}
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}
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

}

