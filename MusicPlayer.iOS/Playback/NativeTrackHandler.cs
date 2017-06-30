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
using MusicPlayer.iOS;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;

namespace MusicPlayer.Playback
{
	internal class NativeTrackHandler : ManagerBase<NativeTrackHandler>
	{
		public NativeTrackHandler()
		{
			NotificationManager.Shared.CurrentSongChanged += (sender, args) => UpdateSong(args.Data);
			NotificationManager.Shared.CurrentTrackPositionChanged += (sender, args) => UpdateProgress(args.Data);
		}

		public void Init()
		{
			if (string.IsNullOrWhiteSpace(Settings.CurrentSong))
				return;
			UpdateSong(Database.Main.GetObject<Song, TempSong>(Settings.CurrentSong));
		}

		MPNowPlayingInfo nowPlayingInfo;
		MPMediaItemArtwork artwork;

		public void UpdateSong(Song song)
		{
			if (song == null)
				return;
			try
			{
				nowPlayingInfo = new MPNowPlayingInfo
				{
					Title = song?.Name ?? "",
					Artist = song?.Artist ?? "",
					AlbumTitle = song?.Album ?? "",
					Genre = song?.Genre ?? "",
					Artwork = (new MPMediaItemArtwork(Images.GetDefaultAlbumArt(Images.AlbumArtScreenSize))),
				};
				artwork = null;
				FetchArtwork(song);
				App.RunOnMainThread(() => MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		async void FetchArtwork(Song song)
		{
			try
			{
				var art = await song.GetLocalImage(Images.MaxScreenSize);

				if (art == null)
				{
					var url = await ArtworkManager.Shared.GetArtwork(song);
					if (string.IsNullOrWhiteSpace(url))
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
				artwork = new MPMediaItemArtwork(art);
				if (nowPlayingInfo == null)
					return;
				nowPlayingInfo.Artwork = artwork;
				App.RunOnMainThread(() => MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		double lastTime = -1;

		public void UpdateProgress(TrackPosition position)
		{
			try
			{
				if (nowPlayingInfo == null)
					return;
				if (Math.Abs(position.CurrentTime - lastTime) < 1)
					return;
				lastTime = position.CurrentTime;
				if (artwork != null && (int) lastTime%10 == 0)
					nowPlayingInfo.Artwork = artwork;
				nowPlayingInfo.ElapsedPlaybackTime = position.CurrentTime;
				nowPlayingInfo.PlaybackDuration = position.Duration;
				App.RunOnMainThread(() => MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}