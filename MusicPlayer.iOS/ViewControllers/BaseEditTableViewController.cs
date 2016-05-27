using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace MusicPlayer.iOS.ViewControllers
{
	public abstract class BaseEditTableViewController : BaseTableViewController
	{
		protected UIBarButtonItem editButton;
		protected UIBarButtonItem doneButton;

		public override void LoadView()
		{
			base.LoadView();
			NavigationItem.RightBarButtonItem =
				editButton = new UIBarButtonItem(UIBarButtonSystemItem.Edit, (sender, args) => toggleEditing());
			doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (sender, args) => toggleEditing());
		}

		void toggleEditing()
		{
			var editing = !TableView.Editing;

			this.NavigationItem.SetRightBarButtonItem(editing ? doneButton : editButton, true);
			TableView.SetEditing(editing, true);
		}
	}
}