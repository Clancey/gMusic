using System;
using AppKit;

using SimpleTables;
using CoreGraphics;

namespace MusicPlayer
{
	public class BaseTableView<TVM,T> : NSView where TVM : TableViewModel<T> where T: class
	{
		public readonly NSTableView TableView;
		NSScrollView tableViewContainer;
		TVM model;
		public TVM Model {
			get {
				return model;
			}
			set {
				model = value;
				ModelChanged ();
			}
		}
		public BaseTableView ()
		{
			TableView = new NSTableView (new CGRect(0,0,500,500));
			TableView.SizeLastColumnToFit ();
			TableView.UsesAlternatingRowBackgroundColors = true;

			AddSubview(tableViewContainer = new NSScrollView(new CGRect(0,0,500,500)));
			tableViewContainer.DocumentView = TableView;
			tableViewContainer.HasVerticalScroller = true;
		}
		public override void ResizeSubviewsWithOldSize (CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			tableViewContainer.Frame = TableView.Frame = Bounds;
		}
	

		public override void ViewWillMoveToSuperview (NSView newSuperview)
		{
			base.ViewWillMoveToSuperview (newSuperview);

			TableView.Source = Model;
			TableView.ReloadData ();
		}

		protected virtual void ModelChanged()
		{
			if (this.Superview != null) {
				TableView.Source = Model;
				TableView.ReloadData ();
			}
		}

	}
}

