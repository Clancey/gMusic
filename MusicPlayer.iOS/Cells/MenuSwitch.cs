using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using Foundation;
using MonoTouch.Dialog;
using NGraphics;
using UIKit;
using MusicPlayer.iOS;

namespace MusicPlayer.Cells
{

	class MenuSwitch : BoolElement, IElementSizing
	{
		public UIColor TextColor = Style.DefaultStyle.MenuTextColor;

		public MenuSwitch(string caption, bool value)
			: base(caption, value)
		{
		}

		public MenuSwitch(string caption, string svg, bool value)
			: base(caption, value)
		{
			image = svg.LoadImageFromSvg(new Size(28, 28), UIImageRenderingMode.AlwaysTemplate);
		}

		protected UIImage image;

		public UIImage Image {
			get {
				return image;
			}
			set {
				image = value != null ? value.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate) : value;
			}
		}

		#region IElementSizing implementation

		public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return 44;
		}

		#endregion

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell<BooleanMenuElementCell>(BooleanMenuElementCell.Key);
			cell.TextLabel.TextColor = TextColor;
            cell.TextLabel.Text = Caption;
			cell.Switch.On = Value;
			cell.ImageView.Image = image;
			cell.ValueChanged = () => {
				Value = cell.Switch.On;
			};
			if(cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = Detail ?? "";
			ApplyStyle(cell);
            return cell;
		}
		protected virtual void ApplyStyle(BooleanMenuElementCell cell)
		{
			cell.StyleAsMenuElement();
			cell.Switch.StyleSwitch();
		}
        void updateImages()
		{
			//sw.ThumbTintColor = sw.On ? Style.Current.Equalizer.SwitchOnThumbColor.Value : Style.Current.Equalizer.SwitchOffThumbColor.Value;
		}

		public class BooleanMenuElementCell : UITableViewCell
		{
			public const string Key = "BooleanMenuElementCell";
			protected const float Padding = 5f;
			protected const float TwicePadding = Padding * 2;
			protected const float ImageWidth = 35f;

			public bool ForceImage {get;set; } = true;

			public new UIImageView ImageView { get; private set; }

			public Action ValueChanged { get; set; }

			public UISwitch Switch { get; private set; }

			public BooleanMenuElementCell()
				: base(UITableViewCellStyle.Subtitle, Key)
			{
				Add(ImageView = new UIImageView(new CGRect(0, 0, ImageWidth, ImageWidth)) {
					ContentMode = UIViewContentMode.Center,
				});
				this.AccessoryView = Switch = new UISwitch() {
					Tag = 1,
				};
				Switch.ValueChanged += (sender, args) => {
					ValueChanged?.Invoke();
				};
				BackgroundColor = UIColor.Clear;
				//sw.TintColor =Style.Current.Colors.Orange.Value;
				//sw.TintColor = UIColor.FromPatternImage(Images.SwitchOffBackground.Value);
				//sw.OnTintColor = UIColor.FromPatternImage(Images.SwitchOnBackground.Value);
				//sw.ThumbTintColor = Style.Current.Equalizer.SwitchOnThumbColor.Value;
				//sw.BackgroundColor = UIColor.DarkGray;
				//sw.Layer.CornerRadius = 16;

			}

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

				var bounds = ContentView.Bounds;

				var frame = bounds;
				frame.Width = !ForceImage ? 0 : ImageWidth + TwicePadding;
				var leftPadding = this.GetSafeArea().Left;
				var center = frame.GetCenter();
				frame.Width += leftPadding;
				center.X += leftPadding;
				ImageView.Center = center;

				var x = frame.Right;
				frame = bounds;
				frame.Width -= x;
				frame.X = x;
				ContentView.Frame = frame;

			}
		}
	}
}
