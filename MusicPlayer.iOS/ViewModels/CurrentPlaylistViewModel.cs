using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using MusicPlayer.Managers;
using UIKit;

namespace MusicPlayer.ViewModels
{
	//IOS overrides
	partial class CurrentPlaylistViewModel
	{
		public override nfloat GetHeightForHeader(UIKit.UITableView tableView, nint section)
		{
			return 0;
		}

		public override void RowSelected(UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			//base.RowSelected(tableView, indexPath);
			PlaybackManager.Shared.PlaySongAtIndex(indexPath.Row);
			tableView.DeselectRow(indexPath, true);
		}

		public override void MoveRow(UIKit.UITableView tableView, Foundation.NSIndexPath sourceIndexPath,
			Foundation.NSIndexPath destinationIndexPath)
		{
			PlaybackManager.Shared.MoveSong(sourceIndexPath.Row, destinationIndexPath.Row);
		}

		public override void CommitEditingStyle(UIKit.UITableView tableView, UIKit.UITableViewCellEditingStyle editingStyle,
			Foundation.NSIndexPath indexPath)
		{
			switch (editingStyle)
			{
				case UITableViewCellEditingStyle.Delete:
					PlaybackManager.Shared.RemoveSong(indexPath.Row);
					tableView.DeleteRows(new NSIndexPath[] {indexPath}, UITableViewRowAnimation.Fade);
					break;
			}
			//base.CommitEditingStyle(tableView, editingStyle, indexPath);
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
			return true;
		}
	}
}