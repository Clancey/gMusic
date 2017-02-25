using System;
using UIKit;
using CoreGraphics;

namespace MusicPlayer.iOS
{
	public class TwoLabelView : UIView
	{
		public float Pading { get; set; } = 5f;

		public UILabel TopLabel { get; } = new UILabel().StyleAsMainText();

		public UILabel BottomLabel { get; } = new UILabel().StyleAsSubText();

		public TwoLabelView()
		{
			Add(TopLabel);
			Add(BottomLabel);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var bounds = Bounds;
			TopLabel.SizeToFit();
			BottomLabel.SizeToFit();


			var topHeight = string.IsNullOrWhiteSpace(TopLabel.Text) ? 0 : TopLabel.Frame.Height;
			var bottomH = string.IsNullOrWhiteSpace(BottomLabel.Text) ? 0 : BottomLabel.Frame.Height;
//			if (tbHeights > 0 && bottomH > 0)
//				tbHeights += Pading;
			var tbHeights = topHeight + bottomH;

			var y = (bounds.Height - tbHeights)/2;
			var frame = new CGRect(0, y, bounds.Width, topHeight);
			TopLabel.Frame = frame;
			y = frame.Bottom + Pading;

			frame = new CGRect(0, y, bounds.Width, bottomH);
			BottomLabel.Frame = frame;
		}
		public void ApplyStyle()
		{
			TopLabel.StyleAsMainText();
			BottomLabel.StyleAsSubText();
		}
		public void ApplyStyle(Style style)
		{
			TopLabel.StyleAsMainText(style);
			BottomLabel.StyleAsSubText(style);
		}
	}
}