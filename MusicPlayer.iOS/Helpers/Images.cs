using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using NGraphics;
using UIKit;

namespace MusicPlayer
{
	public static class Images
	{
		public static float MaxScreenSize;
		public static float AlbumArtScreenSize => Math.Max(MaxScreenSize, 640);

		public static Lazy<UIImage> DisclosureImage =
			new Lazy<UIImage>(() => "SVG/more.svg".LoadImageFromSvg(new Size(15, 15), UIImageRenderingMode.AlwaysTemplate));

		public static Lazy<UIImage> DisclosureTallImage =
			new Lazy<UIImage>(() => "SVG/moreTall.svg".LoadImageFromSvg(new Size(15, 15), UIImageRenderingMode.AlwaysTemplate));

		public static Lazy<UIImage> AccentImage = new Lazy<UIImage>(() => UIImage.FromBundle("accentColor"));

		static readonly Dictionary<Tuple<string, string>, UIImage> CachedGeneratedImages =
			new Dictionary<Tuple<string, string>, UIImage>();

		public static UIImage GetDisclosureImage(double width, double height)
		{
			return GetGeneratedImage("SVG/more.svg", width,height).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
		}
		public static UIImage GetRadioIcon(double width,double height)
		{
			return GetGeneratedImage("SVG/radio.svg", width,height);
		}

		public static UIImage GetDefaultSongImage(double size)
		{
			return GetGeneratedImage("SVG/songsDefault.svg", size);
		}

		public static UIImage GetDefaultAlbumArt(double size)
		{
			return GetGeneratedImage("SVG/icon.svg", size);
		}

		public static UIImage GetPlaybackButton(double size)
		{
			return GetGeneratedImage("SVG/playButton.svg", size).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
		}

		public static UIImage GetBorderedPlaybackButton(double size)
		{
			return
				GetGeneratedImage("SVG/playButtonBordered.svg", size).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
		}

		public static UIImage GetPauseButton(double size)
		{
			return GetGeneratedImage("SVG/pauseButton.svg", size).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
		}

		public static UIImage GetBorderedPauseButton(double size)
		{
			return
				GetGeneratedImage("SVG/pauseButtonBordered.svg", size).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
		}

		public static UIImage GetNextButton(double size)
		{
			return GetGeneratedImage("SVG/next.svg", size).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
		}

		public static UIImage GetPreviousButton(double size)
		{
			return GetGeneratedImage("SVG/previous.svg", size).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate) 	;
		}

		public static UIImage GetShuffleImage(double size)
		{
			return GetGeneratedImage("SVG/shuffle.svg", size);
		}

		public static UIImage GetRepeatImage(double size)
		{
			return GetGeneratedImage("SVG/repeat.svg", size);
		}

		public static UIImage GetRepeatOneImage(double size)
		{
			return GetGeneratedImage("SVG/repeatOne.svg", size);
		}

		public static UIImage GetThumbsUpImage(double size)
		{
			return GetGeneratedImage("SVG/thumbsUp.svg", size);
		}

		public static UIImage GetThumbsDownImage(double size)
		{
			return GetGeneratedImage("SVG/thumbsDown.svg", size);
		}

		public static UIImage GetShareIcon(double size)
		{
			return GetGeneratedImage("SVG/share.svg", size);
		}

		public static UIImage GetCloseImage(double size)
		{
			return GetGeneratedImage("SVG/close.svg", size);
		}

		public static UIImage GetPlaylistIcon(double size)
		{
			return GetGeneratedImage("SVG/playlists.svg", size);
		}

		public static UIImage GetSliderTrack()
		{
			return GetGeneratedImage("SVG/sliderTrack.svg", -1);
		}

		public static UIImage GetPlaybackSliderThumb()
		{
			return GetGeneratedImage("SVG/playbackSliderThumb.svg", 37);
		}

		public static UIImage GetMusicNotes(double size)
		{
			return GetGeneratedImage("SVG/musicalNotes.svg", size);
		}

		public static UIImage GetOfflineImage(double size)
		{
			return GetGeneratedImage("SVG/isOffline.svg",size);
		}
		public static UIImage GetVideoIcon(double size)
		{
			return GetGeneratedImage("SVG/videoIcon.svg", size);
		}

		public static UIImage GetEditIcon(double size)
		{
			return GetGeneratedImage("SVG/edit.svg", size);
		}

		public static UIImage GetDeleteIcon(double size)
		{
			return GetGeneratedImage("SVG/trash.svg", size);
		}

		public static UIImage GetCopyIcon(double size)
		{
			return GetGeneratedImage("SVG/copy.svg", size);
		}

		public static UIImage GetUndoImage(double size)
		{
			return GetGeneratedImage("SVG/undo.svg",size);
		}


		public static UIImage MenuImage => GetGeneratedImage("SVG/menu.svg", 15, 15);

		static UIImage GetGeneratedImage(string name, double size)
		{
			return GetGeneratedImage(name, size, size);
		}

		static UIImage GetGeneratedImage(string imageName, double width, double height)
		{
			return GetGeneratedImage(imageName, new Size(width, height));
		}

		static UIImage GetGeneratedImage(string imageName, Size size)
		{
			var tuple = new Tuple<string, string>(imageName, $"{size.Width},{size.Height}");
			UIImage image;
			if (!CachedGeneratedImages.TryGetValue(tuple, out image))
			{
				CachedGeneratedImages[tuple] = image = imageName.LoadImageFromSvg(size);
			}
			return image;
		}

		static UIImage GetAppBundleImage(string imageName)
		{
			var tuple = new Tuple<string, string>(imageName, imageName);
			UIImage image;
			if (!CachedGeneratedImages.TryGetValue(tuple, out image))
			{
				CachedGeneratedImages[tuple] = image = UIImage.FromBundle(imageName);
			}
			return image;
		}
	}
}