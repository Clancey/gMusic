using System;
using CoreAnimation;
using AVFoundation;
namespace MusicPlayer
{
	public class CustomVideoLayer : CALayer
	{
		public event Action<AVPlayerLayer> VideoLayerChanged;
		AVPlayerLayer videoLayer;

		public AVPlayerLayer VideoLayer {
			get {
				return videoLayer;
			}
			set {
				if (videoLayer == value)
					return;
				videoLayer?.RemoveFromSuperLayer ();
				AddSublayer (videoLayer = value);
				VideoLayerChanged?.InvokeOnMainThread (value);
			}
		}

		public override void LayoutSublayers ()
		{
			base.LayoutSublayers ();
			if (videoLayer == null)
				return;
			videoLayer.Frame = Bounds;
		}

	}
}
