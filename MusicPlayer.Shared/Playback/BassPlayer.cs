using System;
using System.Threading.Tasks;
#if BASS
using ManagedBass;
using MusicPlayer.Models;
using System.IO;
using MusicPlayer.Playback;
using AudioToolbox;
using ObjCRuntime;
using System.Runtime.InteropServices;
using MusicPlayer.Managers;
using ManagedBass.Fx;
using System.Timers;
using MusicPlayer.Data;
namespace MusicPlayer
{
	/// <summary>
	/// Wrapper around OutputQueue and AudioFileStream to allow streaming of various filetypes
	/// </summary>
	public class BassPlayer : Player
	{
		Timer progressTimer;
		static BassPlayer()
		{
#if __IOS__
			Bass.Configure(Configuration.IOSMixAudio, 0);
#endif
			Bass.Init();
			var fxv = BassFx.Version;
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
		}

		void ProgressTimerChanged(object o, EventArgs e)
		{
			if (State != Models.PlaybackState.Playing)
				return;
			var time = CurrentTimeSeconds();
			this.PlabackTimeChanged(time);
		}

		public override bool IsPlayerItemValid => streamHandle != 0;

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
			Stop();
			RemoveHandles();
			BassFileProceduresManager.ClearProcedure(fileProcUser);
			BassFileProceduresManager.ClearProcedure(endSyncUser);
			BassFileProceduresManager.ClearProcedure(bufferSyncUser);

		}

		public override float[] AudioLevels
		{
			get
			{
				if (!IsPlayerItemValid)
					return base.AudioLevels;
				var left = (float)Bass.ChannelGetLevelLeft(streamHandle);
				var right = (float)Bass.ChannelGetLevelRight(streamHandle);
				//Console.WriteLine(left);
				return new[] { left, right };
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

		void Stop()
		{
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

		public override void Play()
		{
			shouldBePlaying = true;
			if (!IsPlayerItemValid)
			{
				SetState();
				return;
			}
			var success = Bass.ChannelPlay(streamHandle, false);
			Console.WriteLine($"Play Song: {success}");
			Console.WriteLine(Bass.LastError);
			SetState();
		}

		void SetState()
		{
			try
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
			finally
			{
				var shouldRun = State == Models.PlaybackState.Playing;
				if (shouldRun)
				{
					if (!progressTimer.Enabled)
						progressTimer.Start();
				}
				else
				{
					progressTimer.Stop();
				}
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
				IsPrepared = true;
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
			   Bass.Start();
			   var data = playbackData.SongPlaybackData;
			   if (data.IsLocal)
			   {
				   streamHandle = Bass.CreateStream(data.Uri.LocalPath, Flags: BassFlags.AutoFree | BassFlags.Prescan);
			   }
			   else
			   {
				   var fileProcData = BassFileProceduresManager.CreateProcedure(fileProcs);
				   fileProcUser = fileProcData.user;
				   streamHandle = Bass.CreateStream(StreamSystem.Buffer, BassFlags.AutoFree, fileProcData.proc, fileProcData.user);//, downloaderPointer);
				}
			   if (streamHandle == 0)
			   {
				   var error = Bass.LastError;
				   Console.WriteLine(error);
				   return false;
			   }
			   Bass.ChannelSetAttribute(streamHandle, ChannelAttribute.Volume, 1f);

			   var endSyncData = BassFileProceduresManager.CreateProcedure(OnTrackEnd);
			   endSyncUser = endSyncData.user;
			   endSync = Bass.ChannelSetSync(streamHandle, SyncFlags.End | SyncFlags.Mixtime, 0, endSyncData.proc, endSyncUser);

			   var bufferSyncData = BassFileProceduresManager.CreateProcedure(OnBuffering); ;
			   bufferSyncUser = bufferSyncData.user;
			   bufferSync = Bass.ChannelSetSync(streamHandle, SyncFlags.Stalled, 0, bufferSyncData.proc, bufferSyncUser);
			   if (location > 0)
			   {
				   Bass.ChannelSetPosition(streamHandle, location);
			   }
			   SetState();
			   IsPrepared = true;
			   if (shouldBePlaying)
				   Play();
			   return true;
		   });
		}

		public override void Seek(double seconds)
		{
			if (!IsPlayerItemValid)
			{
				return;
			}

			var location = Bass.ChannelSeconds2Bytes(streamHandle, seconds);
			Bass.ChannelSetPosition(streamHandle, location);
		}

		void OnFileClose(IntPtr user)
		{
			//We are done with the downloader. Lets free it's memory
			currentData?.DownloadHelper?.Dispose();
		}

		long OnFileLength(IntPtr user)
		{
			try
			{
				while ((currentData?.DownloadHelper.Length ?? 0) == 0)
					Task.Delay(500).Wait();
				return currentData?.DownloadHelper.Length ?? 0;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}

			return 0;
		}

		int OnFileRead(IntPtr buffer, int length, IntPtr user)
		{
			var data = new byte[length];
			var read = 0;
			try
			{
				read = currentData?.DownloadHelper?.Read(data, 0, length) ?? 0;
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
			return true;
		}

		void OnTrackEnd(int handle, int channel, int data, IntPtr user)
		{
			Finished?.Invoke(this);
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
			}
			streamHandle = 0;

		}

		int fxStream;
		int fxEq;
		public override void ApplyEqualizer(Equalizer.Band[] bands)
		{
			if (!IsPlayerItemValid)
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
				Console.WriteLine(Bass.LastError);
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
		readonly PeakEQParameters eq = new PeakEQParameters ();
		public override void ApplyEqualizer()
		{
			ApplyEqualizer(Equalizer.Shared.Bands);
		}

		public override void UpdateBand(int band, float gain)
		{
			if (fxEq == 0)
			{
				ApplyEqualizer();
				if(fxEq == 0)
					return;
			}
			// get values of the selected band
			eq.lBand = band;
			Bass.FXGetParameters (fxEq,eq);
			//eq.fGain = gain+( _cmp1.fThreshold * (1 / _cmp1.fRatio - 1));
			eq.fGain = gain;
			Bass.FXSetParameters (fxEq, eq);
		}
	}
}


#endif