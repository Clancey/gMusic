using System;

using Foundation;
using AppKit;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public partial class VideoPlaybackWindow : NSWindow
	{
		public VideoPlaybackWindow (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public VideoPlaybackWindow (NSCoder coder) : base (coder)
		{
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
		}
		public override void OrderOut (NSObject sender)
		{
			base.OrderOut (sender);
			(this.WindowController as VideoPlaybackWindowController).OnOrderOut ();
		}

		public override void KeyUp (NSEvent theEvent)
		{
			//base.KeyUp (theEvent);
			Console.WriteLine(theEvent.KeyCode);
			switch (theEvent.KeyCode) {
			case 49:
				PlaybackManager.Shared.PlayPause ();
				break;
			case 124:
				PlaybackManager.Shared.NextTrack ();
				break;
			case 123:
				PlaybackManager.Shared.Previous ();
				break;
			}
		}
	}
}
