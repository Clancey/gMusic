﻿using System;
using System.Collections.Generic;
using MusicPlayer.Models;
using System.Threading.Tasks;
using MusicPlayer.Managers;
using MusicPlayer.Playback;
using MusicPlayer.Data;
using MusicPlayer.Models.Scrobbling;
using System.Linq;

namespace MusicPlayer.iOS.Playback
{
	class AudioFadePlayer : Player
	{
		public NativeAudioPlayer Parent { get; set; }

		FixedSizeDictionary<string, bool> isVideoDict = new FixedSizeDictionary<string, bool> (4);
		FixedSizeDictionary<string, Player> playerQueue = new FixedSizeDictionary<string, Player> (2) {
			OnDequeue = (item) => {
				item.Value?.Pause ();
				item.Value?.Dispose ();
			}
		};

		Song currentSong;
		Song nextSong;
		Song fadingToSong;
		bool isUsingFirst = false;

		public AudioFadePlayer ()
		{

			NotificationManager.Shared.VolumeChanged += (s, e) => {
				CurrentPlayer.Volume = Settings.CurrentVolume;
			};
			NotificationManager.Shared.EqualizerChanged += (s, e) => {
				CurrentPlayer.ApplyEqualizer ();
			};
		}

		public Player CurrentPlayer => GetPlayer (currentSong);
		public Song CurrentSong => currentSong;
		public Player SecondaryPlayer => GetPlayer (nextSong);

		public override void Play ()
		{
			CurrentPlayer?.Play ();
		}

		public override void Pause ()
		{
			CurrentPlayer?.Pause ();
		}

		public void StopAllOthers (Song song)
		{
			var first = playerQueue.FirstOrDefault ();
			if (first.Key != song.Id && !string.IsNullOrWhiteSpace (first.Key)) {
				playerQueue.Remove (first.Key);
			}
			var items = playerQueue.ToList();
			foreach (var item in items) {
				if (item.Key != song.Id) {
					item.Value.Seek (0);
					item.Value.Pause ();
				}
			}
		}
		public override void Seek (double time)
		{
			CurrentPlayer?.Seek (time);
		}

		public override float Rate {
			get { return CurrentPlayer.Rate; }
		}
		public override float [] AudioLevels {
			get => CurrentPlayer.AudioLevels;
			set => CurrentPlayer.AudioLevels = value;
		}

		public async Task<bool> PrepareSong (Song song, bool isVideo)
		{
			try {
				if (currentSong == null)
					currentSong = song;
				eqApplied = false;
				isVideoDict [song.Id] = isVideo;
				var player = GetPlayer (song,true);
				if (player.IsPrepared || player.State == PlaybackState.Playing)
					return true;
				var data = await Parent.PrepareSong (song, isVideo);
				if (!data.Item1)
					return false;
				var s = await player.PrepareData (data.Item2);
				player.ApplyEqualizer ();
				return s;

			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}

		public Player GetPlayer (Song song, bool create = false)
		{
			if (string.IsNullOrWhiteSpace (song?.Id))
				return null;
			var player = playerQueue [song.Id] ?? (create  ? (playerQueue [song.Id] = CreatePlayer (song)) : null);
			return player;
		}

		public override async Task<bool> PlaySong (Song song, bool isVideo, bool forcePlay = false)
		{
			if (!isVideo && song.MediaTypes.Length == 1 && song.MediaTypes[0] == MediaType.Video)
				isVideo = true;
			Settings.CurrentPlaybackIsVideo = isVideo;
			isVideoDict [song.Id] = isVideo;
			StopAllOthers (song);
			eqApplied = false;
			currentSong = song;
			if (!forcePlay && isSongPlaying (song)) {
				var player = GetPlayer (song);
				State = player.State;
				SetVideo (player);
				return true;
			}
			else if (isSongPrepared (song)) {
				fadingToSong = null;
				var player = GetPlayer (song,true);
				player.ApplyEqualizer ();
				player.Play ();
				State = player.State;
				player.Volume = Settings.CurrentVolume;
				SetVideo (player);
				//TODO: Fade out
				//ResetSecondary ();
				return true;
			}
			try {
				StopAllOthers (song);
				var player = GetPlayer (song,true);
				player.Volume = Settings.CurrentVolume;
				if (!player.IsPrepared) {
					var data = await Parent.PrepareSong (song, isVideo);
					if (!data.Item1)
						return false;
					var s = await player.PrepareData (data.Item2);
					if (!s)
						return false;
				}
				player.ApplyEqualizer ();
				player.Play ();
				SetVideo (player);
				return true;
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}

		void SetVideo (Player player)
		{
			var videoPlayer = player as AVMediaPlayer;
			if (videoPlayer != null) {
				Parent.VideoLayer.VideoLayer = videoPlayer.PlayerLayer;
			}
		}

		public override void UpdateBand (int band, float gain)
		{
			CurrentPlayer?.UpdateBand (band, gain);
		}

		public override Task<bool> PrepareData (PlaybackData playbackData)
		{
			return CurrentPlayer.PrepareData (playbackData);
		}

		void StopPlayer (Player player)
		{
			//TODO: fade
			player.Pause ();
		}

		bool isSongPlaying (Song song)
		{
			var player = playerQueue [song.Id];
			return player?.Rate > 0;
		}


		bool isSongPrepared (Song song)
		{
			var player = playerQueue [song.Id];
			return player?.IsPrepared ?? false;
		}

		public void Queue (Track track)
		{
			var song = Database.Main.GetObject<Song, TempSong> (track.SongId);
			nextSong = song;
		}


		float lastAudioCheck = 0;
		bool eqApplied;
		void OnPlabackTimeChanged (Player player)
		{
			if (player != CurrentPlayer || State != PlaybackState.Playing)
				return;
			if (!eqApplied) {
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
			var avgAudio = AudioLevels?.Max () ?? 1;
			if (remaining < 3 && avgAudio < .01 && lastAudioCheck < .01) {
				StartNext (current);
			} else if (remaining < .75) {
				StartNext (current);
			} else if (remaining < 15) {
				Console.WriteLine (avgAudio);
				PrepareNextSong ();
			}
			lastAudioCheck = avgAudio;
		}
		async void PrepareNextSong ()
		{
			if (nextSong == null || isSongPrepared(nextSong))
				return;
			var player = GetPlayer (nextSong,true);
			bool isVideo;
			isVideoDict.TryGetValue (nextSong.Id, out isVideo);
			var data = await Parent.PrepareSong (nextSong, isVideo);
			if (player == CurrentPlayer)
				return;
			if (!data.Item1)
				return;
			await player.PrepareData (data.Item2);

		}

		void StartNext (double currentPosition)
		{
			if (nextSong == CurrentSong)
				return;
			var playbackEndEvent = new PlaybackEndedEvent (CurrentSong) {
				TrackId = Settings.CurrentTrackId,
				Context = Settings.CurrentPlaybackContext,
				Position = currentPosition,
				Duration = Duration (),
				Reason = ScrobbleManager.PlaybackEndedReason.PlaybackEnded,
			};
			fadingToSong = nextSong;
			SecondaryPlayer.Play ();
			var videoPlayer = SecondaryPlayer as AVMediaPlayer;
			if (videoPlayer != null) {
				Parent.VideoLayer.VideoLayer = videoPlayer.PlayerLayer;
			}
			isUsingFirst = !isUsingFirst;

			ScrobbleManager.Shared.PlaybackEnded (playbackEndEvent);

			eqApplied = false;
			PlaybackManager.Shared.NextTrackWithoutPause ();
			Parent.CleanupSong (CurrentSong);
		}

		public override void DisableVideo ()
		{
			CurrentPlayer?.DisableVideo ();
		}

		public override void EnableVideo ()
		{
			CurrentPlayer?.EnableVideo ();
		}

		public override double CurrentTimeSeconds ()
		{
			var pos = CurrentPlayer?.CurrentTimeSeconds () ?? 0;
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

		Player CreatePlayer (Song song)
		{
			var isVideo = isVideoDict [song.Id];
#if BASS
			var player = isVideo ? (Player)new AVMediaPlayer () : new BassPlayer ();
#else
			var player = new AVMediaPlayer ();
#endif

			player.StateChanged = (state) => {
				StateChanged?.Invoke (state);
				if (player == CurrentPlayer)
					State = state;
				Parent.State = state;
			};

			player.Finished = (p) => {
				playerQueue.Remove (p.CurrentSongId);
				Finished?.Invoke (p);
			};

			player.PlabackTimeChanged = (time) => {
				PlabackTimeChanged?.Invoke (time);
				OnPlabackTimeChanged (player);
			};

			return player;
		}

		public override void Dispose ()
		{
			playerQueue.Clear ();
		}

		public override void ApplyEqualizer (Equalizer.Band [] bands)
		{
			CurrentPlayer?.ApplyEqualizer (bands);
		}

		public override void ApplyEqualizer ()
		{
			CurrentPlayer?.ApplyEqualizer ();
		}

		public override float Volume { get => CurrentPlayer.Volume; set => CurrentPlayer.Volume = value; }
		public override bool IsPlayerItemValid => CurrentPlayer?.IsPlayerItemValid ?? false;


	}
}

