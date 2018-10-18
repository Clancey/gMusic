using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using NGraphics;
using AppKit;

namespace MusicPlayer
{
	public static class Images
	{
		public static float MaxScreenSize;
		public static float AlbumArtScreenSize => Math.Max(MaxScreenSize, 640);

		public static Lazy<NSImage> DisclosureImage =
			new Lazy<NSImage>(() => "SVG/more.svg".LoadImageFromSvg(new Size(15, 15)));

		public static Lazy<NSImage> DisclosureTallImage =
			new Lazy<NSImage>(() => "SVG/moreTall.svg".LoadImageFromSvg(new Size(15, 15)));

		public static Lazy<NSImage> AccentImage = new Lazy<NSImage>(() => new NSImage("accentColor.png"));
//
		static readonly Dictionary<Tuple<string, string>, NSImage> CachedGeneratedImages =
			new Dictionary<Tuple<string, string>, NSImage>();

		public static NSImage GetDefaultSongImage(double size)
		{
			return GetGeneratedImage("SVG/songsDefault.svg", size);
		}

		public static NSImage GetDefaultAlbumArt(double size)
		{
			return GetGeneratedImage("SVG/icon.svg", size);
		}

		public static NSImage GetPlaybackButton(double size)
		{
			return GetGeneratedImage("SVG/playButton.svg", size, NSColor.ControlText);
		}

		public static NSImage GetBorderedPlaybackButton(double size)
		{
			return
				GetGeneratedImage("SVG/playButtonBordered.svg", size, NSColor.ControlText);
		}

		public static NSImage GetPauseButton(double size)
		{
			return GetGeneratedImage("SVG/pauseButton.svg", size, NSColor.ControlText);
		}

		public static NSImage GetBorderedPauseButton(double size)
		{
			return
				GetGeneratedImage("SVG/pauseButtonBordered.svg", size, NSColor.ControlText);
		}

		public static NSImage GetNextButton(double size)
		{
			return GetGeneratedImage("SVG/next.svg", size, NSColor.ControlText);
		}

		public static NSImage GetPreviousButton(double size)
		{
			return GetGeneratedImage("SVG/previous.svg", size, NSColor.ControlText);
		}

		public static NSImage GetShuffleImage(double size)
		{
			return GetGeneratedImage("SVG/shuffle.svg", size, NSColor.ControlText);
		}

		public static NSImage GetShuffleOffImage(double size)
		{
			return GetGeneratedImage("SVG/shuffle.svg", size, NSColor.ControlText);
		}

		public static NSImage GetShuffleOnImage(double size)
		{
			return GetGeneratedImage("SVG/shuffle.svg", size, Style.Current.AccentColor);
		}
		public static NSImage GetRepeatImage(double size)
		{
			return GetGeneratedImage("SVG/repeat.svg", size, NSColor.ControlText);
		}

		public static NSImage GetRepeatOnImage(double size)
		{
			return GetGeneratedImage("SVG/repeat.svg", size, Style.Current.AccentColor);
		}

		public static NSImage GetRepeatOneImage(double size)
		{
			return GetGeneratedImage("SVG/repeatOne.svg", size, Style.Current.AccentColor);
		}

		public static NSImage GetThumbsUpOffImage(double size)
		{
			return GetGeneratedImage("SVG/thumbsUp.svg", size, NSColor.ControlText);
		}
		public static NSImage GetThumbsUpOnImage(double size)
		{
			return GetGeneratedImage("SVG/thumbsUp.svg", size, Style.Current.AccentColor);
		}

		public static NSImage GetThumbsDownOffImage(double size)
		{
			return GetGeneratedImage("SVG/thumbsDown.svg", size, NSColor.ControlText);
		}

		public static NSImage GetThumbsDownOnImage(double size)
		{
			return GetGeneratedImage("SVG/thumbsDown.svg", size, Style.Current.AccentColor);
		}

		public static NSImage GetShareIcon(double size)
		{
			return GetGeneratedImage("SVG/share.svg", size, NSColor.ControlText);
		}

		public static NSImage GetCloseImage(double size)
		{
			return GetGeneratedImage("SVG/close.svg", size, NSColor.ControlText);
		}

		public static NSImage GetPlaylistIcon(double size)
		{
			return GetGeneratedImage("SVG/playlists.svg", size, NSColor.ControlText);
		}

		public static NSImage GetSliderTrack()
		{
			return GetGeneratedImage("SVG/sliderTrack.svg", -1);
		}

		public static NSImage GetPlaybackSliderThumb()
		{
			return GetGeneratedImage("SVG/playbackSliderThumb.svg", 37);
		}

		public static NSImage GetMusicNotes(double size)
		{
			return GetGeneratedImage("SVG/musicalNotes.svg", size, NSColor.ControlText);
		}

		public static NSImage GetOfflineImage(double size)
		{
			return GetGeneratedImage("SVG/isOffline.svg", size, NSColor.ControlText);
		}
		public static NSImage GetVideoIcon(double size)
		{
			return GetGeneratedImage("SVG/videoIcon.svg", size, NSColor.ControlText);
		}

		public static NSImage GetEditIcon(double size)
		{
			return GetGeneratedImage("SVG/edit.svg", size);
		}

		public static NSImage GetDeleteIcon(double size)
		{
			return GetGeneratedImage("SVG/trash.svg", size);
		}

		public static NSImage GetCopyIcon(double size)
		{
			return GetGeneratedImage("SVG/copy.svg", size);
		}

		public static NSImage GetUndoImage(double size)
		{
			return GetGeneratedImage("SVG/undo.svg", size);
		}


		public static NSImage MenuImage => GetGeneratedImage("SVG/menu.svg", 15, 15);

		static NSImage GetGeneratedImage(string name, double size)
		{
			return GetGeneratedImage(name, size, size);
		}
		static NSImage GetGeneratedImage(string name, double size, NSColor color)
		{
			return GetGeneratedImage(name, new Size(size, size), color);
		}

		static NSImage GetGeneratedImage(string imageName, double width, double height)
		{
			return GetGeneratedImage(imageName, new Size(width, height));
		}

		static NSImage GetGeneratedImage(string imageName, Size size, NSColor color = null)
		{
			var tuple = new Tuple<string, string>(imageName, $"{size.Width},{size.Height},{color}");
			NSImage image;
			if (!CachedGeneratedImages.TryGetValue(tuple, out image))
			{
				CachedGeneratedImages[tuple] = image = imageName.LoadImageFromSvg(size, color);
			}
			return image;
		}

		static NSImage GetAppBundleImage(string imageName)
		{
			var tuple = new Tuple<string, string>(imageName, imageName);
			NSImage image;
			if (!CachedGeneratedImages.TryGetValue(tuple, out image))
			{
				CachedGeneratedImages[tuple] = image = new NSImage(imageName);
			}
			return image;
		}
	}
}