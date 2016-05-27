using System;
using SimpleTables;
using UIKit;

namespace MusicPlayer.Cells
{
	public abstract class BaseCell : IBindingCell
	{
		WeakReference bindingContext;

		public object BindingContext
		{
			get { return bindingContext?.Target ?? null; }
			set { bindingContext = new WeakReference(value); }
		}

		#region ICell implementation

		public abstract UITableViewCell GetCell(UITableView tv);

		#endregion
	}
}