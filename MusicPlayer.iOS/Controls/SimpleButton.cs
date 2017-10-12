using System;
using CoreGraphics;
using UIKit;

namespace MusicPlayer.iOS
{

	public class SimpleShadowButton : SimpleButton
	{

		UIImageView imageView;
		BluredView blur;
		public SimpleShadowButton(IntPtr handle) : base(handle)
		{
			init();
		}

		public SimpleShadowButton(Foundation.NSCoder coder) : base(coder)
		{
			init();
		}

		public SimpleShadowButton()
		{
			init();
		}

		void init()
		{
			Add(blur = new BluredView(UIBlurEffectStyle.Dark) { UserInteractionEnabled = false, Layer = {MasksToBounds = true } });
			Add(imageView = new UIImageView
			{
				Layer =
				{
					ShadowRadius = 11f,
					ShadowColor = UIColor.Black.CGColor,
					ShadowOffset = new CGSize(1f, 1f),
					ShadowOpacity = 2f,
				},
				ContentMode = UIViewContentMode.Center,
			});
		}

		public override UIImage Image
		{
			get { return imageView?.Image; }
			set
			{
				this.imageView.Image = value.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			imageView.Frame = Bounds;
			blur.Frame = Bounds.Inset(6,6);
			var corner = NMath.Min(blur.Frame.Width, blur.Frame.Height) / 2;
			blur.Layer.CornerRadius = corner;
		}
	}


	public class SimpleButton : UIButton
	{
		public SimpleButton(IntPtr handle) : base(handle)
		{
			init();
		}

		public SimpleButton(Foundation.NSCoder coder) : base(coder)
		{
			init();
		}

		public SimpleButton()
		{
			init();
		}

		void init()
		{
			this.TouchUpInside += (object sender, EventArgs e) => { Tapped?.Invoke(this); };
		}

		public new string Text
		{
			get { return this.CurrentTitle; }
			set
			{
				this.SetTitle(value, UIControlState.Normal);
				this.SizeToFit();
			}
		}

		public new UIColor TitleColor
		{
			get { return this.TitleColor(UIControlState.Normal); }
			set { this.SetTitleColor(value, UIControlState.Normal); }
		}

		public UIColor TitleSelectedColor
		{
			get { return this.TitleColor(UIControlState.Highlighted); }
			set { this.SetTitleColor(value, UIControlState.Highlighted); }
		}

		public virtual UIImage Image
		{
			get { return this.ImageForState(UIControlState.Normal); }
			set { this.SetImage(value, UIControlState.Normal); }
		}

		public UIImage BackgroundImage
		{
			get { return this.BackgroundImageForState(UIControlState.Normal); }
			set { this.SetBackgroundImage(value, UIControlState.Normal); }
		}

		public Action<SimpleButton> Tapped { get; set; }
	}
}