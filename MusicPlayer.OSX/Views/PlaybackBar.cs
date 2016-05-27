using System;
using System.Reactive.Linq;
using Foundation;
using AppKit;
using CoreGraphics;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using MusicPlayer.Data;
using Akavache;
using Splat;

namespace MusicPlayer
{
	public  class PlaybackBar : NSColorView
	{
		NSImageView AlbumArt;
		NSButton previous;
		NSButton next;
		NSButton play;
		TwoLabelView textView;
		ProgressView progress;
		VideoView videoView;
		NSSlider volumeSlider;
		NSButton shuffle;
		NSButton repeat;
		NSTextField remaining;
		NSTextField time;
		NSButton thumbsDown;
		NSButton thumbsUp;
		public PlaybackBar (IntPtr handle) : base (handle)
		{
			AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
		}

		public PlaybackBar (CGRect rect) : base (rect)
		{
			BackgroundColor = NSColor.FromRgba(249,249,249,255);
			AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
			AddSubview (AlbumArt = new NSImageView (new CGRect (0, 0, 67, 67)));
			videoView = new VideoView { WantsLayer = true, Hidden = true };

			videoView.MakeBackingLayer ();

			AddSubview (previous = CreateButton ("SVG/previous.svg", PlaybackManager.Shared.Previous));
			AddSubview (play = CreateButton ("SVG/playButtonBordered.svg",()=>{
				if(playing)
					PlaybackManager.Shared.Pause();
				else
					PlaybackManager.Shared.Play();
			}));
			AddSubview (next = CreateButton ("SVG/next.svg",()=>PlaybackManager.Shared.NextTrack()));
			AddSubview (textView = new TwoLabelView ());

			AddSubview (progress = new ProgressView());

			AddSubview(shuffle = CreateButton("SVG/shuffle.svg",25,PlaybackManager.Shared.ToggleRandom));
			AddSubview(repeat = CreateButton("SVG/repeat.svg",25,PlaybackManager.Shared.ToggleRepeat));

			AddSubview(time = new NSTextField{StringValue = "0000:00"}.StyleAsSubText());
			time.SizeToFit();

			AddSubview(remaining = new NSTextField{StringValue = "0000:00", Alignment = NSTextAlignment.Right}.StyleAsSubText());
			remaining.SizeToFit();

			AddSubview(volumeSlider = new NSSlider {DoubleValue = Settings.CurrentVolume,  MinValue = 0, MaxValue = 1});
			volumeSlider.Activated += (object sender, EventArgs e) =>
			{
				Settings.CurrentVolume = (float)volumeSlider.DoubleValue;
			};
			volumeSlider.SizeToFit();

			AddSubview(thumbsDown = CreateButton("SVG/thumbsDown.svg", 30, async () =>
			  {
				var song = MusicManager.Shared.GetCurrentSong();
				if(song.Rating != 1)
					await MusicManager.Shared.ThumbsDown(song);
				else
				{
					await MusicManager.Shared.Unrate(song);
				}
				SetThumbsState(song);
			  }));

			AddSubview(thumbsUp = CreateButton("SVG/thumbsUp.svg", 30, async () =>
			{
				var song = MusicManager.Shared.GetCurrentSong();
				if (song.Rating != 5)
					await MusicManager.Shared.ThumbsUp(song);
				else
					await MusicManager.Shared.Unrate(song);
				SetThumbsState(song);
			}));

			Update (PlaybackManager.Shared.NativePlayer.CurrentSong);
			var dropShadow = new NSShadow{
				ShadowColor = NSColor.Black,
			};
			this.WantsLayer = true;
			this.Shadow = dropShadow;
			NotificationManager.Shared.CurrentSongChanged += (sender, e) => Update (e.Data);
			NotificationManager.Shared.PlaybackStateChanged += (sender, e) => SetState (e.Data);

			NotificationManager.Shared.CurrentTrackPositionChanged += (object sender, SimpleTables.EventArgs<TrackPosition> e) => {
				var data = e.Data;
				//timeLabel.Text = data.CurrentTimeString;
				//remainingTimeLabel.Text = data.RemainingTimeString;
				progress.Progress = data.Percent;
				time.StringValue = data.CurrentTimeString;
				remaining.StringValue = data.RemainingTimeString;
			};

			NotificationManager.Shared.ShuffleChanged += (sender, args) => SetShuffleState(args.Data);
			NotificationManager.Shared.RepeatChanged += (sender, args) => SetRepeatState(args.Data);
			NotificationManager.Shared.SongDownloadPulsed += (object sender, NotificationManager.SongDowloadEventArgs e) => {
				if (e.SongId != Settings.CurrentSong)
					return;
				progress.DownloadProgress = e.Percent;
			};

//			NotificationManager.Shared.ToggleFullScreenVideo += (s, a) => ToggleFullScreenVideo();
			SetState (PlaybackState.Stopped);
			SetShuffleState(Settings.ShuffleSongs);
			SetRepeatState(Settings.RepeatMode);
		}

		const float buttonSize = 50f;

		NSButton CreateButton (string svg,  Action clicked)
		{
			var button = CreateButton(svg, buttonSize, clicked);
			return button;
		}

		NSButton CreateButton (string svg,float size , Action clicked)
		{
			var button = new NSButton (new CGRect (0, 0, size, size));
			button.Image = svg.LoadImageFromSvg (new NGraphics.Size (25, 25));
			button.Bordered = false;
			button.Activated += (sender, e) => clicked();
			button.ImagePosition = NSCellImagePosition.ImageOnly;
			return button;
		}

		public async void Update (Song song)
		{
			textView.TopLabel.StringValue = song?.Name ?? "";
			textView.BottomLabel.StringValue = song?.DetailText ?? "";
			textView.ResizeSubviewsWithOldSize (CGSize.Empty);
			SetThumbsState(song);
			//TODO: default album art;
			await AlbumArt.LoadFromItem (song);
		}
		bool playing;
		void SetState (PlaybackState state)
		{
			switch (state) {
			case PlaybackState.Paused:
			case PlaybackState.Stopped:
				playing = false;
				play.Image = "SVG/playButtonBordered.svg".LoadImageFromSvg (new NGraphics.Size (50, 50));
				break;
			default:
				CheckVideoStatus();
				playing = true;
				play.Image = "SVG/pauseButtonBordered.svg".LoadImageFromSvg (new NGraphics.Size (50, 50));
				SetVideoState (Settings.CurrentPlaybackIsVideo);
				break;
			}
		}
		bool isSetup;
		void CheckVideoStatus()
		{
			if (isSetup)
				return;
			AddSubview(videoView);
			SetVideoState(IsVideo);
			
		}
		void SetShuffleState (bool shuffleSongs)
		{
			const float imageSize = 20;
			shuffle.Image = shuffleSongs ? Images.GetShuffleOnImage(imageSize) : Images.GetShuffleOffImage(imageSize);
		}

		void SetRepeatState (RepeatMode repeatMode)
		{
			const float imageSize =15;
			NSImage image = null;
			switch (repeatMode)
			{
				case RepeatMode.RepeatAll:
					image = Images.GetRepeatOnImage(imageSize);
					break;
				case RepeatMode.RepeatOne:
					image = Images.GetRepeatOneImage(imageSize);
					break;
				default:
					image = Images.GetRepeatImage(imageSize);
					break;
			}
			repeat.Image = image;
		}
		bool IsVideo;
		void SetVideoState(bool isVideo)
		{
			IsVideo = isVideo;
			videoView.Hidden = !isVideo || VideoPlaybackWindowController.IsVisible;
			if (!videoView.Hidden)
				videoView.Show ();
		}
		public void SetThumbsState(Song song)
		{
			const float imageSize = 25;
			thumbsDown.Image = song?.Rating == 1 ? Images.GetThumbsDownOnImage(imageSize) : Images.GetThumbsDownOffImage(imageSize);
			thumbsUp.Image = song?.Rating == 5 ? Images.GetThumbsUpOnImage(imageSize) : Images.GetThumbsUpOffImage(imageSize);
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		const float padding = 5;
		const float minWidth = 250;
		const float progressHeight = 15f;
		const float volumeWidth = 150f;
		public override void ResizeSubviewsWithOldSize (CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			var bounds = Bounds;
			var frame = bounds;
			frame.Width = bounds.Height;
			AlbumArt.Frame = videoView.Frame = frame;
			videoView.ResizeSubviewsWithOldSize (videoView.Bounds.Size);

			var left = frame.Right + padding;

			var midY = bounds.Height / 2;

			var buttonx = NMath.Max((bounds.Width / 2) - (buttonSize * 1.5f) - padding*2 - 30, minWidth);

			frame.Y += 5;
			frame.Height -= 5;
			frame.X = left;
			frame.Width = buttonx - left;
			textView.Frame = frame;

			left = frame.Right + padding;

			frame = thumbsDown.Frame;
			frame.X = left;
			frame.Y = midY  - (frame.Height/2) + padding;
			thumbsDown.Frame = frame;

			left = frame.Right + padding;
			frame = new CGRect (left, 0, bounds.Height, bounds.Height);
			previous.Frame = frame;

			frame.X = frame.Right + padding;
			play.Frame = frame;

			frame.X = frame.Right + padding;
			next.Frame = frame;

			var right = frame.Right + padding;
			frame = thumbsUp.Frame;
			frame.X = right;
			frame.Y = midY - (frame.Height/2) - padding;
			thumbsUp.Frame = frame;

			right = frame.Right + padding;
			frame = shuffle.Frame;
			frame.X = right;
			frame.Y = midY - frame.Height;
			shuffle.Frame = frame;

			frame.Y = midY;
			repeat.Frame = frame;


			frame = bounds;
			frame.Height = progressHeight;
			frame.X = AlbumArt.Frame.Right;
			frame.Width -= frame.X;

			progress.Frame = frame;
			left = frame.Left;
			right = frame.Right;
			frame = time.Frame;
			frame.X = left + padding;
			frame.Y = 3;
			time.Frame = frame;

			frame.X = right - (frame.Width + padding);
			remaining.Frame = frame;

			frame = volumeSlider.Frame;
			frame.Width = volumeWidth;
			frame.X = bounds.Right - volumeWidth - padding;
			frame.Y = midY - (frame.Height / 2) + padding;
			volumeSlider.Frame = frame;

		}
	}
}
