using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
namespace MusicPlayer.iOS
{
	public class CarNavigation : UINavigationController
	{
		public CarNavigation(UIViewController controller) : base(controller)
		{
			SetValueForKey(new MyNavBar(), (NSString) "navigationBar");
		}

		public class MyNavBar : UINavigationBar
		{
			public MyNavBar()
			{
			}
			public override CoreGraphics.CGSize SizeThatFits(CoreGraphics.CGSize size)
			{
				var bounds = Frame.Size;
				bounds.Height = CarStyle.RowHeight;
				return bounds;
			}

			CGSize size;
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

				var subviews = Subviews.ToList();
				var bounds = Bounds;
				if (bounds.Size == size)
					return;
				size = bounds.Size;
				var backButton = subviews.OfType<UIButton>().FirstOrDefault();
				if (backButton != null)
				{
					Console.WriteLine(backButton);
					var frame = Bounds;
					frame.Height *= (30 / 44);
					frame.Width = frame.Height * (37 / 30);
					backButton.Frame = frame;

				}

				var style = this.GetStyle();
				this.BarStyle = style.BarStyle;
				this.SetTitleVerticalPositionAdjustment(style.MainTextFont.PointSize * -.5f, UIBarMetrics.Default);
				this.TitleTextAttributes = new UIStringAttributes
				{
					Font = style.MainTextFont.WithSize(style.MainTextFont.PointSize * 1.5f),
					ForegroundColor = UIColor.White,
				};

			}
		}

	}
}

