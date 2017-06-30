using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using SDWebImage;
using MediaPlayer;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using MusicPlayer.iOS;
using CoreGraphics;

namespace MusicPlayer.Playback
{
	partial class NativeTrackHandler
	{
		void OnInit ()
		{

		}
		MPMediaItemArtwork CreateDefaultArtwork ()
		{
			if (Device.IsIos10) {
				return DefaultResizableArtwork;
			}
			return new MPMediaItemArtwork (Images.GetDefaultAlbumArt (Images.AlbumArtScreenSize));
		}

		void SetAdditionInfo (Song song, MPNowPlayingInfo info)
		{
			if (Device.IsIos10) {
				nowPlayingInfo.MediaType = Settings.CurrentPlaybackIsVideo ? MPNowPlayingInfoMediaType.Video : MPNowPlayingInfoMediaType.Audio;
			}
		}
		void OnSongChanged (Song song)
		{

		}

		async void FetchArtwork (Song song)
		{
			try {
				if (Device.IsIos10) {
					artwork = new MPMediaItemArtwork (new CGSize (Images.MaxScreenSize, Images.MaxScreenSize), (arg) => {
					var img = GetImage (song,arg.Width).Result;
					return img;
				});
				}
				var art = await song.GetLocalImage (Images.MaxScreenSize);

				if (art == null) {
					var url = await ArtworkManager.Shared.GetArtwork (song);
					if (string.IsNullOrWhiteSpace (url))
						return;
					var tcs = new TaskCompletionSource<UIImage>();
					var imageManager = SDWebImageManager.SharedManager.ImageDownloader.DownloadImage(new NSUrl(url), SDWebImageDownloaderOptions.HighPriority, (receivedSize, expectedSize, u) =>
					{

					}, (image, data, error, finished) =>
					{
						if(error != null)
							tcs.TrySetException(new Exception(error.ToString()));
						else
							tcs.TrySetResult(image);
					});
					art = await tcs.Task;
					if (art == null || song.Id != Settings.CurrentSong)
						return;
				}
				artwork = new MPMediaItemArtwork (art);
				if (nowPlayingInfo == null)
					return;
				nowPlayingInfo.Artwork = artwork;
				App.RunOnMainThread (() => MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo);
			} catch (Exception ex) {
				LogManager.Shared.Report (ex);
			}
		}
		async Task<UIImage> GetImage (Song song, double width)
		{
			try {
				var url = await ArtworkManager.Shared.GetArtwork (song);
				if (string.IsNullOrWhiteSpace (url))
					return Images.GetDefaultAlbumArt (width);
				TaskCompletionSource<UIImage> tcs = new TaskCompletionSource<UIImage> ();
				var fether = new HNKNetworkFetcher (new NSUrl (url));
				fether.FetchImage ((image) => { tcs.TrySetResult (image); },
					(error) => { tcs.TrySetException (new Exception (error.ToString ())); });
				var art = await tcs.Task;
				return art;
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return Images.GetDefaultAlbumArt (width);
			}
		}
	}
}
