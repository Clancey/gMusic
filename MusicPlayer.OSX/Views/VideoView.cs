using System;
using AppKit;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class VideoView : NSColorView
	{
		public Action Tapped { get; set; } 
		public VideoView ()
		{
			BackgroundColor = NSColor.Black;
			this.AddGestureRecognizer(new NSClickGestureRecognizer(()=>{
				if(Tapped != null)
					Tapped();
				else
					NotificationManager.Shared.ProcToggleFullScreenVideo();
			}));
			this.WantsLayer = true;
		}
		public override void ResizeWithOldSuperviewSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeWithOldSuperviewSize (oldSize);
			PlaybackManager.Shared.NativePlayer.VideoLayer.Frame = Bounds;
		}
		public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			PlaybackManager.Shared.NativePlayer.VideoLayer.Frame = Bounds;
		}
		public void Show()
		{
			if (Hidden)
				return;
			PlaybackManager.Shared.NativePlayer.VideoLayer.Frame = Bounds;
			if (PlaybackManager.Shared.NativePlayer.VideoLayer.SuperLayer == Layer)
				return;
			Layer.AddSublayer(PlaybackManager.Shared.NativePlayer.VideoLayer);
		}
	}
}

