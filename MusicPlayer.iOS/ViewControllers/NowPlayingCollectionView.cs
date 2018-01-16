using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using SDWebImage;
using MusicPlayer.Data;
using MusicPlayer.iOS.UI;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;
using SimpleTables;

namespace MusicPlayer.iOS.ViewControllers
{
	internal class NowPlayingCollectionView : UICollectionViewController
	{
		public NowPlayingCollectionView() : base(new UICollectionViewFlowLayout
		{
			ScrollDirection = UICollectionViewScrollDirection.Horizontal,
			MinimumInteritemSpacing = 0,
			MinimumLineSpacing = 0,
			ItemSize = new CGSize(100, 100),
		})
		{
			Init();
		}
		public nfloat TopHeight { get; set; }
		static bool isVisible;
		public override async void ViewDidAppear(bool animated)
		{
			isVisible = true;
			base.ViewDidAppear(animated);
			await Task.Delay(50);
			scrollTocurrentSong();
			await Task.Delay(50);
			if (CollectionView.IndexPathsForVisibleItems.Length == 0)
				return;
			var path = CollectionView.IndexPathsForVisibleItems[0];
			var cell = CollectionView.CellForItem(path) as CurrentSongCollectionViewCell;
			cell?.SetNeedsLayout();
		}

		public override void ViewWillDisappear(bool animated)
		{

			isVisible = false;
			base.ViewWillDisappear(animated);
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			scrollTocurrentSong(false);
		}

		//public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		//{
		//	base.WillRotate(toInterfaceOrientation, duration);
		//	scrollTocurrentSong(false);
		//}

		public NowPlayingCollectionView(NSCoder coder) : base(coder)
		{
			Init();
		}

		protected NowPlayingCollectionView(NSObjectFlag t) : base(t)
		{
			Init();
		}

		protected internal NowPlayingCollectionView(IntPtr handle) : base(handle)
		{
			Init();
		}

		public NowPlayingCollectionView(CGRect frame, UICollectionViewLayout layout) : base(layout)
		{
			Init();
		}

		public override nint NumberOfSections(UICollectionView collectionView)
		{
			return 1;
		}

		public override void CellDisplayingEnded(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
		{
			var songCell = cell as CurrentSongCollectionViewCell;
			songCell?.WillDissapear();
		}

		public override void WillDisplayCell(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
		{

			var songCell = cell as CurrentSongCollectionViewCell;
			songCell?.WillAppear();
		}

		int CurrentCount = 0;

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			CurrentCount = Math.Max(1, PlaybackManager.Shared.CurrentPlaylistSongCount);
			Task.Run(()=>App.RunOnMainThread(()=>scrollTocurrentSong()));
			return CurrentCount;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell =
				(CurrentSongCollectionViewCell) collectionView.DequeueReusableCell(CurrentSongCollectionViewCell.Key, indexPath);
			var song = PlaybackManager.Shared.GetSong(indexPath.Row);
			cell.SetSong(song);
			cell.TopHeight = TopHeight;
			return cell;
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			layout.ItemSize = View.Bounds.Size;
		}
	
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			scrollTocurrentSong(false);
		}

		//[Foundation.ExportAttribute("collectionView:layout:sizeForItemAtIndexPath:")]
		//public virtual CoreGraphics.CGSize GetSizeForItem(UIKit.UICollectionView collectionView,
		//	UIKit.UICollectionViewLayout layout, Foundation.NSIndexPath indexPath)
		//{
		//	return Bounds.Size;
		//}

		UICollectionViewFlowLayout layout;

		void Init()
		{
			CollectionView.PagingEnabled = true;
			CollectionView.RegisterClassForCell(typeof (CurrentSongCollectionViewCell), CurrentSongCollectionViewCell.Key);
			CollectionView.CollectionViewLayout = layout = new UICollectionViewFlowLayout
			{
				ScrollDirection = UICollectionViewScrollDirection.Horizontal,
				MinimumInteritemSpacing = 0,
				MinimumLineSpacing = 0,
				ItemSize = new CGSize(100, 100),
			};
			CollectionView.ScrollsToTop = false;
		}

		public override void LoadView()
		{
			base.LoadView();

			NotificationManager.Shared.CurrentPlaylistChanged += (sender, args) =>
			{
				CollectionView.ReloadData();
				scrollTocurrentSong();
			};
			NotificationManager.Shared.CurrentSongChanged += (sender, args) => scrollTocurrentSong(true);
			scrollTocurrentSong();
		}

		public override void DecelerationEnded(UIScrollView scrollView)
		{
			try
			{
				if (!isVisible)
					return;
				NSIndexPath path;
				if (CollectionView.IndexPathsForVisibleItems.Length > 1)
				{
					var width = CollectionView.Bounds.Width;
					var page = (int)Math.Floor((this.CollectionView.ContentOffset.X - width / 2) / width) + 1;
					path = CollectionView.IndexPathsForVisibleItems.First(x => x.Row == page);
				}
				else
					path = CollectionView.IndexPathsForVisibleItems[0];
				var index = path.Row;
				if (index != PlaybackManager.Shared.CurrentSongIndex)
					PlaybackManager.Shared.PlaySongAtIndex(index);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		void scrollTocurrentSong(bool animated = false)
		{
			var index = PlaybackManager.Shared.CurrentSongIndex;
			if (index < 0 || index >= CurrentCount)
				return;
			try
			{
				CollectionView.ScrollToItem(NSIndexPath.FromRowSection(index, 0),
					UICollectionViewScrollPosition.CenteredHorizontally, animated);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		public void SetVisiblePercent(nfloat percent)
		{
			if (CollectionView.IndexPathsForVisibleItems.Length == 0)
				return;
			var path = CollectionView.IndexPathsForVisibleItems[0];
			var cell = CollectionView.CellForItem(path) as CurrentSongCollectionViewCell;
			cell?.UpdatePercent(percent);
		}
		[Register("CurrentSongCollectionViewCell")]
		class CurrentSongCollectionViewCell : UICollectionViewCell
		{
			public const string Key = "CurrentSongCollectionViewCell";
			BlurredImageView backgroundImageView;
			UIImageView albumArtImageView;
			UIImageView smallArtImageView;
			VideoView videoView;
			public TwoLabelView labelView;
			
			const float buttonWidth = 34f;
			const float padding = 5f;

			const float albumArtWidth = 512;

			public nfloat TopHeight { get; set; }
			[Export("initWithFrame:")]
			public CurrentSongCollectionViewCell(CGRect frame) : base(frame)
			{
				BackgroundColor = UIColor.Gray;
				ContentView.Add(backgroundImageView = new BlurredImageView {Image = Images.GetDefaultAlbumArt(albumArtWidth)}.StyleBlurredImageView());
				ContentView.Add(albumArtImageView = new UIImageView(Images.GetDefaultAlbumArt(albumArtWidth))
				{
					Frame = new CGRect(0,0,albumArtWidth,albumArtWidth),
					ContentMode = UIViewContentMode.ScaleAspectFit,
					Layer =
					{
						BorderColor = UIColor.LightGray.CGColor,
						BorderWidth = .5f,
					},
				});

				ContentView.Add(videoView = new VideoView {Frame = new CGRect(0,0,albumArtWidth,albumArtWidth), Hidden = !Settings.CurrentPlaybackIsVideo });

				ContentView.Add(labelView = new TwoLabelView()
				{
					TopLabel = {TextAlignment = UITextAlignment.Center},
					BottomLabel = {TextAlignment = UITextAlignment.Center},
					AccessibilityIdentifier = "NowPlayingBar",
				});
				labelView.AddGestureRecognizer(
					new UITapGestureRecognizer(() => { NotificationManager.Shared.ProcToggleNowPlaying(); }));
				Add(smallArtImageView = new UIImageView(new CGRect(0, 0, NowPlayingViewController.AlbumArtWidth, NowPlayingViewController.AlbumArtWidth))
				{
					Layer =
					{
						BorderColor = UIColor.LightGray.CGColor,
						BorderWidth = .5f,
					}
				});
				this.ClipsToBounds = true;
			}

			public void WillAppear()
			{
				backgroundImageView.StyleBlurredImageView();
				labelView.ApplyStyle();
				NotificationManager.Shared.VideoPlaybackChanged += SharedOnVideoPlaybackChanged;
				//LayoutSubviews();
			}

			void SharedOnPlaybackStateChanged(object sender, EventArgs<PlaybackState> eventArgs)
			{
				UIView.AnimateAsync(.2, LayoutSubviews);
			}

			void SharedOnVideoPlaybackChanged(object sender, EventArgs<bool> eventArgs)
			{
				UIView.AnimateAsync(.2, LayoutSubviews);
			}
			

			public void WillDissapear()
			{
                NotificationManager.Shared.VideoPlaybackChanged -= SharedOnVideoPlaybackChanged;
			}

			static nfloat currentPercent = 0;
			const float widthRatio = 9f/16f;
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				var bounds = Bounds;
				if (bounds.Width > bounds.Height)
					LayoutLandscape();
				else
					LayoutPortrait();
			}

			void LayoutPortrait()
			{
				var bounds = Bounds;
				bool showVideo = songId == Settings.CurrentSong && Settings.CurrentPlaybackIsVideo;
				var maxWidth = NMath.Max(200, NMath.Min(bounds.Width, 512));
				var topHeight = NowPlayingViewController.Current.GetCurrentTopHeight ();
				var frame = new CGRect((bounds.Width - maxWidth) / 2, topHeight + NowPlayingViewController.StatusBarHeight, maxWidth, maxWidth);
				albumArtImageView.Frame = frame;
				if (TopHeight > 0)
				{
					var top = topHeight + NowPlayingViewController.StatusBarHeight;
					albumArtImageView.Center = new CGPoint(bounds.Width / 2,top + (TopHeight - top  ) / 2);
				}
				albumArtImageView.Alpha = showVideo ? Math.Max(0, 1 - (float)currentPercent) : 1;
				frame = bounds;
				var size = (float)Math.Max(frame.Height, frame.Width);
				size *= 1.35f;
				frame.Width = frame.Height = size;
				backgroundImageView.Frame = frame;
				backgroundImageView.Center = albumArtImageView.Center;

				var videoFrame = new CGRect(padding, padding, 160, 90);
				videoFrame.Width += (bounds.Width - videoFrame.Width) * currentPercent;
				videoFrame.X -= currentPercent * padding;
				videoFrame.Height = videoFrame.Width * widthRatio;
				videoFrame.Y = NMath.Max((topHeight + (videoFrame.Height / 2)) * currentPercent, padding);
				frame.X = padding;
				frame.Height = topHeight;
				var w = showVideo ? 160 : NowPlayingViewController.AlbumArtWidth;
				frame.Width = w + padding;
				smallArtImageView.Center = frame.GetCenter();
				smallArtImageView.Hidden = showVideo;

				videoView.Frame = showVideo ? videoFrame : frame;
				videoView.Hidden = !showVideo;
				if (isVisible)
					videoView.Show();
				frame.X = frame.Right;
				frame.Width = bounds.Width - frame.X - buttonWidth - (padding * 2);
				labelView.Frame = frame;
			}

			const float landscapeArtPadding = 25f;
			void LayoutLandscape()
			{
				var bounds = Bounds;
				bool showVideo = songId == Settings.CurrentSong && Settings.CurrentPlaybackIsVideo;
				//Console.WriteLine($"{showVideo} {songId} - {Settings.CurrentSong} - {Settings.CurrentPlaybackIsVideo}");
				var topHeight =  NowPlayingViewController.Current.GetCurrentTopHeight ();
				var maxWidth = NMath.Min(bounds.Height -  landscapeArtPadding *2 - topHeight, 512);
				var frame = new CGRect(landscapeArtPadding, topHeight + landscapeArtPadding, maxWidth, maxWidth);
				albumArtImageView.Frame = frame;

				albumArtImageView.Alpha = showVideo ? Math.Max(0, 1 - (float)currentPercent) : 1;
				frame = bounds;
				var size = (float)Math.Max (bounds.Height, bounds.Width);
				size *= 2f;
				frame.Width = frame.Height = size;
				backgroundImageView.Frame = frame;
				backgroundImageView.Center = albumArtImageView.Center;

				var videoFrame = new CGRect(padding, padding, 78, 44);
				videoFrame.Width += (maxWidth - videoFrame.Width) * currentPercent;
				videoFrame.X -= currentPercent * padding;
				videoFrame.Height = videoFrame.Width * widthRatio;
				videoFrame.Y = NMath.Max((topHeight + (videoFrame.Height / 2)) * currentPercent, padding);

				frame.X = padding;
				frame.Height = topHeight;
				var w = showVideo ? 160 : NowPlayingViewController.AlbumArtWidth;
				frame.Width = w + padding;
				smallArtImageView.Center = frame.GetCenter();
				smallArtImageView.Hidden = showVideo;

				videoView.Frame = showVideo ? videoFrame : frame;
				videoView.Hidden = !showVideo;
				if (isVisible)
					videoView.Show();
				frame.X = frame.Right;
				frame.Width = bounds.Width - frame.X - buttonWidth - (padding * 2);
				labelView.Frame = frame;
			}

			string songId = "";
			string artUrl = "";
			public async void SetSong(Song song)
			{
				songId = song?.Id ?? "";
				artUrl = "";
				labelView.TopLabel.Text = song?.Name ?? "";
				var text = string.IsNullOrWhiteSpace(song?.Artist) || string.IsNullOrWhiteSpace(song?.Album)
					? $"{song?.Artist}{song?.Album}"
					: $"{song?.Artist} - {song?.Album}";
				labelView.BottomLabel.Text = song == null ? "" : text;
				labelView.SetNeedsLayout();
				var locaImage = await song.GetLocalImage(albumArtWidth);
				if (locaImage != null)
				{
					smallArtImageView.Image = backgroundImageView.Image = albumArtImageView.Image = locaImage;
				}
				else
				{

					var aUrl = artUrl = await ArtworkManager.Shared.GetArtwork(song);
					if (string.IsNullOrWhiteSpace(artUrl))
					{
						backgroundImageView.Image = albumArtImageView.Image = Images.GetDefaultAlbumArt(albumArtWidth);
						smallArtImageView.Image = Images.GetDefaultAlbumArt(NowPlayingViewController.AlbumArtWidth);
					}
					else
					{
						backgroundImageView.Image = Images.GetDefaultAlbumArt(albumArtWidth);
						smallArtImageView.Image = Images.GetDefaultAlbumArt(NowPlayingViewController.AlbumArtWidth);
						var frame = albumArtImageView.Frame;
						if (frame.Width <= 0 || frame.Height <= 0) {
							frame.Width = frame.Height = albumArtWidth;
							albumArtImageView.Frame = frame;
						}
						albumArtImageView.SetImage(NSUrl.FromString(artUrl), Images.GetDefaultAlbumArt(albumArtWidth),(image, error, cacheType, imageUrl) => {
							if (aUrl != artUrl)
								return;
							albumArtImageView.Image = image;
							backgroundImageView.Image = image;
							smallArtImageView.SetImage(NSUrl.FromString(artUrl));
						});
					}
				}
			}

			public void UpdatePercent(nfloat percent)
			{
				currentPercent = NMath.Max(0,NMath.Min(percent,1));
				//UIView.AnimateAsync(.2, LayoutSubviews);
				this.LayoutSubviews();

			}
		}

	}
}