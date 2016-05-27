using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Api;
using MusicPlayer.Managers;
using MusicPlayer.ViewModels;
using System.Linq;
using UIKit;
using Localizations;

namespace MusicPlayer.iOS.ViewControllers
{
	class SearchViewController : UIViewController
	{
		public SearchViewController()
		{
			this.Title = Strings.Search;
			this.AutomaticallyAdjustsScrollViewInsets = false;
		}
		UIBarButtonItem menuButton;
		public SearchViewModel Model;
		public override void LoadView()
		{
			Model = new SearchViewModel();
			View = new SearchView(this);
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
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			this.StyleViewController();
			(View as SearchView).ApplyStyle();
		}

		class SearchView : UIView
		{
			WeakReference parent;

			UISearchBar searchBar;
			public TopTabBarController PanaramBarController { get; set; }
			public SearchViewController Parent
			{
				get { return parent?.Target as SearchViewController; }
				set { parent = new WeakReference(value);}
			}

			public SearchView(SearchViewController parent)
			{
				Parent = parent;
				var style = this.GetStyle();
				searchBar = new UISearchBar
				{
					BarTintColor = UIColor.White,
					TintColor = style.AccentColor,
					AccessibilityIdentifier = "searchBar",
				};

				searchBar.CancelButtonClicked += (sender, args) =>
				{
					var search = (sender as UISearchBar);
					var s = search.Superview as SearchView;
					s.Parent.Model.Search("");
					searchBar.ResignFirstResponder();
				};
				searchBar.SearchButtonClicked += (sender, args) =>
				{
					var search = (sender as UISearchBar);
					var s = search.Superview as SearchView;
					s.Parent.Model.Search(searchBar.Text);
					searchBar.ResignFirstResponder();
				};
				searchBar.TextChanged += (object sender, UISearchBarTextChangedEventArgs e) => {
					var search = (sender as UISearchBar);

					var s = search.Superview as SearchView;
					if (search.Text != "")
					{
						s.Parent.Model.KeyPressed(search.Text);
						return;
					}

					s.Parent.Model.Search("");
					this.BeginInvokeOnMainThread(() => {
						searchBar.EndEditing(true);
						searchBar.ResignFirstResponder();
					});
				};

				PanaramBarController = new TopTabBarController();
				Add(PanaramBarController.View);
				parent.AddChildViewController(PanaramBarController);
				PanaramBarController.ViewControllers = Parent.Model.ViewModels.Select(x => new SearchListViewController {Model = x}).ToArray();

				Add(searchBar);
			}

			public void ApplyStyle()
			{
				var style = this.GetStyle();
				searchBar.BarStyle = style.BarStyle;
				searchBar.BarTintColor = style.SectionBackgroundColor;
			}

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

				searchBar.SizeToFit();
				var bounds = Bounds;

				var frame = searchBar.Frame;
				frame.Width = bounds.Width;
				frame.Y = 64;
				searchBar.Frame = frame;
				PanaramBarController.TopOffset = frame.Bottom;
				PanaramBarController.View.Frame = bounds;


			}
		}
	}
}
