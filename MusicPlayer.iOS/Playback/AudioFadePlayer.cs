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
	class AudioFadePlayer : Player
	{
		public NativeAudioPlayer Parent { get; set; }

		Dictionary<string,bool> isVideoDict = new Dictionary<string, bool> ();
		Dictionary<Player,NSObject> playerTimeObservers = new Dictionary<Player, NSObject> ();
		Dictionary<Player,IDisposable> playerRateObservers = new Dictionary<Player, IDisposable> ();
		NSObject endTimeObserver;

		Player player1;
		Player player2;

		Song player1Song;
		Song player2Song;
		Song nextSong;
		Song fadingToSong;
		bool isUsingFirst = false;

		public AudioFadePlayer ()
		{
			player1 = CreatePlayer ();
			player2 = CreatePlayer ();

			NotificationManager.Shared.VolumeChanged += (s, e) =>
			{
				CurrentPlayer.Volume = Settings.CurrentVolume;
			};
			NotificationManager.Shared.EqualizerChanged += (s, e) =>
			{
				CurrentPlayer.ApplyEqualizer ();
			};
		}

		public Player CurrentPlayer {
			get{ return isUsingFirst ? player1 : player2; }
		}
		public Song CurrentSong {
			get{ return isUsingFirst ? player1Song : player2Song; }
		}
		public Player SecondaryPlayer {
			get{ return isUsingFirst ? player2 : player1; }
		}
		public Song SecondarySong {
			get{ return isUsingFirst ? player2Song : player1Song; }
		}

		public override void Play ()
		{
			CurrentPlayer.Play ();
		}

		public override void Pause ()
		{
			CurrentPlayer.Pause ();
		}

		public override void Seek (double time)
		{
			CurrentPlayer.Seek (time);
		}

		public override float Rate {
			get{ return CurrentPlayer.Rate; }
		}
		public override float [] AudioLevels {
			get => CurrentPlayer.AudioLevels;
			set => CurrentPlayer.AudioLevels = value;
		}

		public async Task<bool> PrepareSong(Song song, bool isVideo)
		{
			eqApplied = false;
			if (isSongPlaying (song))
				return true;
			else if (isSongPrepared (song)) {
				var player = GetPlayer (song);
				player.ApplyEqualizer ();
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
				//TODO: FIXME:
				var s = await player.PrepareData (data.Item2);
				player.ApplyEqualizer ();
				return s;
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}

		public override async Task<bool> PlaySong (Song song, bool isVideo, bool forcePlay = false)
		{
			eqApplied = false;
			if (!forcePlay && isSongPlaying (song))
				return true;
			else if (isSongPrepared (song) || song == fadingToSong) {
				fadingToSong = null;
				var player = GetPlayer (song);
				player.ApplyEqualizer ();
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
				//TODO:FIXME:
				var s = await player.PrepareData(data.Item2);
				if (!s)
					return false;
				player.ApplyEqualizer ();
				player.Play ();
				return true;
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}

		public override Task<bool> PrepareData (PlaybackData playbackData)
		{
			return CurrentPlayer.PrepareData (playbackData);
		}

		//void PlayerFinished(AVPlayerItem item)
		//{
		//	if (CurrentItem == item)
		//		SecondaryPlayer.Play ();
		//	if (player1.CurrentItem == item)
		//		ResetPlayer1 ();
		//	else if (player2.CurrentItem == item)
		//		ResetPlayer2 ();
		//}

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
			player1.Dispose ();
			player1 = CreatePlayer ();
		}
		void ResetPlayer2()
		{
			StopPlayer (player2);
			player2Song = null;
			player2.Dispose ();
			player2 = CreatePlayer ();
		}

		void StopPlayer(Player player)
		{
			//TODO: fade
			player.Pause ();
		}

		Player GetPlayer(Song song)
		{
			if (song == player1Song) {
				isUsingFirst = true;
				//VideoLayer.VideoLayer = player1Layer;
				return player1;
			}

			if (song == player2Song) {
				isUsingFirst = false;
				//VideoLayer.VideoLayer = player2Layer;
				return player2;
			}
			isUsingFirst = !isUsingFirst;

			if (isUsingFirst) {
				player1Song = song;
				//VideoLayer.VideoLayer = player1Layer;
			}
			else {
				player2Song = song;
				//VideoLayer.VideoLayer = player2Layer;
			}
			
			return CurrentPlayer;
		}


		Player GetSecondaryPlayer(Song song)
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


		float lastAudioCheck = 0;
		bool eqApplied;
		void OnPlabackTimeChanged (Player player)
		{
			if (player != CurrentPlayer || State != PlaybackState.Playing)
				return;
			if (!eqApplied)
			{
				player.ApplyEqualizer ();
				eqApplied = true;
			}
			if (!Settings.EnableGaplessPlayback)
				return;
			var current = player.CurrentTimeSeconds ();
			var duration = player.Duration ();
			if (current < 1 || duration < 1)
				return;
			var remaining = duration - current;
			//Console.WriteLine ("Time Remaining: {0}",remaining);
			var avgAudio = AudioLevels?.Max() ?? 1;
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
			await player.PrepareData (data.Item2);

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
			var avPlayer = CurrentPlayer as AVMediaPlayer;
			avPlayer?.DisableVideo ();
		}

		public void EnableVideo ()
		{

			var avPlayer = CurrentPlayer as AVMediaPlayer;
			avPlayer?.EnableVideo ();
		}

		public override  double CurrentTimeSeconds ()
		{
			var pos = CurrentPlayer.CurrentTimeSeconds();
			//Console.WriteLine (pos);
			return pos;
		}

		public override double Duration ()
		{
			var dur = CurrentPlayer?.Duration () ?? 0;
			if (dur == 0) {
				PlaybackData data;
				if (Parent.CurrentData.TryGetValue (CurrentSong?.Id ?? "", out data))
					return data?.SongPlaybackData?.CurrentTrack?.Duration ?? 0;
			}
				
			return dur;
		}

		Player CreatePlayer ()
		{
			var player = new AVMediaPlayer ();

			player.StateChanged = (state) => {
				StateChanged?.Invoke (state);
				Parent.State = state;
			};

			player.PlabackTimeChanged = (time) => {
				PlabackTimeChanged?.Invoke (time);
				OnPlabackTimeChanged (player);
			};

			return player;
		}

		public override void Dispose ()
		{
			player1.Dispose ();
			player2.Dispose ();
		}

		public override void ApplyEqualizer (Equalizer.Band [] bands)
		{
			CurrentPlayer.ApplyEqualizer (bands);
		}

		public override void ApplyEqualizer ()
		{
			CurrentPlayer.ApplyEqualizer ();
		}

		public CustomVideoLayer VideoLayer { get; } = new CustomVideoLayer ();
		public override float Volume { get => CurrentPlayer.Volume; set => CurrentPlayer.Volume = value; }
		public override bool IsPlayerItemValid => CurrentPlayer.IsPlayerItemValid;

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

