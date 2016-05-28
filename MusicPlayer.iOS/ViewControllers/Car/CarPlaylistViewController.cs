using System;
using MusicPlayer.iOS.ViewControllers;
using UIKit;

namespace MusicPlayer.iOS.Car
{
	public class CarPlaylistViewController : PlaylistViewController
	{
		public CarPlaylistViewController()
		{
		}

		UIGestureRecognizer panGesture;
		public override void LoadView()
		{
			base.LoadView();
			TableView.SectionIndexMinimumDisplayRowCount = 2000000;
			//panGesture = TableView.AddGestures();
		}

		CoreGraphics.CGSize lastSize;
		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			var size = View.Bounds.Size;
			if (lastSize == size)
				return;
			lastSize = size;
			TableView.RowHeight = CarStyle.RowHeight;
		}
	}
}