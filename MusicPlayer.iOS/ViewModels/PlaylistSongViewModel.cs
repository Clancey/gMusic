using System;
using UIKit;
using MusicPlayer.Managers;
using Foundation;

namespace MusicPlayer.ViewModels
{
	partial class PlaylistSongViewModel
	{
		public override void MoveRow(UIKit.UITableView tableView, Foundation.NSIndexPath sourceIndexPath,
			Foundation.NSIndexPath destinationIndexPath)
		{
			bool goingUp = sourceIndexPath.Row < destinationIndexPath.Row;
			var row = destinationIndexPath.Row;
			string prevId = "";
			string nextId = "";
			if (goingUp)
			{
				prevId = ItemFor(0, row).Id;
				nextId = RowsInSection(0) == row + 1 ? "" : ItemFor(0, row + 1).Id;
			}
			else
			{
				prevId = row <= 0 ? "" : ItemFor(0, row - 1).Id;
				nextId = ItemFor(0, row).Id;
			}
			//Console.WriteLine(prevId + " - " + nextId);
			MoveSong(ItemFor(0, sourceIndexPath.Row), prevId, nextId, destinationIndexPath.Row + 1);
		}

		public override async void CommitEditingStyle(UIKit.UITableView tableView,
			UIKit.UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
		{
			switch (editingStyle)
			{
				case UITableViewCellEditingStyle.Delete:
					var item = ItemFor(indexPath.Section, indexPath.Row);
					await DeleteSong(item);
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