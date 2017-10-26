using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using FlyoutNavigation;
using Foundation;
using MediaPlayer;
using MonoTouch.Dialog;
using MusicPlayer.Data;
using MusicPlayer.Cells;
using MusicPlayer.Managers;
using UIKit;
using SimpleTables;
using Section = MonoTouch.Dialog.Section;
using Localizations;

namespace MusicPlayer.iOS.ViewControllers
{
	internal class RootViewController : UIViewController
	{
		FlyoutNavigationController Menu;
		NowPlayingViewController NowPlaying;

		public RootViewController()
		{
		}

		RootView view;
		Tuple<Element, UIViewController>[] menuItems;
		public override void LoadView()
		{
			View = view = new RootView();
			menuItems = new Tuple<Element, UIViewController>[]
			{
				new Tuple<Element, UIViewController>(new MenuElement(Strings.Search,"SVG/search.svg",20) {SaveIndex = false }, new SearchViewController()),
				new Tuple<Element, UIViewController>(new MenuHeaderElement("my music"), null),
				new Tuple<Element, UIViewController>(new MenuElement(Strings.Artists, "SVG/artist.svg"), new ArtistViewController()),
				new Tuple<Element, UIViewController>(new MenuElement(Strings.Albums, "SVG/album.svg"), new AlbumViewController()),
				new Tuple<Element, UIViewController>(new MenuElement(Strings.Genres, "SVG/genres.svg"), new GenreViewController()),
				new Tuple<Element, UIViewController>(new MenuElement(Strings.Songs, "SVG/songs.svg"), new SongViewController()),
				new Tuple<Element, UIViewController>(new MenuElement(Strings.Playlists, "SVG/playlists.svg"), new PlaylistViewController()),
				new Tuple<Element, UIViewController>(new MenuHeaderElement(Strings.Online), null),
				//new Tuple<Element, UIViewController>(new MenuElement("trending", "SVG/trending.svg"),
				//	new BaseViewController {Title = "Trending", View = {BackgroundColor = UIColor.White}}),
				new Tuple<Element, UIViewController>(new MenuElement(Strings.Radio, "SVG/radio.svg"), new RadioStationViewController()),
				new Tuple<Element, UIViewController>(new MenuHeaderElement(Strings.Settings), null),
				new Tuple<Element, UIViewController>(new MenuSwitch("Offline Only", "SVG/offline.svg", Settings.ShowOfflineOnly) {ValueUpdated = (b)=> Settings.ShowOfflineOnly = b}, null),
				new Tuple<Element, UIViewController>(new MenuSubtextSwitch(Strings.Equalizer
					, MusicPlayer.Playback.Equalizer.Shared.CurrentPreset?.Name , "SVG/equalizer.svg", Settings.EqualizerEnabled){ValueUpdated = (b) => MusicPlayer.Playback.Equalizer.Shared.Active = b},
					new EqualizerViewController()),
				new Tuple<Element, UIViewController>(new MenuElement(Strings.Settings, "SVG/settings.svg"){SaveIndex = false }, new SettingViewController()),
				#if DEBUG || ADHOC

				new Tuple<Element, UIViewController>(new MenuElement("Console", "SVG/settings.svg"){SaveIndex = false }, new ConsoleViewController()),
				#endif
			};
			Menu = new FlyoutNavigationController {};
			Menu.NavigationRoot = new RootElement("gMusic")
			{
				new Section
				{
					menuItems.Select(x => x.Item1)
				}
			};
			Menu.NavigationRoot.TableView.TableFooterView =
				new UIView(new CGRect(0, 0, 320, NowPlayingViewController.MinVideoHeight));
			Menu.NavigationRoot.TableView.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("launchBg"));
			Menu.NavigationRoot.TableView.BackgroundView = new BluredView(Style.DefaultStyle.NavigationBlurStyle);
			Menu.NavigationRoot.TableView.SeparatorColor = UIColor.Clear;
			Menu.NavigationRoot.TableView.EstimatedSectionHeaderHeight = 0;
			Menu.NavigationRoot.TableView.EstimatedSectionFooterHeight = 0;
			Menu.ViewControllers = menuItems.Select(x => x.Item2 == null ? null : new UINavigationController(x.Item2)).ToArray();
			Menu.HideShadow = false;
			Menu.ShadowViewColor = UIColor.Gray.ColorWithAlpha(.25f);
			view.Menu = Menu;
			Menu.SelectedIndex = Settings.CurrentMenuIndex;
			AddChildViewController(Menu);

			NowPlaying = new NowPlayingViewController
			{
				Close = () => view.HideNowPlaying(true, true),
			};
			view.NowPlaying = NowPlaying;
			AddChildViewController(NowPlaying);
			SetupEvents();
		}

		void SetupEvents()
		{
			NotificationManager.Shared.ToggleMenu += SharedOnToggleMenu;
			NotificationManager.Shared.ToggleNowPlaying += ToggleNowPlaying;
			NotificationManager.Shared.GoToAlbum += GoToAlbum;
			NotificationManager.Shared.GoToArtist += GoToArtist;
			NotificationManager.Shared.VideoPlaybackChanged += VideoPlaybackChanged;
		}

		void VideoPlaybackChanged(object sender, EventArgs<bool> eventArgs)
		{
			UIView.AnimateAsync(.2, () => view.LayoutSubviews());
		}
		void GoToArtist(object sender, EventArgs<string> eventArgs)
		{
			var artistControllerItem = menuItems.FirstOrDefault(x => x.Item2 is ArtistViewController);
			var artistViewController = artistControllerItem.Item2 as ArtistViewController;
			var artistIndex = menuItems.IndexOf(artistControllerItem);

			artistViewController.NavigationController.PopToRootViewController(false);
			artistViewController.GoToArtist(eventArgs.Data);
			view?.HideNowPlaying(true);

			Menu.SelectedIndex = artistIndex;
		}

		void GoToAlbum(object sender, EventArgs<string> eventArgs)
		{
			var albumControllerItem = menuItems.FirstOrDefault(x => x.Item2 is AlbumViewController);
			var albumController = albumControllerItem.Item2 as AlbumViewController;
            var albumIndex = menuItems.IndexOf(albumControllerItem);

			albumController.NavigationController.PopToRootViewController(false);
			albumController.GoToAlbum(eventArgs.Data);

			view?.HideNowPlaying(true);
			Menu.SelectedIndex = albumIndex;
		}

		void ToggleNowPlaying(object sender, EventArgs eventArgs)
		{
			view?.ShowNowPlaying(true);
		}

		void SharedOnToggleMenu(object sender, EventArgs eventArgs)
		{
			Menu.ToggleMenu();
		}

		void TearDownEvents()
		{
			NotificationManager.Shared.ToggleMenu -= SharedOnToggleMenu;
			NotificationManager.Shared.ToggleNowPlaying -= ToggleNowPlaying;
			NotificationManager.Shared.GoToAlbum -= GoToAlbum;
			NotificationManager.Shared.GoToArtist -= GoToArtist;
			NotificationManager.Shared.VideoPlaybackChanged -= VideoPlaybackChanged;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			view.WillAppear();
		}
		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			Menu.ViewDidAppear(animated);
			ApiManager.Shared.StartSync();
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			view.WillDisapear();
		}

		public class RootView : UIView
		{
			const float FlickVelocity = 1000f;
			static float NowPlayingGestureTollerance = 50 + 100;
			nfloat startY;


			UIViewController menu;
			NowPlayingViewController nowPlaying;

			public UIViewController Menu
			{
				get { return menu; }
				set
				{
					menu?.RemoveFromParentViewController();
					menu?.View.RemoveFromSuperview();

					menu = value;

					Add(menu.View);
				}
			}

			public override void SafeAreaInsetsDidChange()
			{
				base.SafeAreaInsetsDidChange();
				if (Device.IsIos11)
				{
					NowPlaying.BottomInset = this.SafeAreaInsets.Bottom;
					Console.WriteLine(NowPlaying.BottomInset);
				}
			}

			public NowPlayingViewController NowPlaying
			{
				get { return nowPlaying; }
				set
				{
					nowPlaying?.RemoveFromParentViewController();
					nowPlaying?.View.RemoveFromSuperview();

					nowPlaying = value;

					Add(nowPlaying.View);
				}
			}

			UIPanGestureRecognizer panGesture;

			public void WillAppear()
			{
				NowPlaying.View.AddGestureRecognizer(panGesture = new UIPanGestureRecognizer(Panned)
				{
					ShouldReceiveTouch = (sender, touch) =>
					{
						bool isMovingCell =
							touch.View.ToString().IndexOf("UITableViewCellReorderControl", StringComparison.InvariantCultureIgnoreCase) >
							-1;
						if (isMovingCell || touch.View is UISlider || touch.View is MPVolumeView || isMovingCell ||
							touch.View is ProgressView ||
							touch.View is OBSlider)
							return false;
						return true;
					},
					//ShouldRecognizeSimultaneously = (recognizer, gestureRecognizer) =>
					//{
					//	return true;
					//},
				});
			}

			public void WillDisapear()
			{
				if (panGesture != null)
				{
					NowPlaying.View.RemoveGestureRecognizer(panGesture);
					panGesture = null;
				}
			}


			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				CGRect bounds = Bounds;
				Menu.View.Frame = bounds;
				if (isPanning)
					return;
				if (isHidden)
				{
					HideNowPlaying(false);
					return;
				}
				else
				{
					ShowNowPlaying(false);
				}
				CGRect frame = NowPlaying.View.Frame;
				frame.Size = Bounds.Size;
				frame.Height += NowPlaying.GetHeaderOverhangHeight();
				NowPlaying.View.Frame = frame;
			}

			bool isHidden = true;

			public virtual void HideNowPlaying(bool animated = true, bool completeClose = true)
			{
				isHidden = true;
				if (animated)
					BeginAnimations("hideNowPlaying");
				CGRect frame = Bounds;
				if (frame == CGRect.Empty)
					return;

				frame.Y = frame.Height - (completeClose ? NowPlaying.GetHeight() : NowPlaying.GetVisibleHeight());
				frame.Height += NowPlaying.GetHeaderOverhangHeight();
				NowPlaying.View.Frame = frame;

				if (animated)
					CommitAnimations();
			}

			public virtual void ShowNowPlaying(bool animated = true)
			{
				isHidden = false;
				if (animated)
					BeginAnimations("showNowPlaying");
				CGRect frame = Bounds;
				var overhang = NowPlaying.GetHeaderOverhangHeight();
				frame.Y -= overhang;
				frame.Height += overhang;
				NowPlaying.View.Frame = frame;

				if (animated)
					CommitAnimations();
				//Logger.LogPageView(NowPlaying);
			}

			bool isPanning;

			void Panned(UIPanGestureRecognizer panGesture)
			{
				CGRect frame = NowPlaying.View.Frame;
				nfloat translation = panGesture.TranslationInView(this).Y;
				//Console.WriteLine("Translation: {0}, {1}", translation, NowPlaying.GetOffset());
				if (panGesture.State == UIGestureRecognizerState.Began)
				{
					isPanning = true;
					startY = frame.Y;
					//NowPlaying.BackgroundView = Menu.View;
					//NowPlaying.UpdateBackgound();
				}
				else if (panGesture.State == UIGestureRecognizerState.Changed)
				{
					frame.Y = translation + startY;
					frame.Y = NMath.Min(frame.Height, NMath.Max(frame.Y, NowPlaying.GetHeaderOverhangHeight()*-1));
					NowPlaying.View.Frame = frame;
				}
				else if (panGesture.State == UIGestureRecognizerState.Ended)
				{
					isPanning = false;
					var velocity = panGesture.VelocityInView(this).Y;
					//					Console.WriteLine (velocity);
					var show = (Math.Abs(velocity) > FlickVelocity)
						? (velocity < 0)
						: (translation*-1 > NowPlayingGestureTollerance);
					const float playbackBarHideTollerance = NowPlayingViewController.PlaybackBarHeight*2/3;
					if (show)
						ShowNowPlaying(true);
					else
						HideNowPlaying(true,
							Math.Abs(velocity) > FlickVelocity ||
							(translation > 5 && NowPlaying.GetOffset() - NowPlaying.GetHeaderOverhangHeight() < playbackBarHideTollerance));
				}
			}
		}
	}
}