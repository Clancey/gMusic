using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AVFoundation;
using CoreFoundation;
using Foundation;
using Metal;
using MusicPlayer.Data;
using MusicPlayer.iOS;
using MusicPlayer.iOS.Playback;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using MusicPlayer.Models.Scrobbling;
using UIKit;
using CoreAnimation;

namespace MusicPlayer.Playback
{
	internal partial class NativeAudioPlayer : BaseModel
	{
		public static NativeAudioPlayer Shared {get;set;}
		public readonly AudioFadePlayer player;
		readonly AVAudioPlayer silentPlayer;
		IDisposable observer;

		NSTimer timer;
        PlaybackState lastState;
        DateTime lastInterupt;

		public CustomVideoLayer VideoLayer { get; } = new CustomVideoLayer ();
		public NativeAudioPlayer()
		{
			Shared = this;
            timer = NSTimer.CreateRepeatingScheduledTimer(2,CheckPlaybackStatus);
			NSError error;
			#if __IOS__
			AVAudioSession.SharedInstance().SetCategory(AVAudioSession.CategoryPlayback, out error);
			#endif
			LoaderDelegate.Parent = this;

			player = new AudioFadePlayer
			{
				Parent = this,
				Finished = (obj) => {
					finishedPlaying(player.CurrentSong);
				},
			};

			#if __IOS__
			silentPlayer = new AVAudioPlayer(NSBundle.MainBundle.GetUrlForResource("empty","mp3"),"mp3", out error) {
				NumberOfLoops = -1,
			};

			PictureInPictureManager.Shared.Setup(VideoLayer);
//			observer = player.AddObserver("rate", NSKeyValueObservingOptions.New, (change) =>
//			{
//				Console.WriteLine("Playback state changed: {0}", player.Rate);
//				if (player.Rate == 0)
//				{
//					State = PlaybackState.Paused;
//					Console.WriteLine("AVPlayer Paused");
//				}
//				else
//				{
//					State = PlaybackState.Playing;
//					Console.WriteLine("AVPlayer Playing");
//				}
//			});
//			NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, (notification) =>
//			{
//				finishedPlaying(currentSong);
//				player.ReplaceCurrentItemWithPlayerItem(new AVPlayerItem(new NSUrl("")));
//			});
			AVAudioSession.Notifications.ObserveInterruption((sender, args) =>
				{
					if (args.InterruptionType == AVAudioSessionInterruptionType.Began)
					{
						lastState = State;
						if(State == PlaybackState.Playing)
							Pause();
						else
							State = PlaybackState.Stopped;
						lastInterupt = DateTime.Now;
					}
					else if(args.InterruptionType == AVAudioSessionInterruptionType.Ended){
						State = lastState;
						if(args.Option == AVAudioSessionInterruptionOptions.ShouldResume && lastState == PlaybackState.Playing)
							Play();
						else
							Pause();
					}
					NotificationManager.Shared.ProcPlaybackStateChanged(State);
					Console.WriteLine("Interupted,: {2} -  {0} , {1}", args.InterruptionType, args.Option,(DateTime.Now - lastInterupt).TotalSeconds);
				});

			AVAudioSession.Notifications.ObserveRouteChange((sender, args) =>
				{
					if(args.Reason == AVAudioSessionRouteChangeReason.OldDeviceUnavailable)
						Pause();
					Console.WriteLine("Route Changed");
				});
			#endif
		}

		public void UpdateBand (int band, float gain)
		{
			player?.UpdateBand (band, gain);
		}

		double lastDurration;
		DateTime playbackStarted = DateTime.Now;
		Task checkPlaybackTask;
	   void CheckPlaybackStatus(NSTimer timer)
	    {
			try
			{
				if (CurrentSong == null || (!(player.CurrentPlayer?.IsPlayerItemValid ?? false) && (DateTime.Now - playbackStarted).TotalSeconds < 10))
					return;
				if (Duration > 0 && Math.Abs(lastDurration) < Double.Epsilon)
				{
					Seek(Settings.CurrentPlaybackPercent);
					lastDurration = Duration;
				}
				if (State == PlaybackState.Paused || State == PlaybackState.Stopped)
					return;
				var time = player.CurrentTimeSeconds();
				if (player.Rate > 0 && Math.Abs(time - lastSeconds) > Double.Epsilon)
				{
					lastSeconds = time;
					return;
				}
				if (checkPlaybackTask != null && !checkPlaybackTask.IsCompleted)
					return;
				checkPlaybackTask = Task.Run(async () =>
				{
					if (player.Rate > 0)
					{
						await PrepareSong(CurrentSong, isVideo);
						await player.PlaySong(CurrentSong, isVideo, true);
						if (time > 0)
							player.Seek(time);

					}
					else
						Play();
				});
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
	    }
		public async Task PrepareFirstTrack(Song song, bool isVideo)
		{
			if (!(await PrepareSong (song, isVideo)).Item1)
				return;
			await player.PrepareSong (song, isVideo);
		}

		public double CurrentTime => player.CurrentTimeSeconds();

		public double Duration => player.Duration();

		public float Progress => (float) (Math.Abs(Duration) < Double.Epsilon ? 0 : CurrentTime/Duration);

		public void DisableVideo()
		{
			player.DisableVideo();
		}

		public void EnableVideo()
		{
			player.EnableVideo();
		}
		public void Pause()
		{
			#if __IOS__
			silentPlayer.Pause();
			#endif
			player.Pause();
			State = PlaybackState.Paused;
		}

		public async void Play()
		{
			#if __IOS__
			silentPlayer.Play();
			#endif
			if ((State == PlaybackState.Playing && player.Rate > 0 && !player.IsPlayerItemValid) || State == PlaybackState.Buffering || CurrentSong == null)
			{
				return;
			}
			if (player.IsPlayerItemValid )
			{
				ScrobbleManager.Shared.SetNowPlaying(CurrentSong, Settings.CurrentTrackId);
				player.Play();
				Seek(Settings.CurrentPlaybackPercent);
			}
			else
				await PlaySong(CurrentSong, isVideo);
		}

		public void QueueTrack(Track track)
		{
			player.Queue (track);
		}

		public Song CurrentSong
		{
			get { return currentSong; }
			set { ProcPropertyChanged(ref currentSong, value); }
		}

		public PlaybackState State
		{
			get { return state;	}
			set {
				if(ProcPropertyChanged(ref state, value))
					NotificationManager.Shared.ProcPlaybackStateChanged(state);
			}
		}

		public void ApplyEqualizer (Equalizer.Band [] bands)
		{
			player.ApplyEqualizer (bands);
		}
		public float[] AudioLevels 
		{
			get{return player.AudioLevels;}
			set{ player.AudioLevels = value; }
		}


		void finishedPlaying(Song song)
		{
			ScrobbleManager.Shared.PlaybackEnded(new PlaybackEndedEvent(song)
			{
				TrackId = Settings.CurrentTrackId,
				Context = Settings.CurrentPlaybackContext,
				Position = this.CurrentTime,
				Reason = ScrobbleManager.PlaybackEndedReason.PlaybackEnded,
			});
			CleanupSong(song);
			State = PlaybackState.Stopped;
#pragma warning disable 4014
			PlaybackManager.Shared.NextTrack();
#pragma warning restore 4014
		}

		void TryPlayAgain(string songId)
		{
			var song = Database.Main.GetObject<Song, TempSong>(songId);
			CleanupSong(song);
			PlaySong(song,isVideo);
		}

		public void CleanupSong(Song song)
		{
			lastSeconds = -1;
			if (string.IsNullOrWhiteSpace(song?.Id))
				return;
			var data = GetPlaybackData(song.Id, false);
			if (data == null)
				return;
			data.CancelTokenSource.Cancel();
			data = null;
			CurrentData.Remove(song.Id);
			var key = SongIdTracks.FirstOrDefault(x => x.Value == song.Id);
			if (key.Key != null)
				SongIdTracks.Remove(key.Key);
		}
		AVPlayerItem currentPlayerItem;
        bool isVideo;

		public async Task PlaySong(Song song, bool playVideo = false)
		{
			playbackStarted = DateTime.Now;
			#if __IOS__
			if(!playVideo)
				PictureInPictureManager.Shared.StopPictureInPicture();
			#endif
			player.Pause();
			CleanupSong(CurrentSong);
			CurrentSong = song;
			Settings.CurrentTrackId = "";
			Settings.CurrentPlaybackIsVideo = playVideo;
			NotificationManager.Shared.ProcCurrentTrackPositionChanged(new TrackPosition
			{
				CurrentTime = 0,
				Duration = 0,
			});
			if (song == null)
			{
				State = PlaybackState.Stopped;
				return;
			}
			State = PlaybackState.Buffering;
			#if __IOS__
			silentPlayer.Play();
			#endif

			var success = await player.PlaySong(song, playVideo);
			ScrobbleManager.Shared.SetNowPlaying(song, Settings.CurrentTrackId);
			if (CurrentSong != song)
				return;
			if (!success)
			{
				this.State = PlaybackState.Stopped;
				PlaybackManager.Shared.NextTrack();
			}
		}

		internal static readonly MyResourceLoaderDelegate LoaderDelegate = new MyResourceLoaderDelegate();
		PlaybackState state;
		Song currentSong;

		public bool ShouldWaitForLoadingOfRequestedResource(AVAssetResourceLoader resourceLoader,
			AVAssetResourceLoadingRequest loadingRequest)
		{
			try
			{
				var id = loadingRequest.Request.Url.Path.Trim('/');
				if (id.Contains('.'))
					id = id.Substring(0, id.IndexOf('.'));
				var songId = SongIdTracks[id];
				var data = GetPlaybackData(songId, false);
				Task.Run(() => ProcessesRequest(resourceLoader, loadingRequest, data));
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return false;
		}
		public void SeekTime (double time)
		{
			player.Seek (time);
		}
		public void Seek(float percent)
		{
			var seconds = percent*Duration;
			if(double.IsNaN(seconds))
				seconds = 0;
			player.Seek(seconds);

			NotificationManager.Shared.ProcCurrentTrackPositionChanged(new TrackPosition
			{
				CurrentTime = seconds,
				Duration = Duration,
			});
		}

		public void DidCancelLoadingRequest(AVAssetResourceLoader resourceLoader, AVAssetResourceLoadingRequest loadingRequest)
		{
		}

		async void ProcessesRequest(AVAssetResourceLoader resourceLoader,
			AVAssetResourceLoadingRequest loadingRequest, PlaybackData data)
		{
			if (data == null)
			{
				loadingRequest.FinishLoading();
				return;
			}
			try
			{
				var currentDownloadHelper = data.DownloadHelper;
				var content = loadingRequest.ContentInformationRequest;
				if (content != null)
				{
					content.ByteRangeAccessSupported = true;

					if (string.IsNullOrWhiteSpace(currentDownloadHelper.MimeType))
					{
						var success = await currentDownloadHelper.WaitForMimeType();
					}
					content.ContentType = currentDownloadHelper.MimeType;
					content.ContentLength = currentDownloadHelper.TotalLength;
				}

				var dataRequest = loadingRequest.DataRequest;

				Console.WriteLine(
					$"Data Request: {dataRequest.RequestedOffset} - {dataRequest.RequestedLength} : {dataRequest.CurrentOffset} - {currentDownloadHelper.TotalLength}");
				var allData = Device.IsIos9 && dataRequest.RequestsAllDataToEndOfResource;
                long exspected = allData ? Math.Max(dataRequest.RequestedLength,currentDownloadHelper.TotalLength) : dataRequest.RequestedLength;
				int sent = 0;
				var bufer = new byte[exspected];
				lock (currentDownloadHelper)
				{
					while (sent < exspected)
					{
						if (data.CancelTokenSource.IsCancellationRequested)
						{
							loadingRequest.FinishLoading();
							return;
						}
						var startOffset = dataRequest.CurrentOffset != 0 ? dataRequest.CurrentOffset : dataRequest.RequestedOffset;
						var remaining = exspected - sent;
						currentDownloadHelper.Position = startOffset;
						if (loadingRequest.IsCancelled)
							break;
						var read = currentDownloadHelper.Read(bufer, 0, (int) remaining);
						var sendBuffer = bufer.Take(read).ToArray();
						dataRequest.Respond(NSData.FromArray(sendBuffer));
						sent += read;
						if(sent + startOffset >= currentDownloadHelper.TotalLength)
							break;
					}
				}
				if (!loadingRequest.IsCancelled){
					loadingRequest.FinishLoading();
					if(NativeAudioPlayer.Shared.State == PlaybackState.Buffering || NativeAudioPlayer.Shared.State == PlaybackState.Playing)
						NativeAudioPlayer.Shared.player.CurrentPlayer.Play();
				}
			}
			catch (Exception ex)
			{
				loadingRequest.FinishLoadingWithError(new NSError((NSString) ex.Message, 0));
				TryPlayAgain(data.SongId);
				Console.WriteLine("***************** ERROR ******************");
				Console.WriteLine("Error in Resouce Loader. Trying Again\r\n {0}", ex);
				Console.WriteLine("*******************************************");
			}
		}

		internal class MyResourceLoaderDelegate : AVAssetResourceLoaderDelegate
		{
			public NativeAudioPlayer Parent { get; set; }

			public override bool ShouldWaitForLoadingOfRequestedResource(AVAssetResourceLoader resourceLoader,
				AVAssetResourceLoadingRequest loadingRequest)
			{
				return Parent.ShouldWaitForLoadingOfRequestedResource(resourceLoader, loadingRequest);
			}

			public override void DidCancelLoadingRequest(AVAssetResourceLoader resourceLoader,
				AVAssetResourceLoadingRequest loadingRequest)
			{
				Parent.DidCancelLoadingRequest(resourceLoader, loadingRequest);
				// base.DidCancelLoadingRequest(resourceLoader, loadingRequest);
			}

			public override bool ShouldWaitForRenewalOfRequestedResource(AVAssetResourceLoader resourceLoader,
				AVAssetResourceRenewalRequest renewalRequest)
			{
				return base.ShouldWaitForRenewalOfRequestedResource(resourceLoader, renewalRequest);
			}

			public override bool ShouldWaitForResponseToAuthenticationChallenge(AVAssetResourceLoader resourceLoader,
				NSUrlAuthenticationChallenge authenticationChallenge)
			{
				return base.ShouldWaitForResponseToAuthenticationChallenge(resourceLoader, authenticationChallenge);
			}
		}

		public static async Task<bool> VerifyMp3(string path, bool deleteBadFile = false)
		{
			if (!File.Exists (path)) {
				LogManager.Shared.Log("File does not exist");
				return false;
			}
			var asset = AVAsset.FromUrl(NSUrl.FromFilename(path));
			await asset.LoadValuesTaskAsync(new[]{ "duration" ,"tracks"});
			if (asset.Duration.Seconds > 0)
			{
				asset.Dispose();
				return true;
			}
			LogManager.Shared.Log("File is too short",key:"File Size",value:new FileInfo(path).Length.ToString());
			if (deleteBadFile)
				System.IO.File.Delete(path);
			return false;
		}
	}
}