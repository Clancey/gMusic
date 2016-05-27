using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace MusicPlayer.iOS
{
	class DarkBlurView : UIView
	{
		UIView blurView;
		public DarkBlurView()
		{
			if (Device.IsIos8)
			{
				var blur = UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);
				blurView = new UIVisualEffectView(blur);
			}
			else
			{
				blurView = new UIToolbar
				{
					Opaque = true,
					Translucent = true,
					BarStyle = UIBarStyle.BlackTranslucent,
				};
			}
			Add(blurView);
		}
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			if (blurView != null)
				blurView.Frame = Bounds;
		}
	}
}
