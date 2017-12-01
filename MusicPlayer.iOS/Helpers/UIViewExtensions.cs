using System;
using UIKit;
namespace MusicPlayer.iOS
{
	public static class UIViewExtensions
	{
		public static bool IsLandscape (this UIView view)
		{
			var frame = view.Bounds;
			return frame.Width > frame.Height;
		}
		public static bool IsLandscape (this UIViewController vc)
		{
			return vc.View.IsLandscape ();
		}

		public static UIEdgeInsets GetSafeArea(this UIView view)
		{
			if (Device.IsIos11)
				return view.SafeAreaInsets;
			var margins = view.LayoutMargins;
			margins.Top = 64;
			return margins;
		}
	}
}

