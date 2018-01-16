using System;
using System.Threading.Tasks;
using MusicPlayer.Models;
using MusicPlayer.Playback;
using AVFoundation;
using Foundation;
using MusicPlayer.Data;
using CoreMedia;
using MusicPlayer.iOS.Playback;
using System.Linq;
using CoreFoundation;
using System.Web;
using System.Collections.Generic;
using MobileCoreServices;

namespace MusicPlayer
{
	public class AVMediaPlayer : Player
	{
		AVPlayer player;
		public AVPlayerLayer PlayerLayer { get; }
		NSObject endTimeObserver;
		NSObject playerDidStallObserver;
		NSObject timeObserver;
		IDisposable rateObserver;
		bool equalizerApplied;
		NSTimer healthCheckTimer;
		bool isStalled;
		MyResourceLoaderDelegate resourceDelegate;
		public AVMediaPlayer()
		{
			player = new AVPlayer
			{
				ActionAtItemEnd = AVPlayerActionAtItemEnd.Pause,
#if __IOS__
				AllowsAirPlayVideo = true,
#endif
				AllowsExternalPlayback = true,
				Volume = Settings.CurrentVolume,

			};
			resourceDelegate = new MyResourceLoaderDelegate(this);
			timeObserver = player.AddPeriodicTimeObserver(new CoreMedia.CMTime(5, 30), null, (time) => OnPlabackTimeChanged(player, time));
			rateObserver = player.AddObserver("rate", NSKeyValueObservingOptions.New, (change) => OnStateChanged(player));

			PlayerLayer = AVPlayerLayer.FromPlayer(player);
			endTimeObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, (notification) =>
			{
				var avplayerItem = notification?.Object as AVPlayerItem;
				if (avplayerItem == player.CurrentItem)
					OnFinished(avplayerItem);
			});
			playerDidStallObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.PlaybackStalledNotification, (notification) =>
			{
				var avplayerItem = notification?.Object as AVPlayerItem;
				if (avplayerItem == player.CurrentItem)
				{
					Console.WriteLine("Handle STalling");
					isStalled = true;
					State = PlaybackState.Buffering;
				}
			});
		}

		void OnStateChanged(AVPlayer p)
		{
			State = p?.Rate.IsZero() ?? true ? PlaybackState.Paused : PlaybackState.Playing;
			Console.WriteLine($"State Changed {CurrentSongId} - {State}");
		}

		void OnFinished(AVPlayerItem item)
		{
			item?.Asset?.CancelLoading();
			Finished?.Invoke(this);
		}

		void OnPlabackTimeChanged(AVPlayer p, CMTime time)
		{
			PlabackTimeChanged?.Invoke(CurrentTimeSeconds());
			//Make sure the equalizer is set
			if (!Settings.EnableGaplessPlayback)
				return;
		}
		public override bool IsPrepared
		{
			get
			{
				if (IsPlayerItemValid)
					return base.IsPrepared;
				return false;
			}
			set
			{
				base.IsPrepared = value;
			}
		}
		bool shouldBePlaying;
		IDisposable stateObserver;
		protected PlaybackData playbackData;
		public override async Task<bool> PrepareData(PlaybackData data, bool isPlaying)
		{
			playbackData = data;
			stateObserver?.Dispose();
			stateObserver = null;
			CurrentSongId = data.SongId;
			AVPlayerItem playerItem = null;
			var songPlaybackData = data.SongPlaybackData;
			if (songPlaybackData.IsLocal || songPlaybackData.CurrentTrack.ServiceType == MusicPlayer.Api.ServiceType.iPod)
			{
				if (songPlaybackData.Uri == null)
					return false;
				var url = string.IsNullOrWhiteSpace(songPlaybackData?.CurrentTrack?.FileLocation) ? new NSUrl(songPlaybackData.Uri.AbsoluteUri) : NSUrl.FromFilename(songPlaybackData.CurrentTrack.FileLocation);
				playerItem = AVPlayerItem.FromUrl(url);
				await playerItem.WaitStatus();
			}
			else
			{
#if HttpPlayback

				var urlEndodedSongId = HttpUtility.UrlEncode(data.SongId);
				var url = $"http://localhost:{LocalWebServer.Shared.Port}/api/GetMediaStream/Playback?SongId={urlEndodedSongId}";
				playerItem = AVPlayerItem.FromUrl(new NSUrl(url));

#else
				NSUrlComponents comp =
					new NSUrlComponents(
						NSUrl.FromString(
							$"http://localhost/{songPlaybackData.CurrentTrack.Id}.{data.SongPlaybackData.CurrentTrack.FileExtension}"), false);
				comp.Scheme = "streaming";
				if (comp.Url != null)
				{
					var asset = new AVUrlAsset(comp.Url, new NSDictionary());
					asset.ResourceLoader.SetDelegate(resourceDelegate, DispatchQueue.MainQueue);
					playerItem = new AVPlayerItem(asset, (NSString)"duration");
					stateObserver = playerItem.AddObserver("status", NSKeyValueObservingOptions.New, (obj) =>
					{
						if (shouldBePlaying)
							player.Play();
						Console.WriteLine($"Player Status {playerItem.Status}");
					});

				}
#endif

			}
			player.ReplaceCurrentItemWithPlayerItem(playerItem);
			IsPrepared = true;

			return true;
		}
		void StartHealthCheckTimer()
		{

		}
		void StopHealthCheckTimer()
		{
			healthCheckTimer?.Invalidate();
			healthCheckTimer = null;
		}

		void TryToPlayIfStalled()
		{
			if (!isStalled)
				return;
			if (player.CurrentItem.PlaybackLikelyToKeepUp || SecondsBuffered > 5)
			{
				isStalled = false;
				player.Play();
			}
		}

		double SecondsBuffered
		{
			get
			{
				var range = player?.CurrentItem?.LoadedTimeRanges?.LastOrDefault()?.CMTimeRangeValue
								   ;
				if (range == null)
					return 0;
				return (range.Value.Start + range.Value.Duration).Seconds;
			}
		}

		public override float Rate => player.Rate;

		public override float Volume
		{
			get => player.Volume;
			set => player.Volume = value;
		}

		public override bool IsPlayerItemValid => player?.CurrentItem != null && player?.CurrentItem.Tracks.Length > 0;

		public IntPtr Handle => throw new NotImplementedException();

		public override async void ApplyEqualizer(Equalizer.Band[] bands)
		{
			if (IsPlayerItemValid)
				await AVPlayerEqualizer.Shared.ApplyEqualizer(bands, player?.CurrentItem);
		}

		public override double CurrentTimeSeconds() => player?.CurrentTimeSeconds() ?? 0;

		public override void Dispose()
		{
			StopHealthCheckTimer();
			PlayerLayer?.RemoveFromSuperLayer();
			PlayerLayer?.Dispose();
			NSNotificationCenter.DefaultCenter.RemoveObserver(endTimeObserver);
			NSNotificationCenter.DefaultCenter.RemoveObserver(playerDidStallObserver);
			rateObserver.Dispose();
			player.RemoveTimeObserver(timeObserver);
			player.Dispose();
			StateChanged = null;
		}

		public override double Duration() => player?.Seconds() ?? 0;

		public override void Pause()
		{
			shouldBePlaying = false;
			player.Pause();
			StopHealthCheckTimer();
		}

		public override bool Play()
		{
			shouldBePlaying = true;
			player.Play();
			StartHealthCheckTimer();
			return true;
		}


		public override void Stop()
		{
			shouldBePlaying = false;
			player.Pause();
			player.ReplaceCurrentItemWithPlayerItem(null);
			StopHealthCheckTimer();
		}
		public override async Task<bool> PlaySong(Song song, bool isVideo, bool forcePlay = false)
		{
			throw new NotImplementedException();
		}

		public override void Seek(double time)
		{
			player.Seek(time);
		}
		public override void ApplyEqualizer()
		{
			if (player.CurrentItem == null)
				return;
			AVPlayerEqualizer.Shared.ApplyEqualizer(player.CurrentItem);
			equalizerApplied = true;
		}
		public override void UpdateBand(int band, float gain)
		{
			AVPlayerEqualizer.Shared.UpdateBand(band, gain);
		}

		public override void DisableVideo()
		{
#if __IOS__
			var tracks = player?.CurrentItem?.Tracks?.Where(x => x.AssetTrack.HasMediaCharacteristic(AVMediaCharacteristic.Visual))?.ToList();
			if (tracks?.Any() != true)
				return;
			if (PictureInPictureManager.Shared.StartPictureInPicture())
				return;
			tracks?.ForEach(x =>
			{
				if (x?.AssetTrack?.HasMediaCharacteristic(AVMediaCharacteristic.Visual) ?? false)
					x.Enabled = false;
			});
#endif
		}

		public override void EnableVideo()
		{
#if __IOS__
			var tracks = player?.CurrentItem?.Tracks?.Where(x => x.AssetTrack.HasMediaCharacteristic(AVMediaCharacteristic.Visual))?.ToList();
			if (tracks?.Any() != true)
				return;
			PictureInPictureManager.Shared.StopPictureInPicture();
			tracks?.ForEach(x =>
			{
				if (x?.AssetTrack?.HasMediaCharacteristic(AVMediaCharacteristic.Visual) ?? false)
					x.Enabled = true;
			});
#endif
		}



		#region Resource Loader

		internal class MyResourceLoaderDelegate : AVAssetResourceLoaderDelegate
		{
			AVMediaPlayer player;
			public MyResourceLoaderDelegate(AVMediaPlayer player)
			{
				this.player = player;
			}
			public override bool ShouldWaitForLoadingOfRequestedResource(AVAssetResourceLoader resourceLoader, AVAssetResourceLoadingRequest loadingRequest)
			{
				return player.ShouldWaitForLoadingOfRequestedResource(resourceLoader, loadingRequest);
			}
			public override void DidCancelLoadingRequest(AVAssetResourceLoader resourceLoader, AVAssetResourceLoadingRequest loadingRequest)
			{
				player.DidCancelLoadingRequest(resourceLoader, loadingRequest);
			}
		}



		SimpleQueue<AVAssetResourceLoadingRequest> pendingRequests = new SimpleQueue<AVAssetResourceLoadingRequest>();
		public bool ShouldWaitForLoadingOfRequestedResource(AVAssetResourceLoader resourceLoader, AVAssetResourceLoadingRequest loadingRequest)
		{
			try
			{
				pendingRequests.Enqueue(loadingRequest);
				ProcessRequests();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return true;
		}

		Task requestProcessor;
		Task ProcessRequests() => requestProcessor?.IsCompleted ?? true ? (requestProcessor = Task.Run(processRequests)) : requestProcessor;
		async Task processRequests()
		{
			try
			{
				while (pendingRequests.Count > 0)
				{
					var request = pendingRequests.Peek();
					var finished = await ProcessesRequest(request);
					if (finished)
					{
						request.FinishLoading();
						pendingRequests.Remove(request);
					}
					else
					{
						await Task.Delay(500);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}


		async Task<bool> ProcessesRequest(
			AVAssetResourceLoadingRequest loadingRequest)
		{
			if (playbackData == null)
			{
				return false;
			}
			try
			{
				var currentDownloadHelper = playbackData.DownloadHelper;
				var content = loadingRequest.ContentInformationRequest;
				if (content != null)
				{
					content.ByteRangeAccessSupported = true;

					if (string.IsNullOrWhiteSpace(currentDownloadHelper.MimeType))
					{
						var success = await currentDownloadHelper.WaitForMimeType();
					}
					var type = UTType.CreatePreferredIdentifier(UTType.TagClassMIMEType, currentDownloadHelper.MimeType, null);
					content.ContentType = type;
					content.ContentLength = currentDownloadHelper.TotalLength;
				}

				var dataRequest = loadingRequest.DataRequest;

				var startOffset = dataRequest.CurrentOffset > 0 ? dataRequest.CurrentOffset : dataRequest.RequestedOffset;
				if (startOffset == currentDownloadHelper.TotalLength)
					return true;

				if (currentDownloadHelper.CurrentSize < startOffset)
					return false;
				var unreadBytes = currentDownloadHelper.CurrentSize - startOffset;
				var numberOfBytesToRespondWith = Math.Min(dataRequest.RequestedLength, unreadBytes);
				if (numberOfBytesToRespondWith == 0)
					return false;

				//We can read from the stream, but this will cause a double copy of memory
				//var buffer = new byte[numberOfBytesToRespondWith];
				//currentDownloadHelper.Position = startOffset;

				//var read = currentDownloadHelper.Read(buffer, 0, buffer.Length);

				//dataRequest.Respond(NSData.FromArray(buffer.Take(read).ToArray()));
				//

				//Going to just do it directly in NSFoundation to prevent the copying memory to.net and then back again
				var fileHandle = NSFileHandle.OpenReadUrl(NSUrl.FromFilename(currentDownloadHelper.FilePath), out var error);
				fileHandle.SeekToFileOffset((ulong)startOffset);
				var data = fileHandle.ReadDataOfLength((uint)numberOfBytesToRespondWith);
				dataRequest.Respond(data);


				var endOffset = startOffset + dataRequest.RequestedLength;
				var didRespondFully = currentDownloadHelper.CurrentSize >= endOffset;

				return didRespondFully;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return false;
		}

		[Export("resourceLoader:didCancelLoadingRequest:")]
		public void DidCancelLoadingRequest(AVAssetResourceLoader resourceLoader, AVAssetResourceLoadingRequest loadingRequest)
		{
			pendingRequests.Remove(loadingRequest);
		}


#endregion
	}
}
