using System;

namespace MusicPlayer
{
	public partial class ArtistAlbumsViewModel
	{
		public override nfloat GetRowHeight (AppKit.NSTableView tableView, nint row)
		{
			var cell = GetICell (row) as AlbumDetailsCell;
			return cell.GetHeight ();
		}
	}
}

