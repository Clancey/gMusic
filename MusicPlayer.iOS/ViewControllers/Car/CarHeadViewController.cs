using System;
using CoreGraphics;
using UIKit;
using System.Collections.Generic;
using MusicPlayer.iOS.ViewControllers;
using FlyoutNavigation;
using MonoTouch.Dialog;
using MusicPlayer.Cells;
using MusicPlayer.Data;
using System.Linq;
using MusicPlayer.Managers;

namespace MusicPlayer.iOS.Car
{
	public class CarHeadViewController : UIViewController
	{

		public CarHeadViewController()
		{
		}
		CarHeadView view;
		public override void LoadView()
		{
			View = view = new CarHeadView(this);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NotificationManager.Shared.ToggleMenu += Shared_ToggleMenu;

		}
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			NotificationManager.Shared.ToggleMenu -= Shared_ToggleMenu;
		}
		void Shared_ToggleMenu (object sender, EventArgs e)
		{
			view.Menu.ToggleMenu ();
		}
		public class CarHeadView : UIView
		{
			WeakReference _parent;
			
			public CarHeadViewController Parent {
				get { return _parent.Target as CarHeadViewController;}
				set { _parent = new WeakReference (value); }
			}
			public FlyoutNavigationController Menu;

			UIButton nowPlayingButton;

			Dictionary<TabButton, UIViewController> viewControllers = new Dictionary<TabButton, UIViewController>();

			public CarHeadView(CarHeadViewController parent)
			{
				Parent = parent;
				this.BackgroundColor = UIColor.FromRGB(64, 64, 64);
				nowPlayingButton = new UIButton(new CGRect(0, 0, 175, 80));
				nowPlayingButton.TintColor = Style.DefaultStyle.AccentColor;
				nowPlayingButton.SetTitleColor(Style.DefaultStyle.AccentColor, UIControlState.Normal);
				//nowPlayingButton.TouchUpInside += (sender, e) => ShowNowPlaying ();
				nowPlayingButton.TitleLabel.TextAlignment = UITextAlignment.Right;
				nowPlayingButton.TitleLabel.Lines = 2;
				nowPlayingButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
				nowPlayingButton.SetTitle("Now Playing", UIControlState.Normal);

				//viewControllers = new Dictionary<TabButton, UIViewController>
				//{
				//	{ radioButton, new CarNavigation(new CarRadioViewController()) },
				//	{ songsButton, new CarNavigation(new CarSongsViewController() )},
				//	{ playlistButton, new CarNavigation(new CarPlaylistViewController()) },
				//	{ artistsButton, new CarNavigation(new CarArtistViewController()) },
				//};

				var menuItems = new Tuple<Element, UIViewController>[]
				{
					new Tuple<Element, UIViewController>(new MenuElement("Artists", "SVG/artist.svg"), new CarArtistViewController()),
					new Tuple<Element, UIViewController>(new MenuElement("Albums", "SVG/album.svg"), new AlbumViewController()),
					new Tuple<Element, UIViewController>(new MenuElement("Genres", "SVG/genres.svg"), new CarGenreViewController()),
					new Tuple<Element, UIViewController>(new MenuElement("Songs", "SVG/songs.svg"), new CarSongsViewController()),
					new Tuple<Element, UIViewController>(new MenuElement("Playlists", "SVG/playlists.svg"), new CarPlaylistViewController()),
					new Tuple<Element, UIViewController>(new MenuHeaderElement("online"), null),
					new Tuple<Element, UIViewController>(new MenuElement("Radio", "SVG/radio.svg"), new CarRadioViewController()),
					new Tuple<Element, UIViewController>(new MenuHeaderElement("settings"), null),
					new Tuple<Element, UIViewController>(new MenuSwitch("Offline Only", "SVG/offline.svg", Settings.ShowOfflineOnly) {ValueUpdated = (b)=> Settings.ShowOfflineOnly = b}, null),
					new Tuple<Element, UIViewController>(new MenuSubtextSwitch("Equalizer"
					                                                       , MusicPlayer.Playback.Equalizer.Shared.CurrentPreset?.Name , "SVG/equalizer.svg", Settings.EqualizerEnabled){ValueUpdated = (b) => MusicPlayer.Playback.Equalizer.Shared.Active = b},
					                                 new EqualizerViewController()),
				};
				Menu = new FlyoutNavigationController {};
				Menu.NavigationRoot = new RootElement("gMusic")
				{
					new Section
					{
						menuItems.Select(x => x.Item1)
					}
				};

				Menu.NavigationRoot.TableView.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("launchBg"));
				Menu.NavigationRoot.TableView.BackgroundView = new BluredView(UIBlurEffectStyle.Light);
				Menu.NavigationRoot.TableView.SeparatorColor = UIColor.Clear;
				Menu.ViewControllers = menuItems.Select(x => x.Item2 == null ? null : new CarNavigation(x.Item2)).ToArray();
				Menu.HideShadow = false;
				Menu.ShadowViewColor = UIColor.Gray.ColorWithAlpha(.25f);
				Menu.SelectedIndex = 3;
				AddSubview(Menu.View);
				parent.AddChildViewController(Menu);
			}


			void SelectedItemChanged(TabButton button)
			{
				if (button.Tag == 0)
				{
					//Show more toolbar
					return;
				}
				//SetContent();
			}


			nfloat columnWidth;
			nfloat rowHeight;
			CGSize lastSize;
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				var bounds = Bounds;
				if (lastSize != bounds.Size)
				{
					columnWidth = bounds.Width / 6;
					CarStyle.RowHeight = rowHeight = bounds.Height / 5.5f;
					nowPlayingButton.Font = Fonts.NormalFont(CarStyle.RowHeight * .3f);
				}

				lastSize = bounds.Size;
				Menu.View.Frame = bounds;
				var cellSize = new CGSize(columnWidth, rowHeight);
				//toolbar.Frame = new CGRect(0, 0, bounds.Width - columnWidth, rowHeight);
				//nowPlayingButton.Frame = new CGRect(toolbar.Frame.Right, 0, columnWidth, rowHeight);
				var frame = bounds;
				frame.Y = rowHeight;
				frame.Height -= rowHeight;
				//var currentController = 
				//var tv = currentController as UITableViewController;
				//if (tv != null)
				//	tv.TableView.RowHeight = rowHeight;

			}
		}
	}
}