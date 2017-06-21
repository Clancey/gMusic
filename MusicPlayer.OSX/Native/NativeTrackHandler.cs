using System;
using MusicPlayer.Managers;
using Foundation;
using MusicPlayer.Models;
using AppKit;
using System.Threading.Tasks;
using Akavache;
using System.Reactive.Linq;
using Splat;
using MediaPlayer;
using MusicPlayer.Data;
using CoreGraphics;

namespace MusicPlayer.Playback
{
	partial class NativeTrackHandler
	{

		void OnInit ()
		{
			NotificationManager.Shared.PlaybackStateChanged += (sender, e) => PlaybackStateChanged (e.Data);
			NSUserNotificationCenter.DefaultUserNotificationCenter.DidActivateNotification += (object sender, UNCDidActivateNotificationEventArgs e) => {
				switch (e.Notification.ActivationType) {

				case NSUserNotificationActivationType.ActionButtonClicked:
					var frontmost = NSWorkspace.SharedWorkspace.FrontmostApplication.BundleIdentifier == NSBundle.MainBundle.BundleIdentifier;
					if (frontmost)
						NSWorkspace.SharedWorkspace.FrontmostApplication.Hide ();
					Console.WriteLine (frontmost);
					PlaybackManager.Shared.NextTrack ();
					break;
				}
			};

			//NotificationManager.Shared.CurrentTrackPositionChanged += (sender, args) => UpdateProgress(args.Data);
		}

		MPMediaItemArtwork CreateDefaultArtwork () => DefaultResizableArtwork;
		void OnSongChanged (Song song)
		{
			var frontmost = NSWorkspace.SharedWorkspace.FrontmostApplication.BundleIdentifier == NSBundle.MainBundle.BundleIdentifier;
			if (frontmost || PlaybackManager.Shared.NativePlayer.State != PlaybackState.Playing)
				return;
			var notification = CreateNotification (song);
			NSUserNotificationCenter.DefaultUserNotificationCenter.RemoveAllDeliveredNotifications ();
			NSUserNotificationCenter.DefaultUserNotificationCenter.DeliverNotification (notification);
		}

		//void SetAdditionInfo (Song song, MPNowPlayingInfo info)
		//{
		//	info.MediaType = Settings.CurrentPlaybackIsVideo ? MPNowPlayingInfoMediaType.Video : MPNowPlayingInfoMediaType.Audio;

		//}

		public void PlaybackStateChanged (PlaybackState state)
		{
			if (state == PlaybackState.Paused || state == PlaybackState.Stopped)
				NSUserNotificationCenter.DefaultUserNotificationCenter.RemoveAllDeliveredNotifications ();
			if (state == PlaybackState.Playing)
				UpdateSong (PlaybackManager.Shared.NativePlayer.CurrentSong);
			setMpMediaCenterState (state);
		}
		void setMpMediaCenterState (PlaybackState state)
		{
			try {
				if (!HasMPMediaCenter)
					return;
				MPNowPlayingPlaybackState mpState;
				switch (state) {
				case PlaybackState.Playing:
				case PlaybackState.Buffering:
					mpState = MPNowPlayingPlaybackState.Playing;
					break;
				case PlaybackState.Paused:
					mpState = MPNowPlayingPlaybackState.Paused;
					break;
				case PlaybackState.Stopped:
					mpState = MPNowPlayingPlaybackState.Stopped;
					break;
				default:
					mpState = MPNowPlayingPlaybackState.Unknown;
					break;
				}
				MPNowPlayingInfoCenter.DefaultCenter.PlaybackState = mpState;
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}

		}
		bool? hasMPMediaCenter;
		bool HasMPMediaCenter => (bool)(hasMPMediaCenter ?? (hasMPMediaCenter = System.Environment.OSVersion.Version > new Version (10, 12, 1)));

		public NSUserNotification CreateNotification (Song song)
		{
			var notification = new NSUserNotification {
				Title = song.Name,
				Subtitle = song.Album,
				InformativeText = song.Artist,
				HasActionButton = true,
				ActionButtonTitle = "Skip",
			};

			//todo: get artwork
			//notification.ContentImage
			SetImage (notification, song);
			//notification.contentImage = [self albumArtForTrack: currentTrack];


			//Private APIs – remove if publishing to Mac App Store
			try {
				notification.SetValueForKey (NSObject.FromObject (true), (NSString)"_showsButtons");
				// Show album art on the left side of the notification (where app icon normally is),
				//like iTunes does
				//[notification setValue:notification.contentImage forKey:@"_identityImage"];
				//notification.contentImage = nil;
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}

			return notification;
		}

		void SetImage (NSUserNotification notitification, Song song)
		{

		}
		async void FetchArtwork (Song song)
		{
			try {
				var art = await song.GetLocalImage (Images.MaxScreenSize);

				if (art == null) {
					var url = await ArtworkManager.Shared.GetArtwork (song);
					if (string.IsNullOrWhiteSpace (url))
						return;
					if (art == null || song.Id != Settings.CurrentSong)
						return;
				}

				artwork = new MPMediaItemArtwork (new CGSize (9999, 9999), (arg) => {
					var img = GetImage (song, arg.Width).Result;
					return img;
				});
				//if (nowPlayingInfo == null)
				//	return;
				//nowPlayingInfo.Artwork = artwork;
				//App.RunOnMainThread (() => MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo);
			} catch (Exception ex) {
				LogManager.Shared.Report (ex);
			}
		}
		async Task<NSImage> GetImage (MediaItemBase item, double imageWidth, NSImage defaultImage = null)
		{
			try {
				if (item == null)
					return defaultImage ?? Images.GetDefaultAlbumArt (imageWidth);

				var image = await item.GetLocalImage (imageWidth);
				if (image != null) {
					return image;
				} else {
					var artUrl = await ArtworkManager.Shared.GetArtwork (item);

					if (string.IsNullOrWhiteSpace (artUrl))
						return defaultImage;
					var bitmap = (await BlobCache.LocalMachine.LoadImageFromUrl (artUrl, desiredWidth: (float)imageWidth));
					return bitmap.ToNative ();
				}
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
			return defaultImage;
		}
	}
}

