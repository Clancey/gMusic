using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using UIKit;

namespace MusicPlayer.iOS
{
	internal class BluredView : UIView
	{
		UIView blurView;

		public BluredView (): this(UIBlurEffectStyle.ExtraLight)
		{

		}
		public BluredView(UIBlurEffectStyle style)
		{
			UpdateStyle(style);
		}
		public void UpdateStyle(UIBlurEffectStyle style)
		{
			blurView?.RemoveFromSuperview();
			if (Device.IsIos8)
			{
				var blur = UIBlurEffect.FromStyle(style);
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

	internal class BlurredImageView : UIView
	{
		readonly UIImageView imageView;
		UIView blurView;

		public BlurredImageView()
		{
			Add(imageView = new UIImageView {ContentMode = UIViewContentMode.ScaleAspectFill});

			UpdateStyle(UIBlurEffectStyle.ExtraLight);

			Add(blurView);
			this.ClipsToBounds = true;
		}

		public void UpdateStyle(UIBlurEffectStyle style)
		{
			blurView?.RemoveFromSuperview();
			if (Device.IsIos8)
			{
				var blur = UIBlurEffect.FromStyle(style);
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
		UIImage image;

		public UIImage Image
		{
			get { return image; }
			set
			{
				image = value;
				UpdateImage();
			}
		}
		

		void UpdateImage()
		{
			UIView.Transition(imageView, .25, UIViewAnimationOptions.TransitionCrossDissolve, () => { imageView.Image = image; },
				null);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			imageView.Frame = Bounds;
			if (blurView != null)
				blurView.Frame = Bounds;
		}
	}
}