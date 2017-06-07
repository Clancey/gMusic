using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media.Session;
using Android.Views;
using Android.Widget;
using MusicPlayer.Droid.Services;
using MusicPlayer.Models;
using MusicPlayer.Managers;
using MusicPlayer.Playback;

namespace MusicPlayer.Droid.Playback
{

	public enum PlayerAudioState
	{
		NoFocusNoDuck = 0,
		NoFocusCanDuck = 1,
		Focused = 2,
	}

	class MediaPlayerPlayer : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener,
	                                               Android.Media.MediaPlayer.IOnPreparedListener,
	                                               Android.Media.MediaPlayer.IOnCompletionListener,
	                                               Android.Media.MediaPlayer.IOnErrorListener,
	                                               Android.Media.MediaPlayer.IOnSeekCompleteListener
	{
		public const float VolumeDuck = .2f;
		public const float VolumeNormal = 1f;
		public NativeAudioPlayer Parent;

		static Context Context;
		WifiManager.WifiLock WifiLock;
		NoisyReciever NoisyAudioReciever;// = new MediaPlayerPlayer.NoisyReciever();

		bool playOnFocus;
		bool AudioNoisyRecieverRegistered;
		int currentPosition;
		string currentMediaId;

		PlayerAudioState audioFocus = PlayerAudioState.NoFocusNoDuck;
		static AudioManager audioManager;
		Android.Media.MediaPlayer mediaPlayer;

		IntentFilter audioNoisyIntentFilter;// = new IntentFilter(AudioManager.ActionAudioBecomingNoisy);


		public MediaPlayerPlayer(Context context)
		{
			Context = context;
			//NoisyAudioReciever = new MediaPlayerPlayer.NoisyReciever();
			//audioNoisyIntentFilter = new IntentFilter(AudioManager.ActionAudioBecomingNoisy);;
			audioManager = (AudioManager) context.GetSystemService(Context.AudioService);
			WifiLock = ((WifiManager) context.GetSystemService(Context.WifiService)).CreateWifiLock(WifiMode.Full, "gMusic_lock");
			state = PlaybackStateCompat.StateNone;
		}
		int state;
		public int State
		{
			get { return state; }

			set
			{
				if (state == value)
					return;
				state = value;
				UpdateSession();
				SetPlaybackState();
			}
		}
		public bool StateIsPlaying
		{
			get { return State == PlaybackStateCompat.StatePlaying || State == PlaybackStateCompat.StatePaused;}
		}

		public bool IsMediaPlaying
		{
			get { return mediaPlayer?.IsPlaying ?? false;}
		}
		public int Position
		{
			get
			{
				if (mediaPlayer == null
					|| (State != PlaybackStateCompat.StatePlaying
						&& State != PlaybackStateCompat.StatePaused))
					return -1;
				else
					return mediaPlayer.CurrentPosition;
			}
		}

		public int Duration
		{
			get
			{
				if (mediaPlayer == null
					|| (State != PlaybackStateCompat.StatePlaying
						&& State != PlaybackStateCompat.StatePaused))
					return 0;
				else
					return mediaPlayer.Duration;
			}
		}

		private int buffered = 0;

		public int Buffered
		{
			get
			{
				if (mediaPlayer == null)
					return 0;
				else
					return buffered;
			}
			private set
			{
				buffered = value;
				//OnBuffering(EventArgs.Empty);
			}
		}

		void SetPlaybackState()
		{
			switch (state)
			{
				case PlaybackStateCompat.StateError:
				case PlaybackStateCompat.StateNone:
				case PlaybackStateCompat.StateStopped:
					Managers.NotificationManager.Shared.ProcPlaybackStateChanged(PlaybackState.Stopped);
					return;
				case PlaybackStateCompat.StateBuffering:
					Managers.NotificationManager.Shared.ProcPlaybackStateChanged(PlaybackState.Buffering);
					return;
				case PlaybackStateCompat.StatePaused:
					Managers.NotificationManager.Shared.ProcPlaybackStateChanged(PlaybackState.Paused);
					return;
				case PlaybackStateCompat.StatePlaying:
					Managers.NotificationManager.Shared.ProcPlaybackStateChanged(PlaybackState.Playing);
					return;
			}
		}

		void UpdateSession()
		{

			var stateBuilder = new PlaybackStateCompat.Builder().SetActions(AvailableActions);



			Console.WriteLine("************");
			Console.WriteLine("***************");
			Console.WriteLine($"Media player: Playback State: {StateIsPlaying}/{IsPlaying} - position {Position}");
			Console.WriteLine("***************");
			Console.WriteLine("******");

			var favIcon = Parent.CurrentSong?.Rating > 1 ? Resource.Drawable.ic_star_on : Resource.Drawable.ic_star_off;
			var customActionExtras = new Bundle();
			//TODO: run through wearables
			stateBuilder.AddCustomAction(
				new PlaybackStateCompat.CustomAction.Builder("THUMBSUP", "FAVORITE", favIcon).SetExtras(customActionExtras).Build());
			stateBuilder.SetState(State, Position, 1f, SystemClock.ElapsedRealtime());

			MusicService.Shared.Session.SetPlaybackState(stateBuilder.Build());

			if (State == PlaybackStateCompat.StatePlaying || State == PlaybackStateCompat.StatePaused)
				MusicService.Shared.MediaNotificationManager.StartNotification();


		}
		private long AvailableActions
		{
			get
			{
				long actions =
					PlaybackStateCompat.ActionPlay |
					PlaybackStateCompat.ActionPlayFromMediaId |
					PlaybackStateCompat.ActionPlayFromSearch |
					PlaybackStateCompat.ActionSkipToPrevious |
					PlaybackStateCompat.ActionSetRating |
					PlaybackStateCompat.ActionPlayPause |
					PlaybackStateCompat.ActionSkipToNext
					;
				if (Parent?.State == PlaybackState.Playing)
				{
					actions |= PlaybackStateCompat.ActionPause;
				}
				return actions;
			}
		}
		public bool IsConnected => true;
		public bool IsPlaying => playOnFocus || (mediaPlayer?.IsPlaying ?? false);

		public int CurrentStreamPosition
		{
			get { return mediaPlayer?.CurrentPosition ?? currentPosition; }
			set { currentPosition = value; }
			
		}
		public string CurrentMediaId { get; set; }
		public virtual void Start()
		{

		}

		public virtual void Stop(bool notifyListeners)
		{
			if (notifyListeners)
			{

				State = PlaybackStateCompat.StateStopped;
			}
			else
				state = PlaybackStateCompat.StateStopped;
			currentPosition = CurrentStreamPosition;
			GiveUpAudioFocus();
			UnregisterAudioNoisyReciever();
			RelaxResources(true);
		}

		public void UpdateLastKnownStreamPosition()
		{
			currentPosition = mediaPlayer?.CurrentPosition ?? currentPosition;
		}

		public async Task<bool> Play(Song song)
		{

			playOnFocus = true;
			TryToGetAudioFocus();
			RegisterAudioNoisyReciever();
			var mediaId = song.Id;
			var hasChanges = mediaId != CurrentMediaId;
			if (hasChanges)
				currentPosition = 0;

			CurrentMediaId = mediaId;
			MusicService.Shared.Session.SetMetadata(song.ToMediaMetadataCompat());
			if (State == PlaybackStateCompat.StatePaused && !hasChanges && mediaPlayer != null)
			{
				ConfigureMediaPlayerState();
				return true;
			}

			State = PlaybackStateCompat.StateStopped;
			RelaxResources(false);

			try
			{
				//TODO:  Get local streaming url
				var data = await Parent.PrepareSong(song);
				if (!data.Item1)
					return false;
				var url = data.Item2.SongPlaybackData.Uri;
				CreateMediaPlayerIfNeeded();
				State = PlaybackStateCompat.StateBuffering;
				mediaPlayer.SetAudioStreamType(Stream.Music);
				mediaPlayer.SetDataSource(url.AbsoluteUri);;
				mediaPlayer.PrepareAsync();
				WifiLock.Acquire();
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				return false;
			}
		}
		public void Pause()
		{
			if (State == PlaybackStateCompat.StatePlaying)
			{
				if (mediaPlayer?.IsPlaying ?? false)
				{
					mediaPlayer.Pause();
					currentPosition = mediaPlayer?.CurrentPosition ?? currentPosition;
				}
				RelaxResources(false);
				GiveUpAudioFocus();
			}
			State = PlaybackStateCompat.StatePaused;
			UnregisterAudioNoisyReciever();
		}

		public void Seek(int position)
		{
			if (mediaPlayer == null)
			{
				currentPosition = position;
				return;
			}
			if (mediaPlayer.IsPlaying)
				State = PlaybackStateCompat.StateBuffering;
			mediaPlayer.SeekTo(position);
		}

		void GiveUpAudioFocus()
		{
			if(audioFocus != PlayerAudioState.Focused)
				return;
			if(audioManager.AbandonAudioFocus(this) == AudioFocusRequest.Granted)
				audioFocus = PlayerAudioState.NoFocusNoDuck; 
		}
		void RegisterAudioNoisyReciever()
		{
			if (AudioNoisyRecieverRegistered)
				return;
			//NoisyAudioReciever.Parent = this;
			//Context.RegisterReceiver(NoisyAudioReciever, audioNoisyIntentFilter);
			AudioNoisyRecieverRegistered = true;
		}
		void UnregisterAudioNoisyReciever()
		{

			if (!AudioNoisyRecieverRegistered)
				return;
			//Context.UnregisterReceiver(NoisyAudioReciever);
			AudioNoisyRecieverRegistered = false;
		}

		void RelaxResources(bool releaseMediaPlayer)
		{
			if (releaseMediaPlayer)
			{
				mediaPlayer?.Reset();
				mediaPlayer?.Release();
				mediaPlayer = null;
			}
			if (WifiLock.IsHeld)
				WifiLock.Release();
		}

		void TryToGetAudioFocus()
		{
			if (audioFocus == PlayerAudioState.Focused)
				return;
			var result = audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
			if (result == AudioFocusRequest.Granted)
				audioFocus = PlayerAudioState.Focused;
		}

		void ConfigureMediaPlayerState()
		{
			if (audioFocus == PlayerAudioState.NoFocusNoDuck)
			{
				if (State == PlaybackStateCompat.StatePlaying)
					Pause();
				return;
			}

			if (audioFocus == PlayerAudioState.NoFocusCanDuck)
				mediaPlayer?.SetVolume(VolumeDuck,VolumeDuck);
			else
				mediaPlayer?.SetVolume(VolumeNormal,VolumeNormal);
			if (!playOnFocus)
				return;
			if (!(mediaPlayer?.IsPlaying ?? true))
			{
				if (currentPosition == mediaPlayer.CurrentPosition)
				{
					mediaPlayer.Start();
					State = PlaybackStateCompat.StatePlaying;
				}
				else
				{
					mediaPlayer.SeekTo(currentPosition);
					State = PlaybackStateCompat.StateBuffering;
				}
			}
			playOnFocus = false;
		}

		void CreateMediaPlayerIfNeeded()
		{
			if (mediaPlayer != null)
			{
				mediaPlayer.Reset();
				return;
			}

			mediaPlayer = new Android.Media.MediaPlayer();
			mediaPlayer.SetWakeMode(Context.ApplicationContext, WakeLockFlags.Partial);
			mediaPlayer.SetOnPreparedListener(this);
			mediaPlayer.SetOnCompletionListener(this);
			mediaPlayer.SetOnErrorListener(this);
			mediaPlayer.SetOnSeekCompleteListener(this);
		}

		public void OnAudioFocusChange(AudioFocus focusChange)
		{
			switch (focusChange)
			{
				case AudioFocus.Gain:
					audioFocus = PlayerAudioState.Focused;
					break;
				case AudioFocus.Loss:
				case AudioFocus.GainTransient:
				case AudioFocus.LossTransientCanDuck:
					var canDuck = focusChange == AudioFocus.LossTransientCanDuck;
					audioFocus = canDuck ? PlayerAudioState.NoFocusCanDuck : PlayerAudioState.NoFocusNoDuck;
					if (State == PlaybackStateCompat.StatePlaying && !canDuck)
					{
						playOnFocus = true;
					}
					break;
				default:
					Console.WriteLine($"Audio focus changed {focusChange}");
					break;
			}
			ConfigureMediaPlayerState();
		}

		public void OnPrepared(Android.Media.MediaPlayer mp)
		{
			ConfigureMediaPlayerState();
		}

		public async void OnCompletion(Android.Media.MediaPlayer mp)
		{
			await PlaybackManager.Shared.NextTrack();
		}

		public bool OnError(Android.Media.MediaPlayer mp, [GeneratedEnum] MediaError what, int extra)
		{
			LogManager.Shared.Report(new Exception($"MediaPlayer {what} ({extra})"));
			return true;
		}

		public void OnSeekComplete(Android.Media.MediaPlayer mp)
		{
			currentPosition = mp.CurrentPosition;
			if (State == PlaybackStateCompat.StateBuffering)
			{
				mediaPlayer.Start();
				State = PlaybackStateCompat.StatePlaying;
			}
		}
		[BroadcastReceiver]
		class NoisyReciever : BroadcastReceiver
		{
			public MediaPlayerPlayer Parent { get; set; }
			public override void OnReceive(Context context, Intent intent)
			{
				if (AudioManager.ActionAudioBecomingNoisy != intent.Action || !Parent.IsPlaying)
					return;
				var i = new Intent(context, typeof(MusicService));
				i.SetAction(MusicService.ActionCmd);
				i.PutExtra(MusicService.CmdName, MusicService.CmdPause);
				MediaPlayerPlayer.Context.StartService(i);
			}
		}
	}
}