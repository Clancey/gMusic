using System;
using MusicPlayer.Managers;
using Foundation;
using MusicPlayer.Models;
using AppKit;
using System.Threading.Tasks;
using System.Reactive.Linq;
using SDWebImage;

namespace MusicPlayer
{
	public class NativeTrackHandler : ManagerBase<NativeTrackHandler>
	{
		public NativeTrackHandler ()
		{
		}

		public void Init()
		{

			NotificationManager.Shared.CurrentSongChanged += (sender, args) => UpdateSong(args.Data);
			NotificationManager.Shared.PlaybackStateChanged += (sender, e) => PlaybackStateChanged (e.Data);
			NSUserNotificationCenter.DefaultUserNotificationCenter.DidActivateNotification += (object sender, UNCDidActivateNotificationEventArgs e) => {
				switch (e.Notification.ActivationType)
				{

        			case NSUserNotificationActivationType.ActionButtonClicked:
					var frontmost = NSWorkspace.SharedWorkspace.FrontmostApplication.BundleIdentifier == NSBundle.MainBundle.BundleIdentifier;
					if(frontmost)
						NSWorkspace.SharedWorkspace.FrontmostApplication.Hide ();
					Console.WriteLine (frontmost);
					PlaybackManager.Shared.NextTrack ();
					break;
				}
			};

			//NotificationManager.Shared.CurrentTrackPositionChanged += (sender, args) => UpdateProgress(args.Data);
		}

		public void UpdateSong (Song song)
		{
			var frontmost = NSWorkspace.SharedWorkspace.FrontmostApplication.BundleIdentifier == NSBundle.MainBundle.BundleIdentifier;
			if (frontmost || PlaybackManager.Shared.NativePlayer.State != PlaybackState.Playing)
				return;
			var notification = CreateNotification (song);
			NSUserNotificationCenter.DefaultUserNotificationCenter.RemoveAllDeliveredNotifications ();
			NSUserNotificationCenter.DefaultUserNotificationCenter.DeliverNotification (notification);
		}

		public void PlaybackStateChanged (PlaybackState state )
		{
			if (state == PlaybackState.Paused || state == PlaybackState.Stopped)
				NSUserNotificationCenter.DefaultUserNotificationCenter.RemoveAllDeliveredNotifications ();
			if (state == PlaybackState.Playing)
				UpdateSong (PlaybackManager.Shared.NativePlayer.CurrentSong);
		}

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
		async void SetImage (NSUserNotification notification, Song song)
		{

		}
		async Task<NSImage> GetImage (MediaItemBase item, NSImage defaultImage = null)
		{
			float imageWidth = 60f;
			try {
				if (defaultImage == null)
					defaultImage = Images.GetDefaultAlbumArt (imageWidth);
				if (item == null)
					return defaultImage;
				
				var image = await item.GetLocalImage (imageWidth);
				if (image != null) {
					return image;
				} else {
					var artUrl = await ArtworkManager.Shared.GetArtwork (item);

					if (string.IsNullOrWhiteSpace (artUrl))
						return defaultImage;

					var tcs = new TaskCompletionSource<NSImage>();
					var imageManager = SDWebImageManager.SharedManager.ImageDownloader.DownloadImage(new NSUrl(artUrl), SDWebImageDownloaderOptions.HighPriority, (receivedSize, expectedSize, u) =>
					{

					}, (i, data, error, finished) =>
					{
						if (error != null)
							tcs.TrySetException(new Exception(error.ToString()));
						else
							tcs.TrySetResult(i);
					});
					return await tcs.Task;

				}
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
			return defaultImage;
		}
	}
}

