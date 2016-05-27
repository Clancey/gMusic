using System;
using System.Collections.Generic;
using System.Text;
using MonoTouch.Dialog;
using UIKit;

namespace MusicPlayer.iOS
{
	class MenuHeaderElement : StringElement
	{
		public MenuHeaderElement(string caption) : base(caption)
		{
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = base.GetCell(tv);
			cell.StyleAsMenuHeaderElement();
			return cell;
		}
	}
}
