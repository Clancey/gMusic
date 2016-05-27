using System;
using UIKit;
using CoreGraphics;
namespace MusicPlayer.iOS
{
	public class TabButton : UIControl
	{
		public UIImageView ImageView { get; }
		public UILabel Label { get; }

		public string Title
		{
			get
			{
				return Label?.Text ?? "";
			}

			set
			{
				Label.Text = value;
			}
		}

		public string ImageSvg { get; set; }

		public TabButton(string title, string svg, nint tag) : this()
		{
			Title = title;
			ImageSvg = svg;
			this.Tag = tag;
		}
		public TabButton()
		{
			Add(ImageView = new UIImageView
			{
				ContentMode = UIViewContentMode.ScaleAspectFit,
			});
			Add(Label = new UILabel
			{
				Font = Fonts.NormalFont(200),
				AdjustsFontSizeToFitWidth = true,
				TextAlignment = UITextAlignment.Center,
				TextColor = UIColor.White,
			});
		}

		public int TextHeight { get; } = 4;
		public int SidePadding { get; } = 1;
		public int Spacing { get; } = 1;

		public nfloat ActualFontSize { get; private set;}
		CGSize currentSize;
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			var bounds = Bounds;
			if (bounds.Size == currentSize)
				return;
			currentSize = bounds.Size;
			var rowHeight = bounds.Height / 18;

			var sidePadding = rowHeight * SidePadding;
			var padding = rowHeight * Spacing;
			var textHeight = rowHeight * TextHeight;
			var frame = new CGRect(sidePadding, bounds.Height - textHeight - sidePadding, bounds.Width - sidePadding * 2, textHeight);
			Label.Frame = frame;
			ActualFontSize = textHeight * .8f;
			Label.Font = Fonts.NormalFont(ActualFontSize);

			var bottom = frame.Y - padding - sidePadding;
			frame = new CGRect(sidePadding, sidePadding, frame.Width, bottom);
			ImageView.Frame = frame;

			if (!string.IsNullOrWhiteSpace(ImageSvg))
				ImageView.LoadSvg(ImageSvg, UIImageRenderingMode.AlwaysTemplate);

		}
		public override void TintColorDidChange()
		{
			base.TintColorDidChange();
			Label.TextColor = TintColor ?? UIColor.White;
		}
	}
}

