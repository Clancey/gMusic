using CoreGraphics;
using MusicPlayer.Models;
using UIKit;
using System;
using MusicPlayer.iOS;

namespace MusicPlayer
{
	class MediaItemCellView : UIView
	{
		public MediaItemCellView ()
		{
			BackgroundColor = UIColor.Clear;
		}
	}
	public class MediaItemCell : UITableViewCell
	{
		bool showOffline;
		protected const float Padding = 5f;
		protected const float TwicePadding = Padding*2;
		protected const float ImageWidth = 35f;
		const float durationWidth = 30f;
		const float ratingWidth = 50f;
		const float minTitleWidth = 200f;
		const float accessoryWidth = 30f;
		const float offlineIconWidth = 13f;

		public new UIImageView ImageView { get; set; }

		public UIImageView OfflineImageView { get; set; }
		public UIImageView MediaTypeImage { get; set; }
		public SimpleButton DisclosureButton { get; set; }
		public TwoLabelView TextView { get; set; }
		MediaItemCellView CellView;

		public MediaItemCell(string key,bool hasImageborder = true) : base(UITableViewCellStyle.Subtitle, key)
		{
			Add (CellView = new MediaItemCellView ());
			Add(ImageView = new UIImageView(new CGRect(0, 0, ImageWidth, ImageWidth))
			{
				Layer =
				{
					BorderColor = hasImageborder ?  UIColor.LightGray.CGColor : UIColor.Clear.CGColor,
					BorderWidth = .5f,
				},
				ContentMode = UIViewContentMode.ScaleAspectFit
			});
			Add(TextView = new TwoLabelView());
			Add(OfflineImageView = new UIImageView(Images.GetOfflineImage(offlineIconWidth).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate)){Hidden = true});
			Add(MediaTypeImage = new UIImageView(Images.GetVideoIcon(offlineIconWidth).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate)) { Hidden = true });
			AccessoryView = DisclosureButton = new SimpleButton
			{
				Image = Images.DisclosureImage.Value,
				Tapped = TappedAccessory,
			};
			TextLabel.TextColor = DetailTextLabel.TextColor = UIColor.Clear;

			var style = this.GetStyle();
			this.TintColor = style.AccentColor;
		}

		public virtual void ApplyStyle(UITableView tv)
		{
			var style = tv.GetStyle();
			this.BackgroundColor = style.BackgroundColor;
			this.TintColor = style.AccentColor;
			TextView.ApplyStyle(style);
		}


		public virtual void TappedAccessory(SimpleButton button)
		{
		}

		public void SetText(MediaItemBase item)
		{
			TextLabel.Text = TextView.TopLabel.Text = item?.Name ?? "";
			DetailTextLabel.Text = TextView.BottomLabel.Text = item?.DetailText ?? "";
			TextView.SetNeedsLayout();
		}

		public bool ShowOffline
		{
			get { return showOffline; }
			set
			{
				showOffline = value;
				OfflineImageView.Hidden = !ShowOffline;
			}
		}
		public float TextOffset {get;set; } = 0;
		CGSize lastSize;
		public override void LayoutSubviews()
		{
			var bounds = ContentView.Bounds;
			if (bounds.Size == lastSize)
				return;
			lastSize = bounds.Size;

			var padding = bounds.Height * .11f;
			var frameHeight = bounds.Height * .8f;
			CellView.Frame = Bounds;
			if (AccessoryView != null)
				AccessoryView.Frame = new CGRect(0, 0, frameHeight, frameHeight);
			base.LayoutSubviews();
			nfloat aLeft = 0;

			//Fix the accessoryview so its covers the whole right of the cell
			if (AccessoryView != null)
			{
				DisclosureButton.Image = Images.GetDisclosureImage(frameHeight/2, frameHeight/2);
				var aFrame = AccessoryView.Frame;
				var extraWidth = Bounds.Right - aFrame.Right;
				aFrame.Width += extraWidth;
				AccessoryView.Frame = aFrame;
				aLeft = aFrame.Left;
				bounds.Width = aLeft;
			}

			var leftPadding = this.GetSafeArea().Left;
			var frame = bounds;
			frame.Width = frame.Height = frameHeight;
			frame.Y = (bounds.Height - frame.Height) / 2;
			frame.X = padding + leftPadding;
			ImageView.Frame = frame;

			var x = frame.Right + padding + TextOffset;

			var offIconW = NMath.Min (offlineIconWidth, OfflineImageView.Frame.Width);
			var right = bounds.Right;
			var width = right - x - offIconW;
			frame.Width = width;
			frame.X = x;
			TextView.Frame = frame;

			x = frame.Right;
			frame.Width = bounds.Width - frame.Right;
			frame.X = x;

			OfflineImageView.Center = frame.GetCenter();

			MediaTypeImage.Frame = new CGRect(aLeft + Padding , bounds.Height - offlineIconWidth - Padding, offlineIconWidth,offlineIconWidth);

		}
	}
}