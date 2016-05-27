using System;
using AppKit;
using CoreGraphics;

namespace MusicPlayer
{
	public class TwoLabelView : NSView
	{
		public float Pading { get; set; } = 5f;

		public NSTextField TopLabel { get; } = new NSTextField {
			Bezeled = false,
			DrawsBackground = false,
			Editable = false,
			Selectable = false,
		}.StyleAsMainText();

		public NSTextField BottomLabel { get; } = new NSTextField {
			Bezeled = false,
			DrawsBackground = false,
			Editable = false,
			Selectable = false,
		}.StyleAsSubText();

		public TwoLabelView()
		{
			AddSubview(TopLabel);
			AddSubview(BottomLabel);
		}
		public override bool IsFlipped {
			get {
				return true;
			}
		}

		bool isCentered;
		public bool IsCentered {
			get {
				return isCentered;
			}
			set {
				isCentered = value;
				TopLabel.Alignment = BottomLabel.Alignment = value ? NSTextAlignment.Center : NSTextAlignment.Left;
			}
		}
		public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			var bounds = Bounds;
			TopLabel.SizeToFit();
			BottomLabel.SizeToFit();


			var topHeight = string.IsNullOrWhiteSpace(TopLabel.StringValue) ? 0 : TopLabel.Frame.Height;
			var bottomH = string.IsNullOrWhiteSpace(BottomLabel.StringValue) ? 0 : BottomLabel.Frame.Height;
			//			if (tbHeights > 0 && bottomH > 0)
			//				tbHeights += Pading;
			var tbHeights = topHeight + bottomH;

			var y = (bounds.Height - tbHeights)/2;
			var frame = new CGRect(0, y, bounds.Width, topHeight);
			TopLabel.Frame = frame;
			y = frame.Bottom;

			frame = new CGRect(0, y, bounds.Width, bottomH);
			BottomLabel.Frame = frame;
		}
	}
}

