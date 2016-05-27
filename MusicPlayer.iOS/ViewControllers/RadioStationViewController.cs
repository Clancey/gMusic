using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using CoreGraphics;
using Localizations;
using MusicPlayer.Managers;
using MusicPlayer.ViewModels;
using UIKit;

namespace MusicPlayer.iOS.ViewControllers
{
	internal class RadioStationViewController : TopTabBarController
	{
		public RadioStationViewController()
		{
			Title = Strings.Radio;
			HeaderHeight = 44;
			this.EdgesForExtendedLayout = UIRectEdge.All;
		}
		UIBarButtonItem menuButton;
		public override void LoadView()
		{
			base.LoadView();
			var style = View.GetStyle();
			SelectedTitleColor = style.AccentColor;
			TitleFont = SelectedTitleFont = style.ButtonTextFont;
			ViewControllers = new UIViewController[]
			{
				new RadioStationTab
				{
					Title = Strings.RecentStations,
					IsIncluded = false,
				},
				new RadioStationTab
				{
					Title = Strings.MyStations,
					IsIncluded = true,
				},
			};

			if (NavigationController == null)
				return;
			NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes
			{
				ForegroundColor = style.AccentColor
			};

			if (NavigationController.ViewControllers.Length != 1) return;

			menuButton = new UIBarButtonItem(Images.MenuImage, UIBarButtonItemStyle.Plain,
				(s, e) => { NotificationManager.Shared.ProcToggleMenu(); })
			{
				AccessibilityIdentifier = "menu",
			};
			NavigationItem.LeftBarButtonItem = BaseViewController.ShouldShowMenuButton(this) ? menuButton : null;
		}
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			NavigationItem.LeftBarButtonItem = BaseViewController.ShouldShowMenuButton(this) ? menuButton : null;
		}

		public class RadioStationTab : BaseEditTableViewController
		{
			public RadioStationTab()
			{
				model = new RadioStationViewModel();
			}

			public bool IsIncluded
			{
				get { return model.IsIncluded; }
				set { model.IsIncluded = value; }
			}

			readonly RadioStationViewModel model;

			public override void LoadView()
			{
				base.LoadView();
				TableView.Source = model;
				TableView.TableFooterView = new UIView(new CGRect(0, 0, 320, 150));
			}


			public override void SetupEvents()
			{
				NotificationManager.Shared.RadioDatabaseUpdated += RadioStationDatabaseUpdated;
			}

			void RadioStationDatabaseUpdated(object sender, EventArgs eventArgs)
			{
				TableView.ReloadData();
			}

			public override void TeardownEvents()
			{
				NotificationManager.Shared.RadioDatabaseUpdated -= RadioStationDatabaseUpdated;
			}
		}
	}
}