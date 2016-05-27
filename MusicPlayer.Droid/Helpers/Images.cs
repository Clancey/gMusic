using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGraphics;
using Android.Graphics;
using Android;

namespace MusicPlayer
{
	public static class Images
	{
		public static float MaxScreenSize;
		public static float AlbumArtScreenSize => Math.Max(MaxScreenSize, 640);

		public static Lazy<Bitmap> DisclosureImage =
			new Lazy<Bitmap>(() => "SVG/more.svg".LoadImageFromSvg(new Size(15, 15)));

		public static Lazy<Bitmap> DisclosureTallImage =
			new Lazy<Bitmap>(() => "SVG/moreTall.svg".LoadImageFromSvg(new Size(15, 15)));

		//public static Lazy<Bitmap> AccentImage = new Lazy<Bitmap>(() => Bitmap.FromBundle("accentColor"));

		static readonly Dictionary<Tuple<string, string>, Bitmap> CachedGeneratedImages =
			new Dictionary<Tuple<string, string>, Bitmap>();

		public static Bitmap GetDisclosureImage(double width, double height)
		{
			return GetGeneratedImage("SVG/more.svg", width,height);
		}
		public static Bitmap GetRadioIcon(double width,double height)
		{
			return GetGeneratedImage("SVG/radio.svg", width,height);
		}

		public static Bitmap GetDefaultSongImage(double size)
		{
			return GetGeneratedImage("SVG/songsDefault.svg", size);
		}

		public static Bitmap GetDefaultAlbumArt(double size)
		{
			return GetGeneratedImage("SVG/icon.svg", size);
		}

		public static Bitmap GetPlaybackButton(double size)
		{
			return GetGeneratedImage("SVG/playButton.svg", size);
		}

		public static Bitmap GetBorderedPlaybackButton(double size)
		{
			return
				GetGeneratedImage("SVG/playButtonBordered.svg", size);
		}

		public static Bitmap GetPauseButton(double size)
		{
			return GetGeneratedImage("SVG/pauseButton.svg", size);
		}

		public static Bitmap GetBorderedPauseButton(double size)
		{
			return
				GetGeneratedImage("SVG/pauseButtonBordered.svg", size);
		}

		public static Bitmap GetNextButton(double size)
		{
			return GetGeneratedImage("SVG/next.svg", size);
		}

		public static Bitmap GetPreviousButton(double size)
		{
			return GetGeneratedImage("SVG/previous.svg", size);
		}

		public static Bitmap GetShuffleImage(double size)
		{
			return GetGeneratedImage("SVG/shuffle.svg", size);
		}

		public static Bitmap GetRepeatImage(double size)
		{
			return GetGeneratedImage("SVG/repeat.svg", size);
		}

		public static Bitmap GetRepeatOneImage(double size)
		{
			return GetGeneratedImage("SVG/repeatOne.svg", size);
		}

		public static Bitmap GetThumbsUpImage(double size)
		{
			return GetGeneratedImage("SVG/thumbsUp.svg", size);
		}

		public static Bitmap GetThumbsDownImage(double size)
		{
			return GetGeneratedImage("SVG/thumbsDown.svg", size);
		}

		public static Bitmap GetShareIcon(double size)
		{
			return GetGeneratedImage("SVG/share.svg", size);
		}

		public static Bitmap GetCloseImage(double size)
		{
			return GetGeneratedImage("SVG/close.svg", size);
		}

		public static Bitmap GetPlaylistIcon(double size)
		{
			return GetGeneratedImage("SVG/playlists.svg", size);
		}

		public static Bitmap GetSliderTrack()
		{
			return GetGeneratedImage("SVG/sliderTrack.svg", -1);
		}

		public static Bitmap GetPlaybackSliderThumb()
		{
			return GetGeneratedImage("SVG/playbackSliderThumb.svg", 37);
		}

		public static Bitmap GetMusicNotes(double size)
		{
			return GetGeneratedImage("SVG/musicalNotes.svg", size);
		}

		public static Bitmap GetOfflineImage(double size)
		{
			return GetGeneratedImage("SVG/isOffline.svg",size);
		}
		public static Bitmap GetVideoIcon(double size)
		{
			return GetGeneratedImage("SVG/videoIcon.svg", size);
		}

		public static Bitmap GetEditIcon(double size)
		{
			return GetGeneratedImage("SVG/edit.svg", size);
		}

		public static Bitmap GetDeleteIcon(double size)
		{
			return GetGeneratedImage("SVG/trash.svg", size);
		}

		public static Bitmap GetCopyIcon(double size)
		{
			return GetGeneratedImage("SVG/copy.svg", size);
		}

		public static Bitmap GetUndoImage(double size)
		{
			return GetGeneratedImage("SVG/undo.svg",size);
		}


		public static Bitmap MenuImage => GetGeneratedImage("SVG/menu.svg", 15, 15);

		static Bitmap GetGeneratedImage(string name, double size)
		{
			return GetGeneratedImage(name, size, size);
		}

		static Bitmap GetGeneratedImage(string imageName, double width, double height)
		{
			return GetGeneratedImage(imageName, new Size(width, height));
		}

		static Bitmap GetGeneratedImage(string imageName, Size size)
		{
			var tuple = new Tuple<string, string>(imageName, $"{size.Width},{size.Height}");
			Bitmap image;
			if (!CachedGeneratedImages.TryGetValue(tuple, out image))
			{
				CachedGeneratedImages[tuple] = image = imageName.LoadImageFromSvg(size);
			}
			return image;
		}

		//static Bitmap GetAppBundleImage(string imageName)
		//{
		//	var tuple = new Tuple<string, string>(imageName, imageName);
		//	Bitmap image;
		//	if (!CachedGeneratedImages.TryGetValue(tuple, out image))
		//	{
		//		CachedGeneratedImages[tuple] = image = Bitmap.FromBundle(imageName);
		//	}
		//	return image;
		//}
	}
}