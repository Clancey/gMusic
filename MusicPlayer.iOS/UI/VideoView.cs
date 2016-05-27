using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Managers;
using UIKit;

namespace MusicPlayer.iOS.UI
{
	class VideoView : UIView
	{
		public Action Tapped { get; set; } 
		public VideoView()
		{
			BackgroundColor = UIColor.Black;
			this.AddGestureRecognizer(new UITapGestureRecognizer(() =>
			{
				if(Tapped != null)
					Tapped();
				else
					NotificationManager.Shared.ProcToggleFullScreenVideo();
            }));
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			PlaybackManager.Shared.NativePlayer.VideoLayer.Frame = Bounds;
		}

		public void Show()
		{
			if (Hidden)
				return;
			if (PlaybackManager.Shared.NativePlayer.VideoLayer.SuperLayer == Layer)
				return;
			Layer.AddSublayer(PlaybackManager.Shared.NativePlayer.VideoLayer);
		}
	}
}
