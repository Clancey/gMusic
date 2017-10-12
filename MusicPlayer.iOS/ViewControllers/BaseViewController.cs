using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Managers;
using UIKit;

namespace MusicPlayer.iOS.ViewControllers
{
	public class BaseViewController : UITableViewController
	{
		public bool ShouldHideMenu { get; set; }

		UIBarButtonItem menuButton;
		public override void LoadView()
		{
			base.LoadView();

			this.StyleViewController();
			if (NavigationController == null)
				return;
			var style = View.GetStyle();
			NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes
			{
				ForegroundColor = style.AccentColor
			};
			if (NavigationController.ViewControllers.Length == 1 && !ShouldHideMenu)
			{
				menuButton = new UIBarButtonItem(Images.MenuImage, UIBarButtonItemStyle.Plain,
					(s, e) => { NotificationManager.Shared.ProcToggleMenu(); })
				{
					AccessibilityIdentifier = "menu",
				};
				NavigationItem.LeftBarButtonItem = BaseViewController.ShouldShowMenuButton(this) ? menuButton : null;
			}

			TableView.EstimatedSectionFooterHeight = 0;
			TableView.EstimatedSectionHeaderHeight = 0;
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			if (ShouldHideMenu)
				return;
			NavigationItem.LeftBarButtonItem = ShouldShowMenuButton(this) ? menuButton : null;
		}

		public static bool ShouldShowMenuButton(UIViewController vc)
		{
			if (vc.NavigationController?.ViewControllers?.Length != 1)
				return false;
			if (UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad)
				return true;
			return(vc.InterfaceOrientation == UIInterfaceOrientation.Portrait || vc.InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown);
		}

	}
}