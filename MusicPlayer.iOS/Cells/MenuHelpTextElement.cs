using System;
using MonoTouch.Dialog;
using UIKit;

namespace MusicPlayer.iOS
{
	public class MenuHelpTextElement : StyledStringElement, IElementSizing
	{
		public MenuHelpTextElement (string text) : base(text)
		{
			Lines = 0;
			BackgroundColor = UIColor.Clear;
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = base.GetCell (tv);
			var style = tv.GetStyle();
			cell.BackgroundColor = UIColor.Clear;
			cell.TextLabel.TextColor = style.SubTextColor;
			cell.TextLabel.Font = style.SubTextFont;
			cell.TextLabel.Lines = 0;
			cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;

			return cell;
		}

		#region IElementSizing implementation
		public nfloat GetHeight (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var tvsize = tableView.Bounds.Size;
			tvsize.Width -= 100;
			var style = tableView.GetStyle();
			var size = this.Caption.StringSize (style.SubTextFont,tvsize);
			return size.Height + 30;
		}
		#endregion
	}
}

