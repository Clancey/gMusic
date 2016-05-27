using CoreGraphics;
using Foundation;
using MonoTouch.Dialog;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace MusicPlayer.iOS
{
	class MenuSection : Section, IElementSizing
	{
		public MenuSection(string title, UIColor textColor) : this(title)
		{
			(HeaderView as UILabel).TextColor = textColor;
		}
		public MenuSection(string title) : base(title)
		{
			HeaderView = new UILabel(new CGRect(25, 0, 320, 50))
			{
				Font = Style.DefaultStyle.HeaderTextThinFont,
				TextColor = Style.DefaultStyle.HeaderTextColor,
				Text = string.Format("  {0}", title),
				BackgroundColor =  UIColor.DarkGray.ColorWithAlpha(.1f),
			};
		}


		#region IElementSizing implementation

		public nfloat GetHeight(UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return 75;
		}

		#endregion
	}
	public class MenuSectionElement : StyledStringElement, IElementSizing
	{
		public MenuSectionElement(string name) : base(name)
		{
			init();

		}
		void init()
		{
			ShouldDeselect = true;
			Font = Style.DefaultStyle.HeaderTextThinFont;
			TextColor = Style.DefaultStyle.HeaderTextColor;
			BackgroundColor = UIColor.Clear;
		}

		public nfloat GetHeight(UITableView tableview, NSIndexPath path)
		{
			return 75f;
		}
		public override UITableViewCell GetCell(UITableView tv)
		{
			var style = tv.GetStyle();
			TextColor = style.HeaderTextColor;
			Font = style.HeaderTextThinFont;
			var cell = base.GetCell(tv);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			//cell.SelectedBackgroundView = new UIView();
			//cell.TintColor =style.Navigation.IconTintColor.Value;
			return cell;
		}
		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			base.Selected(dvc, tableView, indexPath);
		}
	}

}

