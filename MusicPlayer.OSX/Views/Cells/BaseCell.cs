using System;
using SimpleTables;
using Foundation;
using AppKit;

namespace MusicPlayer
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
		public abstract NSView GetCell (NSTableView tableView, NSTableColumn tableColumn, NSObject owner);
		public abstract string GetCellText (NSTableColumn tableColumn);
		#endregion

	}
}

