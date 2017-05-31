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

namespace MusicPlayer
{
	public class AVMediaPlayer : Player
	{
		AVPlayer player;
		AVPlayerLayer playerLayer;
		NSObject endTimeObserver;
		NSObject timeObserver;
		IDisposable rateObserver;
		bool equalizerApplied;

		public AVMediaPlayer ()
		{
			player = new AVPlayer {
				ActionAtItemEnd = AVPlayerActionAtItemEnd.Pause,
#if __IOS__
				AllowsAirPlayVideo = true,
#endif
				AllowsExternalPlayback = true,
				Volume = Settings.CurrentVolume,

			};

			timeObserver = player.AddPeriodicTimeObserver (new CoreMedia.CMTime (5, 30), null, (time) => OnPlabackTimeChanged (player, time));
			rateObserver = player.AddObserver ("rate", NSKeyValueObservingOptions.New, (change) => OnStateChanged (player));

			playerLayer = AVPlayerLayer.FromPlayer (player);
			endTimeObserver = NSNotificationCenter.DefaultCenter.AddObserver (AVPlayerItem.DidPlayToEndTimeNotification, (notification) => {
				var avplayerItem = notification.Object as AVPlayerItem;
				OnFinished (avplayerItem);
			});
		}

		void OnStateChanged (AVPlayer player)
		{
			State = (Math.Abs (player.Rate) < float.Epsilon) ? PlaybackState.Paused : PlaybackState.Playing;
			StateChanged?.Invoke (State);
		}

		void OnFinished (AVPlayerItem item)
		{
			item?.Asset?.CancelLoading ();
			Finished?.Invoke ();
		}

		void OnPlabackTimeChanged (AVPlayer player, CMTime time)
		{
			PlabackTimeChanged?.Invoke (CurrentTimeSeconds ());
			//Make sure the equalizer is set
			if (!Settings.EnableGaplessPlayback)
				return;
		}

		public override async Task<bool> PrepareData (PlaybackData data)
		{
			AVPlayerItem playerItem = null;
			var playbackData = data.SongPlaybackData;
			if (playbackData.IsLocal || playbackData.CurrentTrack.ServiceType == MusicPlayer.Api.ServiceType.iPod) {
				if (playbackData.Uri == null)
					return false;
				var url = string.IsNullOrWhiteSpace(playbackData?.CurrentTrack?.FileLocation) ? new NSUrl(playbackData.Uri.AbsoluteUri) : NSUrl.FromFilename(playbackData.CurrentTrack.FileLocation);
				playerItem = AVPlayerItem.FromUrl(url);
				await playerItem.WaitStatus();
			} else {
				NSUrlComponents comp =
					new NSUrlComponents(
						NSUrl.FromString(
							$"http://localhost/{playbackData.CurrentTrack.Id}.{data.SongPlaybackData.CurrentTrack.FileExtension}"), false);
				comp.Scheme = "streaming";
				if (comp.Url != null)
				{
					var asset = new AVUrlAsset(comp.Url, new NSDictionary());
					asset.ResourceLoader.SetDelegate(NativeAudioPlayer.LoaderDelegate, DispatchQueue.MainQueue);
					playerItem = new AVPlayerItem(asset);
				}
				if (data.CancelTokenSource.IsCancellationRequested)
					return false;

				await playerItem.WaitStatus();
			}
			player.ReplaceCurrentItemWithPlayerItem (playerItem);
			return true;
		}

		public override float Rate => player.Rate;

		public override float Volume { 
			get => player.Volume;
			set => player.Volume = value;
		}

		public override bool IsPlayerItemValid => player?.CurrentItem != null && player?.CurrentItem.Tracks.Length > 0;

		public override void ApplyEqualizer (Equalizer.Band [] bands)
		{
			throw new NotImplementedException ();
		}

		public override double CurrentTimeSeconds () => player?.CurrentTimeSeconds() ?? 0;

		public override void Dispose ()
		{
			playerLayer?.RemoveFromSuperLayer ();
			playerLayer?.Dispose ();
			NSNotificationCenter.DefaultCenter.RemoveObserver (endTimeObserver);
			rateObserver.Dispose ();
			player.RemoveTimeObserver (timeObserver);
			player.Dispose ();
			StateChanged = null;
		}

		public override double Duration () => player?.Seconds () ?? 0;

		public override void Pause ()
		{
			player.Pause ();
		}

		public override void Play ()
		{
			player.Play ();
		}

		public override async Task<bool> PlaySong (Song song, bool isVideo, bool forcePlay = false)
		{
			throw new NotImplementedException ();
		}

		public override void Seek (double time)
		{
			player.Seek (time);
		}
		public override void ApplyEqualizer ()
		{
			if (player.CurrentItem == null)
				return;
			AVPlayerEqualizer.Shared.ApplyEqualizer (player.CurrentItem);
			equalizerApplied = true;
		}

		public void DisableVideo ()
		{
#if __IOS__
			var tracks = player?.CurrentItem?.Tracks?.Where (x => x.AssetTrack.HasMediaCharacteristic (AVMediaCharacteristic.Visual))?.ToList ();
			if (tracks?.Any () != true)
				return;
			if (PictureInPictureManager.Shared.StartPictureInPicture ())
				return;
			tracks.ForEach (x => {
				if (x.AssetTrack.HasMediaCharacteristic (AVMediaCharacteristic.Visual))
					x.Enabled = false;
			});
#endif
		}

		public void EnableVideo ()
		{
#if __IOS__
			var tracks = player?.CurrentItem?.Tracks?.Where (x => x.AssetTrack.HasMediaCharacteristic (AVMediaCharacteristic.Visual))?.ToList ();
			if (tracks?.Any () != true)
				return;
			PictureInPictureManager.Shared.StopPictureInPicture ();
			tracks.ForEach (x => {
				if (x.AssetTrack.HasMediaCharacteristic (AVMediaCharacteristic.Visual))
					x.Enabled = true;
			});
#endif
		}
	}
}
