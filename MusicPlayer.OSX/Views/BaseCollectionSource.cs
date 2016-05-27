using System;
using SimpleTables;

namespace MusicPlayer
{
	public class BaseCollectionSource<T> : CollectionViewSource<T>
	{
		public BaseCollectionSource (TableViewModel<T> model) : base (model)
		{
		}

		public override ICollectionCell GetICollectionCell (int section, int row)
		{
			var item = Model.ItemFor (section, row);
			var cell = item as ICollectionCell;
			if (cell != null)
				return cell;

//			if(item == null)
//				return new StringCell("");
			cell = GetCellFromEvent (item);

			if (cell == null)
				cell = CellRegistrar.GetCollectionCell (item.GetType ());

			if (cell == null)
				cell = base.GetICollectionCell (section, row);

			var binding = cell as IBindingCell;
			if (binding != null)
				binding.BindingContext = item;

			return cell;
		}
		public event Action<Foundation.NSSet> IndexesSelected;
		public override void ItemsSelected (AppKit.NSCollectionView collectionView, Foundation.NSSet indexPaths)
		{
			IndexesSelected?.Invoke (indexPaths);
			base.ItemsSelected (collectionView, indexPaths);
		}
	}
}

