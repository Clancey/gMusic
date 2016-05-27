using System;
using AppKit;
using CoreGraphics;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class ProgressView : NSColorView
	{
		NSColorView backgroundProgress;
		NSColorView downloadProgress;
		NSAnimatedSlider slider;
		public Action<float> ValueChanged { get; set; }
		public ProgressView ()
		{
			BackgroundColor = NSColor.Clear;
			AddSubview (backgroundProgress = new NSColorView {
				BackgroundColor = NSColor.DarkGray,
			});
			AddSubview (downloadProgress = new NSColorView {
				BackgroundColor = NSColor.LightGray,
			});
			AddSubview (slider = new NSAnimatedSlider{
				ValueChanged = (f)=>{
					PlaybackManager.Shared.Seek(f);
				}
			});
			slider.WantsLayer = true;
			slider.Layer.MasksToBounds = false;
			this.WantsLayer = true;
			this.Layer.MasksToBounds = false;

		}

		float _downloadProgress;
		public float DownloadProgress {
			get {
				return _downloadProgress;
			}
			set {
				if (Math.Abs (_downloadProgress - value) < float.Epsilon)
					return;
				_downloadProgress = value;
				ResizeSubviewsWithOldSize (CGSize.Empty);
			}
		}

		public float Progress {
			get {
				return slider.Value;
			}
			set {
				slider.Value = value;
			}
		}
		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			slider.Frame = Bounds;
			var frame = Bounds;
			frame.Height = 2f;
			backgroundProgress.Frame = frame;

			frame.Width *= DownloadProgress;
//			frame.Height = 3f;
			downloadProgress.Frame = frame;
		}
	}
}

