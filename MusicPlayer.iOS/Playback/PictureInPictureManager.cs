using AVFoundation;
using AVKit;
using Foundation;
using MusicPlayer.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayer.iOS.Playback
{
	class PictureInPictureManager : AVPictureInPictureControllerDelegate
	{
		public static PictureInPictureManager Shared { get; set; } = new PictureInPictureManager();

		bool IsSetep;
		AVPictureInPictureController controller;
		public void Setup(CustomVideoLayer layer)
		{
			if(!IsSupported() || IsSetep)
				return;
			
			if (layer?.VideoLayer != null)
			{
				controller = new AVPictureInPictureController(layer.VideoLayer);
				controller.Delegate = this;
			}

			layer.VideoLayerChanged += (AVPlayerLayer obj) => {
				if (!IsSupported())
					return;
				bool isActive = controller?.PictureInPictureActive ?? false;
				if(isActive)
					controller?.StopPictureInPicture();
				controller = new AVPictureInPictureController(layer.VideoLayer);
				controller.Delegate = this;
				if(isActive)
					controller.StartPictureInPicture();

			};
			IsSetep = true;

		}
		static bool IsSupported()
		{
			return Device.IsIos9 && AVPictureInPictureController.IsPictureInPictureSupported;
		}
		public bool StartPictureInPicture()
		{
			if(!IsSupported() || controller == null)
				return false;
			if (controller.PictureInPictureActive)
				return true;
            controller.StartPictureInPicture();
			return true;
        }

		public void StopPictureInPicture()
		{
			if (!IsSupported() || controller == null || !controller.PictureInPictureActive)
				return;

			controller.StopPictureInPicture();
		}

		public override void DidStartPictureInPicture(AVPictureInPictureController pictureInPictureController)
		{
			Console.WriteLine("DidStartPictureInPicture(pictureInPictureController)");
		}
		public override void DidStopPictureInPicture(AVPictureInPictureController pictureInPictureController)
		{
			Console.WriteLine("DidStopPictureInPicture(pictureInPictureController)");
        }

		public override void FailedToStartPictureInPicture(AVPictureInPictureController pictureInPictureController, NSError error)
		{
			Console.WriteLine($"FailedToStartPictureInPicture(pictureInPictureController, {error.LocalizedDescription})");
        }
		public override void WillStartPictureInPicture(AVPictureInPictureController pictureInPictureController)
		{
			Console.WriteLine("WillStartPictureInPicture(pictureInPictureController)");
        }
		public override void WillStopPictureInPicture(AVPictureInPictureController pictureInPictureController)
		{
			Console.WriteLine("WillStopPictureInPicture(pictureInPictureController)");
        }
	
	}
}
