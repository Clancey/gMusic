using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using MonoTouch.Dialog;
using NGraphics;
using UIKit;
using MusicPlayer.Data;
using MusicPlayer.iOS;

namespace MusicPlayer.Cells
{
	internal class MenuElement : StyledStringElement, IElementSizing
	{
		public bool SaveIndex {get;set; } = true;

		public MenuElement(string name, Action tapped) : base(name,tapped)
		{
			Init();
		}

		public MenuElement(string name, string svg) : this(name,svg,28)
		{

		}
		public MenuElement(string name, string svg, double size) : base(name)
		{
			Image = svg.LoadImageFromSvg(new Size(size, size), UIImageRenderingMode.AlwaysTemplate);
			Init();
		}

		void Init()
		{
			ShouldDeselect = true;
			BackgroundColor = UIColor.Clear;
			style = UITableViewCellStyle.Value1;
		}

		public float Height { get; set; } = 55f;
        public nfloat GetHeight(UITableView tableView, NSIndexPath indexPath)
		{
			return Height;
		}


		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(MenuElementCell.Key) as MenuElementCell ?? new MenuElementCell(style);
			cell.ImageView.Image = Image;
			cell.TextLabel.Text = Caption;
			if(cell.DetailTextLabel != null) {
				cell.DetailTextLabel.Text = Value?? "";
			}
			ApplyStyle(cell);
			return cell;
		}

		protected virtual void ApplyStyle(MenuElementCell cell)
		{
			cell.StyleAsMenuElement();
		}
		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			base.Selected(dvc, tableView, indexPath);
			if(SaveIndex)
				Settings.CurrentMenuIndex = IndexPath.Row;
		}

		public class MenuElementCell : UITableViewCell
		{
			public const string Key = "MenuElementCell";
            protected const float Padding = 5f;
			protected const float TwicePadding = Padding * 2;
			protected const float ImageWidth = 35f;
			public new UIImageView ImageView { get; set; }

			public bool ForceImage {get;set; } = true;

			public MenuElementCell(UITableViewCellStyle style) : base(style, Key)
			{
				Add(ImageView = new UIImageView(new CGRect(0, 0, ImageWidth, ImageWidth))
				{
					ContentMode = UIViewContentMode.Center,
				});
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