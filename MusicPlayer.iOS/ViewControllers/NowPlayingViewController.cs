using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using SDWebImage;
using MediaPlayer;
using MusicPlayer.Data;
using MusicPlayer.iOS.UI;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;

namespace MusicPlayer.iOS.ViewControllers
{
	internal class NowPlayingViewController : UIViewController
	{
		public static NowPlayingViewController Current { get; private set; }
		public Action Close { get; set; }
		public const float TopBarHeight = 54f;
		public const float LandscapeTopBarHeight = 44f;
		public const float MinVideoHeight = 100f;
		public const float PlaybackBarHeight = 100;
		public const float AlbumArtWidth = 25;
		public static nfloat CurrentVideoWidth = AlbumArtWidth;
		public static nfloat StatusBarHeight = UIApplication.SharedApplication.StatusBarFrame.Height;

		public NowPlayingViewController()
		{
			Current = this;
			this.View.InsetsLayoutMarginsFromSafeArea = true;
		}
		public nfloat GetHeight()
		{
			return GetCurrentTopHeight();
		}
		public nfloat BottomInset { get; set; }
		
		public nfloat GetCurrentTopHeight()
		{
			if (this.IsLandscape())
				return (Settings.CurrentPlaybackIsVideo ? TopBarHeight : LandscapeTopBarHeight) + BottomInset;
			return Settings.CurrentPlaybackIsVideo ? MinVideoHeight : TopBarHeight + BottomInset;
		}
		public nfloat GetVisibleHeight()
		{
			if (this.IsLandscape ())
				return GetHeight ();
			var offset = GetOffset();
			if (offset > PlaybackBarHeight/3)
				return GetCurrentTopHeight() + PlaybackBarHeight;
			return GetHeight();
		}

		public nfloat GetOffset()
		{
			return View.Frame.Height - View.Frame.Top - GetCurrentTopHeight();
		}

		public nfloat GetHeaderOverhangHeight()
		{
			return GetCurrentTopHeight();
		}

		NowPlayingView view;

		public override void LoadView()
		{
			View = view = new NowPlayingView(this);
			NotificationManager.Shared.CurrentSongChanged += (sender, args) => view.SetCurrentSong(args.Data);
			NotificationManager.Shared.PlaybackStateChanged += (sender, args) => view.SetState(args.Data);
			NotificationManager.Shared.CurrentTrackPositionChanged += (sender, args) => view.SetCurrentTrackPosition(args.Data);
			NotificationManager.Shared.ShuffleChanged += (sender, args) => view.SetShuffleState(args.Data);
			NotificationManager.Shared.RepeatChanged += (sender, args) => view.SetRepeatState(args.Data);
			NotificationManager.Shared.SongDownloadPulsed += Shared_SongDownloadPulsed;
			NotificationManager.Shared.VideoPlaybackChanged += (sender, args) => view.SetVideoState(args.Data);
			NotificationManager.Shared.ToggleFullScreenVideo += (s, a) => ToggleFullScreenVideo();
            view.SetShuffleState(Settings.ShuffleSongs);
			view.SetRepeatState(Settings.RepeatMode);
			NotificationManager.Shared.StyleChanged += (object sender, EventArgs e) =>
			{
				ApplyStyle();
			};
			ApplyStyle();
		}

		public void ApplyStyle()
		{
			view.ApplyStyle();
		}

		FullScreenMovieController fullScreenController = new FullScreenMovieController();
		public void ToggleFullScreenVideo()
		{
			if (Equals(PresentedViewController, fullScreenController))
				HideFullScreenVideo();
			else
				ShowFullScreenVideo();
        }
		public void ShowFullScreenVideo(bool animated = true)
		{
			if (PresentedViewController == fullScreenController)
				return;
			this.PresentViewControllerAsync(fullScreenController, animated);
		}
		public void HideFullScreenVideo(bool animated = true)
		{
			if (PresentedViewController != fullScreenController)
				return;
			fullScreenController.DismissViewControllerAsync(animated);
		}

		void Shared_SongDownloadPulsed(object sender, NotificationManager.SongDowloadEventArgs e)
		{
			if (e.SongId != Settings.CurrentSong)
				return;
			view.UpdateDownloadProgress(e.Percent);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			if (string.IsNullOrWhiteSpace(Settings.CurrentSong))
				return;
			var song = Database.Main.GetObject<Song, TempSong>(Settings.CurrentSong);
			view.SetCurrentSong(song);
		}

		UINavigationController currentPlaylistNavigationController;
		CurrentPlaylistViewController currentPlaylist;

		public void ShowCurrentPlaylist()
		{
			if (currentPlaylist == null)
			{
				currentPlaylist = new CurrentPlaylistViewController();
				currentPlaylistNavigationController = new UINavigationController(currentPlaylist);
				currentPlaylistNavigationController.ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;
			}

			this.PresentViewControllerAsync(currentPlaylistNavigationController, true);
		}

		class NowPlayingView : UIView
		{
			WeakReference parent;

			public NowPlayingViewController Parent
			{
				get { return parent?.Target as NowPlayingViewController; }
				set { parent = new WeakReference(value); }
			}

			SimpleButton playButton;
			BottomView footer;
			SimpleButton closeButton;
			SimpleButton showCurrentPlaylist;
			NowPlayingCollectionView collectionView;


			const float imageWidth = 25f;
			const float buttonWidth = 34f;
			const float padding = 5f;

			public NowPlayingView(NowPlayingViewController parent)
			{
				Parent = parent;
				this.ClipsToBounds = true;
				BackgroundColor = UIColor.Gray;
				collectionView = new NowPlayingCollectionView();
				Add(collectionView.View);
				Parent.AddChildViewController(collectionView);

				var buttonFrame = new CGRect(0, 0, 44, 44);
				Add(closeButton = new SimpleShadowButton()
				{
					Image = Images.GetCloseImage(15).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
					TintColor = Style.DefaultStyle.AccentColor,
					Tapped = (b) => Parent?.Close?.Invoke(),
					Frame = buttonFrame,
				});
				Add(showCurrentPlaylist = new SimpleShadowButton
				{
					Image = Images.GetPlaylistIcon(20).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
					TintColor = Style.DefaultStyle.AccentColor,
					Tapped = (b) => Parent?.ShowCurrentPlaylist(),
					Frame = buttonFrame,
				});

				Add(playButton = new SimpleButton
				{
					Frame = new CGRect(0, 0, buttonWidth, buttonWidth),
					Image = Images.GetBorderedPlaybackButton(buttonWidth),
					AccessibilityIdentifier = "Play",
					Tapped = (b) => { PlaybackManager.Shared.PlayPause(); }
				}.StylePlaybackControl());
				Add(footer = new BottomView());
			}

			public override CGRect Frame
			{
				get { return base.Frame; }
				set
				{
					if (base.Frame == value)
						return;
					base.Frame = value;
					this.SetNeedsLayout();
				}
			}

			public void ApplyStyle()
			{
				footer.ApplyStyle();
				collectionView.CollectionView.ReloadData();
			}
			
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				var bounds = Bounds;
				nfloat bottomOffset = 0;
				nfloat topOffset = 0;
				if (Device.IsIos11)
				{
					bottomOffset = this.SafeAreaInsets.Bottom;
					topOffset = this.SafeAreaInsets.Top;
				}
				else
				{
					bottomOffset = this.LayoutMargins.Bottom;
					topOffset = this.LayoutMargins.Top;
				}
				footer.BottomOffset = bottomOffset;
				var frame = bounds;
				var frameH = bounds.Height;
				var topHeight = Parent.GetCurrentTopHeight() ;
				frame.Height = topHeight;
				var screenY = Frame.Y;
				var topBarBottom = frame.Bottom;
				var playbackTop = frameH - screenY - topBarBottom;
				frame.X = bounds.Width - buttonWidth - (padding*2);
				frame.Width = buttonWidth + padding;

				playButton.Center = frame.GetCenter();
				
				
				var maxWidth = NMath.Min(bounds.Width, 512);
				if (bounds.Width > bounds.Height)
					maxWidth = NMath.Min (bounds.Height - topHeight, 512);
				frame = new CGRect((bounds.Width - maxWidth)/2, topHeight, maxWidth, maxWidth);
				var y = frame.Bottom;

				frame = closeButton.Frame;
				frame.X = padding;
				frame.Y = topHeight + topOffset;
				closeButton.Frame = frame;

				frame.X = bounds.Width - frame.Width - padding;
				showCurrentPlaylist.Frame = frame;


				collectionView.View.Frame = new CGRect(0, 0, bounds.Width, bounds.Height);


				frame = bounds;
				frame.Height -= y + StatusBarHeight;

				var top = NMath.Max(topBarBottom, playbackTop - frame.Height);

				var contentHeight = bounds.Height - frame.Height - topHeight;
				var availableHeight = top - topBarBottom;

				var percent = availableHeight / contentHeight;
				//var percent = (playbackTop - topHeight) / topHeight;
				//if (percent > .9f)
				//	percent = 100;
				collectionView.SetVisiblePercent(percent);

			//	Console.WriteLine($"Now Playing Screen - Content Height: {contentHeight}   - AvailableHeight: {availableHeight}");

				var alpha = (playbackTop - topHeight) / topHeight;
				footer.SetSliderAlpha (alpha);
				if (bounds.Width < bounds.Height) {
					//portrait layout
                                  					//Bar stays below top content
					footer.MaxHeight = frame.Height;
					frame.Height = NMath.Min (frame.Height, frameH - screenY - topHeight);
					frame.Height = NMath.Max (frame.Height, PlaybackBarHeight + bottomOffset);
					frame.Height += bottomOffset;
					frame.Y = top- bottomOffset;;
					footer.IsLandscape = false;
					footer.Frame = frame;
				} else {
					//Landscape
					//Bottom view is on the side
					//with slider on the bottom
					const float landscapeArtPadding = 25f;
					maxWidth = NMath.Min(bounds.Height -  landscapeArtPadding - topHeight, 512);
					frame.Height = contentHeight;
					frame.X = 0;
					frame.Width = bounds.Width;
					frame.Y = topHeight;
					footer.ContentStart = maxWidth + landscapeArtPadding;
					footer.IsLandscape = true;
					footer.Frame = frame;

				}
			}

			public void SetCurrentSong(Song song)
			{
				footer.SetCurrentSong(song);
			}

			public void SetState(PlaybackState state)
			{
				switch (state)
				{
					case PlaybackState.Stopped:
					case PlaybackState.Paused:
						playButton.Image = Images.GetBorderedPlaybackButton(buttonWidth);
						playButton.AccessibilityIdentifier = "Play";
						break;
					case PlaybackState.Playing:
					case PlaybackState.Buffering:
						playButton.Image = Images.GetBorderedPauseButton(buttonWidth);
						playButton.AccessibilityIdentifier = "Pause";
						break;
				}
				footer.SetState(state);
			}


			public void SetShuffleState(bool data)
			{
				footer.SetShuffleState(data);
			}

			public void SetRepeatState(RepeatMode data)
			{
				footer.SetRepeatState(data);
			}

			public void SetCurrentTrackPosition(TrackPosition data)
			{
				footer.SetCurrentTrackPosition(data);
			}

			internal void UpdateDownloadProgress(float percent)
			{
				footer.UpdateDownloadProgress(percent);
			}

			public void SetVideoState(bool isVideo)
			{
				//videoView.Hidden = !isVideo;
			}
			class BottomView : UIView
			{
				ProgressView slider;
				UILabel timeLabel;
				UILabel remainingTimeLabel;
				TwoLabelView labelView;
				SimpleButton thumbsDownButton;
				SimpleButton thumbsUpButton;
				SimpleButton previousButton;
				SimpleButton playButton;
				SimpleButton nextButton;
				MPVolumeView volumeView;
				SimpleButton shareButton;
				SimpleButton shuffleButton;
				SimpleButton repeatButton;
				SimpleButton menuButton;

				BluredView backgroundBluredView;
				const float playButtonSize = 30f;
				const float nextbuttonSize = 25;
				public nfloat MaxHeight {get;set;}

				public BottomView()
				{
					Add(backgroundBluredView = new BluredView());

					Add(slider = new ProgressView());
					Add(timeLabel = new UILabel {Text = "0000:00",AccessibilityIdentifier = "CurrentTime"}.StyleAsSubText());
					Add(remainingTimeLabel = new UILabel {Text = "0000:00",AccessibilityIdentifier = "RemainingTime", TextAlignment = UITextAlignment.Right}.StyleAsSubText());
					timeLabel.SizeToFit();
					remainingTimeLabel.SizeToFit();

					Add(labelView = new TwoLabelView
					{
						TopLabel = {TextAlignment = UITextAlignment.Center},
						BottomLabel = {TextAlignment = UITextAlignment.Center},
					});
					labelView.TopLabel.StylePlaybackControl();
					labelView.BottomLabel.StylePlaybackControl();

					var buttonFrame = new CGRect(0, 0, 44, 44);
					Add(thumbsDownButton = new SimpleButton
					{
						Image = Images.GetThumbsDownImage(25).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
						Frame = buttonFrame,
						AccessibilityIdentifier = "ThumbsDown",
						TintColor = UIColor.Black,
						Tapped = async (b) =>
						{
							var song = MusicManager.Shared.GetCurrentSong();
							if(song.Rating != 1)
								await MusicManager.Shared.ThumbsDown(song);
							else
							{
								await MusicManager.Shared.Unrate(song);
							}
							SetThumbsState(song);
						}
					});
					Add(thumbsUpButton = new SimpleButton
					{
						Image = Images.GetThumbsUpImage(25).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
						Frame = buttonFrame,
						AccessibilityIdentifier = "ThumbsUp",
						TintColor = UIColor.Black,
						Tapped = async (b) =>
						{
							var song = MusicManager.Shared.GetCurrentSong();
							if (song.Rating != 5)
								await MusicManager.Shared.ThumbsUp(song);
							else
								await MusicManager.Shared.Unrate(song);
							SetThumbsState(song);
						}
					});
					Add(previousButton = new SimpleButton
					{
						Image = Images.GetPreviousButton(nextbuttonSize),
						Frame = buttonFrame,
						AccessibilityIdentifier = "Previous",
						Tapped = button => PlaybackManager.Shared.Previous(),
					});
					Add(playButton = new SimpleButton
					{
						Image = Images.GetPlaybackButton(playButtonSize),
						Frame = buttonFrame,
						AccessibilityIdentifier = "Play",
						TintColor = UIColor.Black,
						Tapped = (button) => PlaybackManager.Shared.PlayPause()
					});
					Add(nextButton = new SimpleButton
					{
						Image = Images.GetNextButton(nextbuttonSize),
						Frame = buttonFrame,
						AccessibilityIdentifier = "Next",
						Tapped = (button) => PlaybackManager.Shared.NextTrack()
					});
					Add(volumeView = new MPVolumeView());
					volumeView.SetRouteButtonImage(Images.GetAirplayButton(20), UIControlState.Normal);
					volumeView.TintColor = Style.DefaultStyle.AccentColor;
					Add(shareButton = new SimpleButton
					{
						Image = Images.GetShareIcon(18).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
						Frame = buttonFrame,
						AccessibilityIdentifier = "Share",
						Tapped = (b) => ShareSong(),
						Enabled = false,
					});
					Add(shuffleButton = new SimpleButton
					{
						Image = Images.GetShuffleImage(18).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
						Frame = buttonFrame,
						AccessibilityIdentifier = "Shuffle",
						Tapped = (button) => PlaybackManager.Shared.ToggleRandom(),
					});
					Add(repeatButton = new SimpleButton
					{
						Image = Images.GetRepeatImage(18).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
						Frame = buttonFrame,
						AccessibilityIdentifier = "Repeat",
						Tapped = (button) => PlaybackManager.Shared.ToggleRepeat(),
					});
					Add(menuButton = new SimpleButton
					{
						Image = Images.DisclosureImage.Value.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
						Frame = buttonFrame,
						AccessibilityIdentifier = "More",
						TintColor = UIColor.Black,
						Tapped = (b) => { PopupManager.Shared.ShowNowPlaying(b); }
					});
				}

				public void ApplyStyle()
				{
					timeLabel.StyleAsSubText();
					remainingTimeLabel.StyleAsSubText();
					labelView.ApplyStyle();
					backgroundBluredView.StyleBlurView();

					previousButton.StyleNowPlayingButtons();
					nextButton.StyleNowPlayingButtons();
					menuButton.StyleNowPlayingButtons();
					shareButton.StyleNowPlayingButtons();
					playButton.StyleNowPlayingButtons();
					SetShuffleState(Settings.ShuffleSongs);
					SetRepeatState(Settings.RepeatMode);
					SetThumbsState(PlaybackManager.Shared.NativePlayer.CurrentSong);
				}
				UIActivityViewController shareController;
				bool isSharing;
				void ShareSong()
				{
					var song = PlaybackManager.Shared.NativePlayer.CurrentSong;
					if (song == null || isSharing)
						return;
					isSharing = true;
					if (shareController != null) {
						shareController.CompletionHandler = null;
						shareController = null;
					}
					var tintColor = UIApplication.SharedApplication.KeyWindow.TintColor;
					UIApplication.SharedApplication.KeyWindow.TintColor = null;
					shareController = new UIActivityViewController (new NSObject[]{new SongSharingActivityProvider(song,false), new SongSharingActivityProvider(song,true)}, null);
					shareController.CompletionHandler = (nsstring, success) => {
						if (tintColor != null)
							UIApplication.SharedApplication.KeyWindow.TintColor = tintColor;
						isSharing = false;
					};
					UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewControllerAsync (shareController, true);
				}
				public void SetCurrentSong(Song song)
				{
					shareButton.Enabled = song != null;
					labelView.TopLabel.Text = song?.Name ?? "";
					var artist = song?.Artist;
					var album = song?.Album;
					var text = string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(album)
						? $"{artist}{album}"
						: $"{artist} - {album}";
					labelView.BottomLabel.Text = text;
					labelView.SetNeedsLayout();
					UpdateDownloadProgress(0);
					SetThumbsState(song);
				}

				public void SetThumbsState(Song song)
				{
					thumbsDownButton.StyleActivatedButton(song?.Rating == 1);
					thumbsUpButton.StyleActivatedButton( song?.Rating == 5);
				}
				public void SetState(PlaybackState state)
				{
					switch (state)
					{
						case PlaybackState.Stopped:
						case PlaybackState.Paused:
							playButton.Image = Images.GetPlaybackButton(playButtonSize);
							playButton.AccessibilityIdentifier = "Play";
							return;
						case PlaybackState.Playing:
						case PlaybackState.Buffering:
							playButton.Image = Images.GetPauseButton(playButtonSize);
							playButton.AccessibilityIdentifier = "Pause";
							return;
					}
				}

				public void SetShuffleState(bool data)
				{
					shuffleButton.StyleActivatedButton(data);
				}

				public void SetRepeatState(RepeatMode data)
				{
					repeatButton.StyleActivatedButton(data != RepeatMode.NoRepeat);
					repeatButton.Image = data == RepeatMode.RepeatOne
						? Images.GetRepeatOneImage(15).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate)
						: Images.GetRepeatImage(15).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
				}

				public void SetCurrentTrackPosition(TrackPosition data)
				{
					timeLabel.Text = data.CurrentTimeString;
					remainingTimeLabel.Text = data.RemainingTimeString;
					slider.SliderProgress = data.Percent;
				}

				const float padding = 5;
				const float smallPadding = 2;
				public bool IsLandscape { get; set; }

				public override void LayoutSubviews ()
				{
					base.LayoutSubviews();
					if (IsLandscape)
						LayoutLandscape ();
					else
						LayoutPortrait ();
				}
				public void LayoutPortrait()
				{
					var bounds = Bounds;
					backgroundBluredView.Frame = bounds.WithHeight(bounds.Height*2);
					slider.SizeToFit();
					var frame = slider.Frame;
					frame.X = frame.Y = 0;
					frame.Y = (frame.Height/2 + 1)*-1;
					frame.Width = bounds.Width;
					slider.Frame = frame;

					var y = frame.Bottom;
					frame = timeLabel.Frame;
					frame.X = padding;
					frame.Y = y;
					timeLabel.Frame = frame;

					frame.X = bounds.Width - frame.Width - padding;
					remainingTimeLabel.Frame = frame;

					y = frame.Bottom;

					var height = Bounds.Height - y;
					var fourth = height/4 - padding;
					y -= smallPadding;

					var offset = MaxHeight - Frame.Height;

					frame = new CGRect(padding, y + offset, bounds.Width - 2*padding, fourth);
					labelView.Frame = frame;

					y = frame.Bottom;
					var frameTop = frame.Top;

					frame.Y = y - offset - frame.Height + padding*2 ;
					if (frame.Bottom > frameTop) {
						frame.Y += frame.Bottom - frameTop;
					}
					var center = frame.GetCenter();
					playButton.Center = center;

					const float centerDiff = 65;
					center.X -= centerDiff;
					previousButton.Center = center;
					center.X -= centerDiff;

					thumbsDownButton.Center = center;

					center = playButton.Center;
					center.X += centerDiff;

					nextButton.Center = center;
					center.X += centerDiff;

					thumbsUpButton.Center = center;

					frame.Y += fourth + padding;

					center = frame.GetCenter();

					y = frame.Bottom;

					//Volume views have sizing issues.  Let them choose their own heights
					volumeView.SizeToFit();
					frame.Height = volumeView.Frame.Height;
					volumeView.Frame = frame;
					volumeView.Center = center;

					frame.Height = fourth;
					frame.Y = y;



					center = frame.GetCenter();
					var x = center.X = 20;
					var right = bounds.Width - 20;
					var spacing = (right - x)/3;

					shareButton.Center = center;
					center.X += spacing;

					shuffleButton.Center = center;

					center.X += spacing;
					repeatButton.Center = center;

					center.X = right;
					menuButton.Center = center;
				}
				public nfloat ContentStart { get; set; }
				public nfloat BottomOffset { get; set; }

				void LayoutLandscape ()
				{
					var bounds = Bounds;
					bounds.Height -= BottomOffset;
					slider.SizeToFit();
					var frame = slider.Frame;
					frame.X = frame.Y = 0;
					frame.Y = (frame.Height/2 + 1)*-1;
					frame.Width = bounds.Width;
					slider.Frame = frame;

					var y = frame.Bottom - 10;
					frame = timeLabel.Frame;
					frame.X = padding;
					frame.Y = y;
					timeLabel.Frame = frame;

					frame.X = bounds.Width - frame.Width - padding;
					remainingTimeLabel.Frame = frame;

					y = frame.Bottom;

					var height = Bounds.Height - y;
					var fourth = height/4 - padding;
					y -= smallPadding;
					var contentWidth = bounds.Width - ContentStart;
					backgroundBluredView.Frame = CGRect.Empty;
					frame = new CGRect(ContentStart + padding, y, contentWidth - 2*padding, fourth);
					labelView.Frame = frame;

					frame.Y = frame.Bottom + padding + padding;

					var center = frame.GetCenter();
					playButton.Center = center;

					const float centerDiff = 65;
					center.X -= centerDiff;
					previousButton.Center = center;
					center.X -= centerDiff;

					thumbsDownButton.Center = center;

					center = playButton.Center;
					center.X += centerDiff;

					nextButton.Center = center;
					center.X += centerDiff;

					thumbsUpButton.Center = center;

					frame.Y += fourth + padding*2;

					center = frame.GetCenter();

					y = frame.Bottom + padding;

					//Volume views have sizing issues.  Let them choose their own heights
					volumeView.SizeToFit();
					frame.Height = volumeView.Frame.Height;
					volumeView.Frame = frame;
					volumeView.Center = center;

					frame.Height = fourth;
					frame.Y = y - 44;


					center = frame.GetCenter();
					var x = center.X = ContentStart;
					var right = bounds.Width - 20;
					var spacing = (right - x)/3;

					shareButton.Center = center;
					center.X += spacing;

					shuffleButton.Center = center;

					center.X += spacing;
					repeatButton.Center = center;

					center.X = right;
					menuButton.Center = center;
				}

				internal void UpdateDownloadProgress(float percent)
				{
					slider.DownloadProgress = percent;
				}

				internal void SetSliderAlpha(nfloat alpha)
				{
					slider.SetAlpha(alpha);
				}
			}
		}
	}
}