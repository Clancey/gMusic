using System;
using UIKit;
using MusicPlayer.iOS;

namespace MusicPlayer.ViewModels
{
	partial class BaseViewModel<T>
	{
		//public BaseViewModel()
		//{
		//	UITableViewHeaderFooterView
		//}
		public override void WillDisplayHeaderView(UIKit.UITableView tableView, UIKit.UIView headerView, nint section)
		{
			//base.WillDisplayHeaderView(tableView, headerView, section);
			var view = headerView as UITableViewHeaderFooterView;
			view.StyleSectionHeader();
		}

	}
}

