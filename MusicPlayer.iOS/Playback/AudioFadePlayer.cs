using System;
using CoreAnimation;
using AVFoundation;
using System.Collections.Generic;
using MusicPlayer.Models;
using Foundation;
using CoreMedia;
using System.Threading.Tasks;
using MusicPlayer.Managers;
using MusicPlayer.Playback;
using MusicPlayer.Data;
using MusicPlayer.Models.Scrobbling;
using System.Linq;

namespace  MusicPlayer.iOS.Playback
{
	class AudioFadePlayer
	{
		public NativeAudioPlayer Parent { get; set; }

		Dictionary<string,bool> isVideoDict = new Dictionary<string, bool> ();
		Dictionary<AVPlayer,NSObject> playerTimeObservers = new Dictionary<AVPlayer, NSObject> ();
		Dictionary<AVPlayer,IDisposable> playerRateObservers = new Dictionary<AVPlayer, IDisposable> ();
		NSObject endTimeObserver;

		AVPlayer player1;
		AVPlayer player2;

		AVPlayerLayer player1Layer;
		AVPlayerLayer player2Layer;

		Song player1Song;
		Song player2Song;
		Song nextSong;
		Song fadingToSong;
		bool isUsingFirst = false;

		public AudioFadePlayer ()
		{
			player1 = CreatePlayer ();
			player1Layer = AVPlayerLayer.FromPlayer (player1);
			player2 = CreatePlayer ();
			player2Layer = AVPlayerLayer.FromPlayer (player2);

			endTimeObserver = NSNotificationCenter.DefaultCenter.AddObserver (AVPlayerItem.DidPlayToEndTimeNotification, (notification) => {
				var avplayerItem = notification.Object as AVPlayerItem;
				PlayerFinished(avplayerItem);
			});
			NotificationManager.Shared.VolumeChanged += (s, e) =>
			{
				CurrentPlayer.Volume = Settings.CurrentVolume;
			};
			NotificationManager.Shared.EqualizerChanged += (s, e) =>
			{
				AVPlayerEqualizer.Shared.ApplyEqualizer(CurrentItem);
			};
		}

		public AVPlayer CurrentPlayer {
			get{ return isUsingFirst ? player1 : player2; }
		}
		public Song CurrentSong {
			get{ return isUsingFirst ? player1Song : player2Song; }
		}
		public AVPlayer SecondaryPlayer {
			get{ return isUsingFirst ? player2 : player1; }
		}
		public Song SecondarySong {
			get{ return isUsingFirst ? player2Song : player1Song; }
		}

		public AVPlayerItem CurrentItem {
			get{ return CurrentPlayer?.CurrentItem; }
		}

		public void Play ()
		{
			CurrentPlayer.Play ();
		}

		public void Pause ()
		{
			CurrentPlayer.Pause ();
		}

		public void Seek (double time)
		{
			CurrentPlayer.Seek (time);
		}

		public float Rate {
			get{ return CurrentPlayer.Rate; }
		}
		public float[] AudioLevels = {0, 0};

		public async Task<bool> PrepareSong(Song song, bool isVideo)
		{
			eqApplied = false;
			if (isSongPlaying (song))
				return true;
			else if (isSongPrepared (song)) {
				var player = GetPlayer (song);
				AVPlayerEqualizer.Shared.ApplyEqualizer(player.CurrentItem);
				ResetSecondary ();
				player.Volume = Settings.CurrentVolume;
				return true;
			}
			try{
				var player = GetPlayer (song);
				StopPlayer (player);
				ResetSecondary();
				var data = await Parent.PrepareSong(song,isVideo);
				if(!data.Item1)
					return false;
				player.ReplaceCurrentItemWithPlayerItem(data.Item2);
				AVPlayerEqualizer.Shared.ApplyEqualizer(data.Item2);
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}

		public async Task<bool> PlaySong (Song song, bool isVideo)
		{
			eqApplied = false;
			if (isSongPlaying (song))
				return true;
			else if (isSongPrepared (song) || song == fadingToSong) {
				fadingToSong = null;
				var player = GetPlayer (song);
				AVPlayerEqualizer.Shared.ApplyEqualizer(player.CurrentItem);
				player.Play();
				player.Volume = Settings.CurrentVolume;
				//TODO: Fade out
				//ResetSecondary ();
				return true;
			}
			try{
				var player = GetPlayer (song);
				StopPlayer (player);
				player.Volume = Settings.CurrentVolume;
				ResetSecondary();
				var data = await Parent.PrepareSong(song,isVideo);
				if(!data.Item1)
					return false;
				player.ReplaceCurrentItemWithPlayerItem(data.Item2);
				AVPlayerEqualizer.Shared.ApplyEqualizer(data.Item2);
				player.Play();
				return true;
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}
		void PlayerFinished(AVPlayerItem item)
		{
			if (CurrentItem == item)
				SecondaryPlayer.Play ();
			if (player1.CurrentItem == item)
				ResetPlayer1 ();
			else if (player2.CurrentItem == item)
				ResetPlayer2 ();
		}
		void ResetSecondary()
		{
			if (isUsingFirst) {
				ResetPlayer2 ();
			} else {
				ResetPlayer1 ();
			}
		}
		void ResetPlayer1()
		{
			StopPlayer (player1);
			player1Song = null;
			player1?.CurrentItem?.Asset?.CancelLoading ();
			ResetPlayer (player1);
			player1 = CreatePlayer ();
			player1Layer = AVPlayerLayer.FromPlayer (player1);
		}
		void ResetPlayer2()
		{
			StopPlayer (player2);
			player2Song = null;
			player2?.CurrentItem?.Asset?.CancelLoading ();
			ResetPlayer (player2);
			player2 = CreatePlayer ();
			player2Layer = AVPlayerLayer.FromPlayer (player2);
		}

		void StopPlayer(AVPlayer player)
		{
			//TODO: fade
			player.Pause ();
		}

		AVPlayer GetPlayer(Song song)
		{
			if (song == player1Song) {
				isUsingFirst = true;
				VideoLayer.VideoLayer = player1Layer;
				return player1;
			}

			if (song == player2Song) {
				isUsingFirst = false;
				VideoLayer.VideoLayer = player2Layer;
				return player2;
			}
			isUsingFirst = !isUsingFirst;

			if (isUsingFirst) {
				player1Song = song;
				VideoLayer.VideoLayer = player1Layer;
			}
			else {
				player2Song = song;
				VideoLayer.VideoLayer = player2Layer;
			}
			
			return CurrentPlayer;
		}


		AVPlayer GetSecondaryPlayer(Song song)
		{
			if (song == player1Song) {
				return player1;
			}

			if (song == player2Song) {
				return player2;
			}

			if (isUsingFirst)
				player2Song = song;
			else
				player1Song = song;

			return SecondaryPlayer;
		}

		bool isSongPlaying (Song song)
		{
			if (song == player1Song)
				return isUsingFirst ? player1.Rate != 0 : false;
			if (song == player2Song)
				return isUsingFirst ? false : player2.Rate != 0;
			return false;
		}


		bool isSongPrepared (Song song)
		{
			if (song == player1Song)
				return !isUsingFirst;
			if (song == player2Song)
				return isUsingFirst;
			return false;
		}

		public void Queue (Track track)
		{
			var song = Database.Main.GetObject<Song,TempSong> (track.SongId);
			nextSong = song;
		}

		void StateChanged (AVPlayer player)
		{
			if (player == CurrentPlayer)
				Parent.State = player.Rate == 0 ? PlaybackState.Paused : PlaybackState.Playing;
		}

		float lastAudioCheck = 0;
		bool eqApplied;
		void PlabackTimeChanged (AVPlayer player, CMTime time)
		{
			if (player != CurrentPlayer)
				return;
			if (!eqApplied)
			{
				AVPlayerEqualizer.Shared.ApplyEqualizer(player.CurrentItem);
				eqApplied = true;
			}
			if (!Settings.EnableGaplessPlayback)
				return;
			var current = player.CurrentTimeSeconds ();
			var duration = player.Seconds ();
			if (current < 1 || duration < 1)
				return;
			var remaining = duration - current;
			//Console.WriteLine ("Time Remaining: {0}",remaining);
			var avgAudio = AudioLevels.Max();
			if (remaining < 3 && avgAudio < .01 && lastAudioCheck < .01) {
				StartNext (current);
			} else if (remaining < .75) {
				StartNext (current);
			}
			else if (remaining < 15) {
				Console.WriteLine (avgAudio);
				PrepareNextSong ();
			}
			lastAudioCheck = avgAudio;
		}
		async void PrepareNextSong()
		{
			if (nextSong == null || nextSong == SecondarySong)
				return;
			var player = GetSecondaryPlayer (nextSong);
			bool isVideo;
			isVideoDict.TryGetValue (nextSong.Id, out isVideo);
			var data = await Parent.PrepareSong(nextSong,isVideo);
			if (player == CurrentPlayer)
				return;
			if(!data.Item1)
				return;
			player.ReplaceCurrentItemWithPlayerItem(data.Item2);

		}
		async void StartNext(double currentPosition){
			if (nextSong == CurrentSong)
				return;
			var playbackEndEvent = new PlaybackEndedEvent (CurrentSong) {
				TrackId = Settings.CurrentTrackId,
				Context = Settings.CurrentPlaybackContext,
				Position = currentPosition,
				Duration = Duration(),
				Reason = ScrobbleManager.PlaybackEndedReason.PlaybackEnded,
			};
			fadingToSong = SecondarySong;
			SecondaryPlayer.Play();
			isUsingFirst = !isUsingFirst;

			ScrobbleManager.Shared.PlaybackEnded(playbackEndEvent);

			#pragma warning disable 4014
			eqApplied = false;
			PlaybackManager.Shared.NextTrackWithoutPause();
			Parent.CleanupSong(CurrentSong);
		}

		public void DisableVideo ()
		{
			#if __IOS__
			var tracks = CurrentPlayer?.CurrentItem?.Tracks?.Where(x=> x.AssetTrack.HasMediaCharacteristic(AVMediaCharacteristic.Visual))?.ToList();
			if (tracks?.Any() != true)
				return;
			if (PictureInPictureManager.Shared.StartPictureInPicture())
				return;
			tracks.ForEach(x => {
				if(x.AssetTrack.HasMediaCharacteristic(AVMediaCharacteristic.Visual))
					x.Enabled = false;
			});
			#endif
		}

		public void EnableVideo ()
		{
			#if __IOS__
			var tracks = CurrentPlayer?.CurrentItem?.Tracks?.Where(x => x.AssetTrack.HasMediaCharacteristic(AVMediaCharacteristic.Visual))?.ToList();
			if (tracks?.Any() != true)
				return;
			PictureInPictureManager.Shared.StopPictureInPicture();
			tracks.ForEach(x => {
				if(x.AssetTrack.HasMediaCharacteristic(AVMediaCharacteristic.Visual))
					x.Enabled = true;
			});
			#endif
		}

		public double CurrentTimeSeconds ()
		{
			var pos = CurrentPlayer.CurrentTimeSeconds();
			//Console.WriteLine (pos);
			return pos;
		}

		public double Duration ()
		{
			var dur = CurrentPlayer?.Seconds () ?? 0;
			if (dur == 0) {
				MusicPlayer.Playback.NativeAudioPlayer.PlaybackData data;
				if (Parent.CurrentData.TryGetValue (CurrentSong?.Id ?? "", out data))
					return data?.SongPlaybackData?.CurrentTrack?.Duration ?? 0;
			}
				
			return dur;
		}

		void ResetPlayer(AVPlayer player)
		{
			var timeObserver = playerTimeObservers [player];
			player.RemoveTimeObserver (timeObserver);
			playerTimeObservers.Remove (player);
			timeObserver.Dispose ();

			var rateObserver = playerRateObservers [player];
			playerRateObservers.Remove (player);
			rateObserver.Dispose ();
			player.Dispose ();
		}

		AVPlayer CreatePlayer ()
		{
			var player = new AVPlayer {
				ActionAtItemEnd = AVPlayerActionAtItemEnd.Pause,
				#if __IOS__
				AllowsAirPlayVideo = true,
				#endif
				AllowsExternalPlayback = true,
				Volume = Settings.CurrentVolume,

			};
			playerTimeObservers [player] = player.AddPeriodicTimeObserver (new CMTime (5, 30), null, (time) => PlabackTimeChanged (player, time));
			playerRateObservers [player] = player.AddObserver ("rate", NSKeyValueObservingOptions.New, (change) => StateChanged (player));
			return player;
		}

		public CustomVideoLayer VideoLayer { get; } = new CustomVideoLayer();

		public class CustomVideoLayer : CALayer
		{
			public event Action<AVPlayerLayer> VideoLayerChanged;
			AVPlayerLayer videoLayer;

			public AVPlayerLayer VideoLayer {
				get {
					return videoLayer;
				}
				set {
					if (videoLayer == value)
						return;
					videoLayer?.RemoveFromSuperLayer ();
					AddSublayer (videoLayer = value);
					VideoLayerChanged?.InvokeOnMainThread (value);
				}
			}

			public override void LayoutSublayers ()
			{
				base.LayoutSublayers ();
				if (videoLayer == null)
					return;
				videoLayer.Frame = Bounds;
			}

		}
	}
}

