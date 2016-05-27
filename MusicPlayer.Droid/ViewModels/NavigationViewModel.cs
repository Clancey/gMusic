using System;
using Android.Content;
using Android.Widget;

namespace MusicPlayer
{
	public class NavigationViewModel : ListViewModel<MenuItem>
	{
		public NavigationViewModel ()
		{

		}
		public override void RowSelected (MenuItem item)
		{
			base.RowSelected (item);
			var tapped = item.Tapped;
			if (tapped != null)
				tapped (item);
		}
		public override SimpleTables.ICell GetICell (int section, int row)
		{
			var cell = base.GetICell (section, row);
			var item = ItemFor (section, row);
			(cell as SimpleTables.Cells.Cell).Caption = item.Title;
			return cell;
		}
	}
}

