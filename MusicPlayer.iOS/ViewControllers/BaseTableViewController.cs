using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using MusicPlayer.Managers;
using UIKit;

namespace MusicPlayer.iOS.ViewControllers
{
	public abstract class BaseTableViewController : UITableViewController
	{
		public bool DisablePullToRefresh { get; set; }
		public BaseTableViewController()
		{
		}

		protected UIBarButtonItem menuButton;
		public override void LoadView()
		{
			base.LoadView();
			var style = View.GetStyle();
			View.TintColor = style.AccentColor;
			TableView.TableFooterView = new UIView(new CGRect(0, 0, 320, NowPlayingViewController.MinVideoHeight));
			TableView.SectionIndexMinimumDisplayRowCount = 30;
			TableView.SectionIndexBackgroundColor = UIColor.Clear;
			if (Device.IsIos9)
				TableView.CellLayoutMarginsFollowReadableWidth = false;

			if (NavigationController == null)
				return;
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

			//if(Device.IsIos8)
			//	NavigationController.HidesBarsOnSwipe = true;
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			if (ShouldHideMenu)
				return;
			NavigationItem.LeftBarButtonItem = BaseViewController.ShouldShowMenuButton(this) ? menuButton : null;
		}

		public bool ShouldHideMenu { get; set; }

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			SetupEvents();
			SetupRefresh();
			this.StyleViewController();
			TableView.ReloadData();
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			TeardownEvents();
		}

		public virtual void SetupEvents()
		{
			NotificationManager.Shared.SongDatabaseUpdated += SongDatabaseUpdated;
		}

		void SongDatabaseUpdated(object sender, EventArgs eventArgs)
		{
			TableView.ReloadData();
		}

		public virtual void TeardownEvents()
		{
			NotificationManager.Shared.SongDatabaseUpdated -= SongDatabaseUpdated;
			TearDownRefresh();
		}


		void SetupRefresh()
		{
			if (DisablePullToRefresh)
				return;
			if(RefreshControl == null)
				RefreshControl = new UIRefreshControl();
			RefreshControl.ValueChanged += RefreshControl_ValueChanged;;
		}

		async void RefreshControl_ValueChanged(object sender, EventArgs e)
		{
			if (await Refresh())
				RefreshControl.AttributedTitle = new NSAttributedString(String.Format("Last Updated" + ":{0:g}", DateTime.Now));
			RefreshControl.EndRefreshing();
			TableView.ReloadData ();
		}

		void TearDownRefresh()
		{
			if(RefreshControl != null)
				RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
		}
		public virtual async Task<bool> Refresh()
		{
			try
			{
				await ApiManager.Shared.StartSync();
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				return false;
			}
		}
	}
}