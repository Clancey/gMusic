using System;
using MusicPlayer.iOS.ViewControllers;
using CoreGraphics;
using UIKit;

namespace MusicPlayer.iOS.Car
{
	class CarRadioViewController : RadioStationViewController.RadioStationTab
	{
		public CarRadioViewController()
		{
		}

		UIGestureRecognizer panGesture;
		public override void LoadView()
		{
			base.LoadView();
			TableView.SectionIndexMinimumDisplayRowCount = 2000000;
			//panGesture = TableView.AddGestures();
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
		}

		CoreGraphics.CGSize lastSize;
		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			var size = View.Bounds.Size;
			if (lastSize == size)
				return;
			lastSize = size;
			TableView.RowHeight = CarStyle.RowHeight*.75f;
		}
	}
}