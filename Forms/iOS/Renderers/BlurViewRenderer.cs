using System;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using Xamarin.Forms;
using MusicPlayer.Forms;
using MusicPlayer.Forms.iOS;

[assembly: ExportRenderer(typeof(BlurView), typeof(BlurViewRenderer))]
namespace MusicPlayer.Forms.iOS
{
	public class BlurViewRenderer : VisualElementRenderer<BlurView>
	{
		
		UIView blurView;
		public BlurViewRenderer()
		{
		}
		public BlurViewRenderer(UIBlurEffectStyle style)
		{
			UpdateStyle(style);
		}

		public void UpdateStyle()
		{
			UpdateStyle(Element?.BlurStyle ?? BlurStyle.ExtraLight);
		}
		public void UpdateStyle(BlurStyle style)
		{
			var blurStyle = UIBlurEffectStyle.ExtraLight;
			switch (style)
			{
				case BlurStyle.ExtraLight:
					blurStyle = UIBlurEffectStyle.ExtraLight;
					break;
				case BlurStyle.Light:
					blurStyle = UIBlurEffectStyle.Light;
					break;
				case BlurStyle.Dark:
					blurStyle = UIBlurEffectStyle.Dark;
					break;
				default:
					blurStyle = UIBlurEffectStyle.ExtraLight;
					break;
			}
			UpdateStyle(blurStyle);
		}
		protected override void OnElementChanged(ElementChangedEventArgs<BlurView> e)
		{
			base.OnElementChanged(e);
			UpdateStyle();
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(BlurView.BlurStyle))
				UpdateStyle();
			base.OnElementPropertyChanged(sender, e);
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
			this.InsertSubview(blurView, 0);
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
			Add(imageView = new UIImageView { ContentMode = UIViewContentMode.ScaleAspectFill });

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
