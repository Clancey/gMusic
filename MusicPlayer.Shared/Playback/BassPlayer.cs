using System;
using System.Threading.Tasks;
#if BASS
using ManagedBass;
using MusicPlayer.Models;
using MusicPlayer.Playback;
using System.Runtime.InteropServices;
using MusicPlayer.Managers;
using ManagedBass.Fx;
using System.Timers;
using MusicPlayer.Data;
using System.Collections.Generic;
namespace MusicPlayer
{
	/// <summary>
	/// Wrapper around OutputQueue and AudioFileStream to allow streaming of various filetypes
	/// </summary>
	public class BassPlayer : Player
	{
		Timer progressTimer;
		static List<BassPlayer> currentPlayers = new List<BassPlayer>();
		static BassPlayer()
		{
#if __IOS__
			Bass.Configure(Configuration.IOSMixAudio, 0);
#else
			Bass.Configure(Configuration.GlobalMusicVolume, 0);
#endif

#if __MACOS__
			AudioOutputHelper.OutputChanged = () =>
			{
				DeviceInfo di;
				for (int d = 1; Bass.GetDeviceInfo(d, out di); d++)
				{
					if (di.IsDefault)
					{
						var oldDevice = Bass.DefaultDevice;
						if (oldDevice == d)
							break;
						Bass.Init(d);
						currentPlayers.ForEach(x => x.OutputChanged(d));
						Bass.CurrentDevice = d;
					}
				}
			};
#endif
			Bass.Init();
			Bass.Stop();
			var fxv = BassFx.Version;

		}
		static int bassPlayers = 0;
		static object bassPlayerLocker = new object();
		public static void StartBass()
		{
			lock (bassPlayerLocker)
				bassPlayers++;
			UpdateStaticBass();
		}
		public static void StopBass()
		{
			lock (bassPlayerLocker)
				bassPlayers--;
			UpdateStaticBass();

		}
		static void UpdateStaticBass()
		{
			lock (bassPlayerLocker)
			{
				Console.WriteLine($"Bass Players {bassPlayers}");
				if (bassPlayers == 0)
				{
					Task.Run(() =>
					{
						Console.WriteLine("Stopping Bass");
						Bass.Stop();
						Console.WriteLine("Stopped Bass");
					});
				}
				else if (bassPlayers > 0)
				{
					Bass.Start();
					Console.WriteLine("Starting Bass");
				}
			}
		}

		int streamHandle;
		int bufferSync;
		int endSync;
		FileProcedures fileProcs;
		PlaybackData currentData;
		IntPtr fileProcUser;
		IntPtr endSyncUser;
		IntPtr bufferSyncUser;
		bool shouldBePlaying = false;
		bool isDisposed = false;
		bool hasBassStarted = false;

		public BassPlayer()
		{
			State = Models.PlaybackState.Stopped;
			fileProcs = new FileProcedures
			{
				Close = OnFileClose,
				Length = OnFileLength,
				Read = OnFileRead,
				Seek = OnFileSeek,
			};
			progressTimer = new Timer(500);
			progressTimer.Elapsed += ProgressTimerChanged;
			currentPlayers.Add(this);
		}

		void OutputChanged(int device)
		{
			if(IsPlayerItemValid)
				Bass.ChannelSetDevice(streamHandle, device);
		}

		void ProgressTimerChanged(object o, EventArgs e)
		{
			if (!shouldBePlaying && IsPlayerItemValid && Bass.ChannelIsActive(streamHandle) == ManagedBass.PlaybackState.Playing)
				Bass.ChannelPause(streamHandle);
			if (State != Models.PlaybackState.Playing)
				return;
			if (Rate.IsZero())
				Play();
			var time = CurrentTimeSeconds();
			this.PlabackTimeChanged(time);
		}

		public override bool IsPlayerItemValid => streamHandle != 0;

		public override bool IsPrepared
		{
			get
			{
				return IsPlayerItemValid;
			}
			set
			{
				base.IsPrepared = value;
			}
		}

		public override float Rate => IsPlayerItemValid && Bass.ChannelIsActive(streamHandle) == ManagedBass.PlaybackState.Playing ? 1 : 0;

		public override float Volume
		{
			get => Bass.GlobalStreamVolume / 10000f;
			set => Bass.GlobalStreamVolume = (int)(value * 10000);
		}

		double currentTime;
		public override double CurrentTimeSeconds()
		{
			var t = IsPlayerItemValid ? Bass.ChannelBytes2Seconds(streamHandle, Bass.ChannelGetPosition(streamHandle)) : 0;
			if (t >= 0)
				currentTime = t;
			return currentTime;
		}
		public override void Dispose()
		{
			isDisposed = true;
			progressTimer.Elapsed -= ProgressTimerChanged;
			Stop();
			RemoveHandles();
			BassFileProceduresManager.ClearProcedure(fileProcUser);
			BassFileProceduresManager.ClearProcedure(endSyncUser);
			BassFileProceduresManager.ClearProcedure(bufferSyncUser);
			if (currentPlayers.Contains(this))
				currentPlayers.Remove(this);
		}

		public override float[] AudioLevels
		{
			get
			{
				const float MaxValue = 32768;
				if (!IsPlayerItemValid)
					return base.AudioLevels;
				var left = (float)Bass.ChannelGetLevelLeft(streamHandle);
				var right = (float)Bass.ChannelGetLevelRight(streamHandle);
				//Console.WriteLine(left);
				return new[] { left / MaxValue, right / MaxValue };
			}
			set
			{
				base.AudioLevels = value;
			}
		}

		public override double Duration()
		{
			if (!IsPlayerItemValid)
				return this.currentData?.SongPlaybackData?.CurrentTrack?.Duration ?? 0;
			var legnth = Bass.ChannelGetLength(streamHandle);
			return Bass.ChannelBytes2Seconds(streamHandle, legnth);
		}

		public override void Pause()
		{
			currentPossition = IsPlayerItemValid ? Bass.ChannelGetPosition(streamHandle) : 0;
			Bass.Pause();
			shouldBePlaying = false;
			if (!IsPlayerItemValid)
			{
				SetState();
				return;
			}
			//Bass.ChannelSlideAttribute (streamHandle, ChannelAttribute.Volume, -1, 20);
			Bass.ChannelPause(streamHandle);
			SetState();
		}
		long currentPossition;

		public override void Stop()
		{
			progressTimer.Enabled = false;
			if (hasBassStarted)
			{
				hasBassStarted = false;
				StopBass();
			}
			shouldBePlaying = false;
			if (!IsPlayerItemValid)
			{
				SetState();
				return;
			}
			Task.Run(() =>
			{
				Bass.ChannelStop(streamHandle);
				SetState();
			});


		}

		public override bool Play()
		{
			shouldBePlaying = true;
			if (!IsPlayerItemValid)
			{
				SetState();
				return false;
			}
			if (!hasBassStarted)
			{
				hasBassStarted = true;
				StartBass();
			}
			else
				Bass.Start();
			if (currentPossition > 0 && Bass.ChannelGetPosition(streamHandle) < currentPossition)
				Bass.ChannelSetPosition(streamHandle, currentPossition);
			var success = Bass.ChannelPlay(streamHandle, false);
			Console.WriteLine($"Play Song: {success}");
			if (!success)
			{
				var error = Bass.LastError;
				if (error == Errors.Handle)
				{
					RemoveHandles();
					Stop();
					streamHandle = 0;
				}
				Console.WriteLine(error);
			}
			progressTimer.Enabled = true;
			SetState();
			return success;
		}

		void SetState()
		{
			if (!IsPlayerItemValid)
			{
				State = shouldBePlaying && !string.IsNullOrWhiteSpace(CurrentSongId) ? Models.PlaybackState.Buffering : Models.PlaybackState.Stopped;
				return;
			}
			var state = Bass.ChannelIsActive(streamHandle);
			switch (state)
			{
				case ManagedBass.PlaybackState.Paused:
					State = Models.PlaybackState.Paused;
					return;
				case ManagedBass.PlaybackState.Stalled:
					State = Models.PlaybackState.Buffering;
					return;
				case ManagedBass.PlaybackState.Stopped:
					State = Models.PlaybackState.Stopped;
					return;
				case ManagedBass.PlaybackState.Playing:
					State = Models.PlaybackState.Playing;
					return;
			}

		}
		public override Task<bool> PlaySong(Song song, bool isVideo, bool forcePlay = false)
		{
			throw new NotImplementedException();
		}

		Task<bool> prepareDataTask;
		public override Task<bool> PrepareData(PlaybackData playbackData, bool isPlaying)
		{
			if (prepareDataTask?.IsCompleted ?? true)
				prepareDataTask = prepareData(playbackData, isPlaying);
			return prepareDataTask;
		}

		async Task<bool> prepareData(PlaybackData playbackData, bool isPlaying)
		{
			CurrentSongId = playbackData.SongId;
			shouldBePlaying = isPlaying;
			SetState();
			//Only reprep the same song twice if it changed between local and streamed
			if (IsPlayerItemValid &&
				currentData?.SongPlaybackData?.CurrentTrack?.Id == playbackData.SongPlaybackData.CurrentTrack.Id &&
			   currentData?.SongPlaybackData?.IsLocal == playbackData.SongPlaybackData.IsLocal)
			{
				return true;
			}
			currentData = playbackData;
			var location = Bass.ChannelGetPosition(streamHandle);
			if (IsPlayerItemValid)
			{
				Stop();
				RemoveHandles();
			}
			return await Task.Run(() =>
		   {
			   var data = playbackData.SongPlaybackData;
			   if (data.IsLocal)
			   {
				   streamHandle = Bass.CreateStream(data.Uri.LocalPath, Flags: BassFlags.Prescan);
			   }
			   else
			   {
				   var fileProcData = BassFileProceduresManager.CreateProcedure(fileProcs);
				   fileProcUser = fileProcData.user;
				   streamHandle = Bass.CreateStream(StreamSystem.Buffer, BassFlags.Prescan, fileProcData.proc, fileProcData.user);//, downloaderPointer);
			   }
			   if (streamHandle == 0)
			   {
				   var error = Bass.LastError;
				   Console.WriteLine(error);
				   return false;
			   }
			   Bass.ChannelSetAttribute(streamHandle, ChannelAttribute.Volume, 1f);
#if __MACOS__
				Bass.ChannelSetDevice(streamHandle, Bass.CurrentDevice);
#endif

			   var endSyncData = BassFileProceduresManager.CreateProcedure(OnTrackEnd);
			   endSyncUser = endSyncData.user;
			   endSync = Bass.ChannelSetSync(streamHandle, SyncFlags.End, 0, endSyncData.proc, endSyncUser);

			   var bufferSyncData = BassFileProceduresManager.CreateProcedure(OnBuffering); ;
			   bufferSyncUser = bufferSyncData.user;
			   bufferSync = Bass.ChannelSetSync(streamHandle, SyncFlags.Stalled | SyncFlags.Mixtime, 0, bufferSyncData.proc, bufferSyncUser);
			   if (location > 0)
			   {
				   Bass.ChannelSetPosition(streamHandle, location);
			   }
			   SetState();
			   if (shouldBePlaying)
				   Play();
			   if (isDisposed)
				   Dispose();
			   return true;
		   });
		}

		public override void Seek(double seconds)
		{
			if (!IsPlayerItemValid)
			{
				return;
			}

			currentPossition = Bass.ChannelSeconds2Bytes(streamHandle, seconds);
			Bass.ChannelSetPosition(streamHandle, currentPossition);
		}

		void OnFileClose(IntPtr user)
		{
			//We are done with the downloader. Lets free it's memory
			//currentData?.DataStream?.Dispose();
		}

		long OnFileLength(IntPtr user)
		{
			try
			{
				while ((currentData?.DataStream.Length ?? 0) == 0)
				{
					Task.Delay(500).Wait();

					if (isDisposed)
						return 0;
				}
				return currentData?.DataStream.Length ?? 0;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}

			return 0;
		}

		int OnFileRead(IntPtr buffer, int length, IntPtr user)
		{
			if (isDisposed)
				return 0;
			var data = new byte[length];
			var read = 0;
			try
			{
				read = currentData?.DataStream?.Read(data, 0, length) ?? 0;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			if (read > 0)
				Marshal.Copy(data, 0, buffer, read);
			return read;
		}


		bool OnFileSeek(long offset, IntPtr user)
		{
			if (isDisposed)
				return false;
			currentData?.DataStream.Seek(offset, System.IO.SeekOrigin.Begin);
			return true;
		}

		void OnTrackEnd(int handle, int channel, int data, IntPtr user)
		{
			Finished?.Invoke(this);
			Stop();
		}

		void OnBuffering(int handle, int channel, int data, IntPtr user)
		{
			SetState();
		}

		void RemoveHandles()
		{
			if (IsPlayerItemValid)
			{
				Bass.ChannelRemoveSync(streamHandle, bufferSync);
				Bass.ChannelRemoveSync(streamHandle, endSync);
				BassFileProceduresManager.ClearProcedure(fileProcUser);
				BassFileProceduresManager.ClearProcedure(endSyncUser);
				BassFileProceduresManager.ClearProcedure(bufferSyncUser);
				streamHandle = Bass.StreamFree(streamHandle) ? 0 : streamHandle;
			}
			else
				streamHandle = 0;

		}

		int fxStream;
		int fxEq;
		public override void ApplyEqualizer(Equalizer.Band[] bands)
		{
			if (!IsPlayerItemValid || !Settings.EqualizerEnabled)
				return;
			if (fxStream == streamHandle)
			{
				for (int i = 0; i < bands.Length; i++)
				{
					UpdateBand(i, bands[i].Gain);
				}
				return;
			}

			fxEq = Bass.ChannelSetFX(streamHandle, EffectType.PeakEQ, 1);
			if (fxEq == 0)
			{
				fxStream = 0;
				LogManager.Shared.Report(new Exception($"Equalizer not working {Bass.LastError}"));
				return;
			}

			fxStream = streamHandle;
			eq.fQ = 0f;
			eq.fBandwidth = .6f;
			eq.lChannel = FXChannelFlags.All;

			for (int i = 0; i < bands.Length; i++)
			{
				eq.lBand = i;
				eq.fCenter = bands[i].Center;
				eq.fGain = bands[i].Gain;
				Console.WriteLine(eq.fCenter);
				Bass.FXSetParameters(fxEq, eq);
			}
		}
		readonly PeakEQParameters eq = new PeakEQParameters();
		public override void ApplyEqualizer()
		{
			ApplyEqualizer(Equalizer.Shared.Bands);
		}

		public override void UpdateBand(int band, float gain)
		{
			if (fxEq == 0)
			{
				ApplyEqualizer();
				if (fxEq == 0)
					return;

			}
			// get values of the selected band
			eq.lBand = band;

			Bass.FXGetParameters(fxEq, eq);
			//eq.fGain = gain+( _cmp1.fThreshold * (1 / _cmp1.fRatio - 1));
			eq.fGain = gain;
			Bass.FXSetParameters(fxEq, eq);
		}
	}
}


#endif