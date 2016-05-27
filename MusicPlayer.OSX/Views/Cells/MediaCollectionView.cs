using System;
using AppKit;
using Foundation;
using CoreGraphics;

namespace MusicPlayer
{
	[Register("MediaCollectionView")]
	public class MediaCollectionView : NSCollectionViewItem
	{
		public const string Key = "MediaCollectionView";
		public MediaCollectionView ()
		{
		}
		public MediaCollectionView(IntPtr handle) : base(handle)
		{
			this.TextField = new NSTextField (new CGRect(0,0,100,100));
			this.View.AddSubview (new NSColorView (new CGRect (0, 0, 200, 200)) {
				BackgroundColor = NSColor.Black,
			});
		}
	}
}

