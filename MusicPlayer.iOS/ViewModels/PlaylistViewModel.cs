using System;
using UIKit;
using Foundation;
using MusicPlayer.Managers;

namespace MusicPlayer.ViewModels
{
	public partial class PlaylistViewModel
	{
		public override async void CommitEditingStyle(UIKit.UITableView tableView,
			UIKit.UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
		{
			switch (editingStyle)
			{
				case UITableViewCellEditingStyle.Delete:
					using (var spinner = new Spinner("Deleting"))
					{
						var item = ItemFor(indexPath.Section, indexPath.Row);
						var success = await MusicManager.Shared.Delete(item);
						tableView.ReloadData();

					}
					break;
			}
		}

		public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return UITableViewCellEditingStyle.Delete;
		}

		public override bool CanEditRow(UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return true;
		}

		public override bool CanMoveRow(UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return false;
		}
	}
}