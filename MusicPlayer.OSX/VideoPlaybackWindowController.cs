using System;

using Foundation;
using AppKit;
using CoreGraphics;
using MusicPlayer.Managers;
using MusicPlayer.Data;

namespace MusicPlayer
{
	public partial class VideoPlaybackWindowController : NSWindowController, INSWindowDelegate
	{
		public VideoPlaybackWindowController (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public VideoPlaybackWindowController (NSCoder coder) : base (coder)
		{
		}

		public VideoPlaybackWindowController () : base ("VideoPlaybackWindow")
		{
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
		}
		VideoView videoView;
		public override void WindowDidLoad ()
		{
			base.WindowDidLoad ();
			MainView.WantsLayer = true;
			MainView.AddSubview (videoView = new VideoView {
				Frame = MainView.Bounds,
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable,
				Tapped = ()=>{

				},
			});
			Window.Delegate = this;
			Window.MakeKeyAndOrderFront (null);
			Window.Level = NSWindowLevel.Status;
		}
		public new VideoPlaybackWindow Window {
			get { return (VideoPlaybackWindow)base.Window; }
		}
		bool isFullScreen;
		static bool isVisible;
		public static bool IsVisible {
			get {
				return isVisible;
			}
			private set {
				if (isVisible == value)
					return;
				isVisible = value;
				NotificationManager.Shared.ProcVideoPlaybackChanged(Settings.CurrentPlaybackIsVideo);
			}
		}
		public void Show()
		{
			IsVisible = true;
			Window.OrderFront(this);
			videoView.Show ();
		}
		public void Hide()
		{
			if (isFullScreen)
				Window.ToggleFullScreen (this);
			IsVisible = false;
			Window.OrderOut(this);
		}
		public override void Close ()
		{
			IsVisible = false;
			base.Close ();
		}
		public void OnOrderOut()
		{
			IsVisible = false;
		}

		public void Toggle()
		{
			if (IsVisible)
				Hide ();
			else
				Show ();
		}
		[Export ("windowDidEnterFullScreen:")]
		public void DidEnterFullScreen (Foundation.NSNotification notification)
		{
			isFullScreen = true;
			videoView.Frame = MainView.Bounds;
			Console.WriteLine ("Entered Full Screen");
		}

		[Export ("windowDidExitFullScreen:")]
		public void DidExitFullScreen (Foundation.NSNotification notification)
		{
			isFullScreen = false;
			Console.WriteLine ("Exited Full Screen");
			IsVisible = true;
			videoView.Show ();
			Window.MakeKeyAndOrderFront (null);
			Window.Level = NSWindowLevel.Status;
		}

	}
}
