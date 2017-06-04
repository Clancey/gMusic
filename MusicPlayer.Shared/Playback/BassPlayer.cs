using System;
using System.Threading.Tasks;
#if BASS
using ManagedBass;
using ManagedBass.DirectX8;
using MusicPlayer.Models;
using System.IO;
using MusicPlayer.Playback;
using AudioToolbox;
using ObjCRuntime;
using System.Runtime.InteropServices;
using MusicPlayer.Managers;
using ManagedBass.Fx;
using System.Timers;
namespace MusicPlayer
{
	/// <summary>
	/// Wrapper around OutputQueue and AudioFileStream to allow streaming of various filetypes
	/// </summary>
	public class BassPlayer : Player
	{
		Timer progressTimer;
		static BassPlayer ()
		{
			Bass.Init ();
			var fxv = BassFx.Version;
		}
		int streamHandle;
		int bufferSync;
		int endSync;
		FileProcedures fileProcs;
		PlaybackData currentData;

		public BassPlayer ()
		{
			State = Models.PlaybackState.Stopped;
			fileProcs = new FileProcedures {
				Close = OnFileClose,
				Length = OnFileLength,
				Read = OnFileRead,
				Seek = OnFileSeek,
			};
			progressTimer = new Timer (500);
			progressTimer.Elapsed += ProgressTimerChanged;
		}

		void ProgressTimerChanged (object o, EventArgs e)
		{
			if (State != Models.PlaybackState.Playing)
				return;
			var time = CurrentTimeSeconds ();
			this.PlabackTimeChanged(time);
		}

		public override bool IsPlayerItemValid => StreamIsValid;

		bool StreamIsValid => streamHandle != 0;

		public override float Rate => IsPlayerItemValid && Bass.ChannelIsActive (streamHandle) == ManagedBass.PlaybackState.Playing ? 1 : 0;

		public override float Volume {
			get => Bass.GlobalStreamVolume / 1000f;
			set => Bass.GlobalStreamVolume = (int)(value * 1000);
		}

		public override double CurrentTimeSeconds () => StreamIsValid ? Bass.ChannelBytes2Seconds (streamHandle, Bass.ChannelGetPosition (streamHandle)) : 0;

		public override void Dispose ()
		{
			Stop ();
			RemoveHandles ();
		}

		public override float [] AudioLevels {
			get {
				if (!IsPlayerItemValid)
					return base.AudioLevels;
				var left = (float)Bass.ChannelGetLevelLeft (streamHandle);
				var right = (float)Bass.ChannelGetLevelRight (streamHandle);
				//Console.WriteLine(left);
				return new [] { left, right };
			}
			set {
				base.AudioLevels = value;
			}
		}

		public override double Duration () => StreamIsValid ? Bass.ChannelBytes2Seconds (streamHandle, Bass.ChannelGetLength (streamHandle)) : 0;

		public override void Pause ()
		{
			if (!StreamIsValid)
				return;
			Bass.ChannelSlideAttribute (streamHandle, ChannelAttribute.Volume, -1, 20);
			Bass.ChannelPause (streamHandle);
		}

		void Stop ()
		{
			if (!StreamIsValid)
				return;
			Bass.ChannelSlideAttribute (streamHandle, ChannelAttribute.Volume, -1, 20);
			Bass.ChannelStop (streamHandle);
		}

		public override void Play ()
		{
			if (!StreamIsValid)
				return;
			
			Bass.ChannelPlay (streamHandle, false);
			Bass.ChannelSlideAttribute (streamHandle, ChannelAttribute.Volume, 1, 20);
			SetState ();
		}

		void SetState ()
		{
			try {
				if (!IsPlayerItemValid) {
					State = Models.PlaybackState.Stopped;
					return;
				}
				var state = Bass.ChannelIsActive (streamHandle);
				switch (state) {
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
			} finally {
				var shouldRun = State == Models.PlaybackState.Playing;
				if (shouldRun) {
					if (!progressTimer.Enabled)
						progressTimer.Start ();
				} else {
					progressTimer.Stop ();
				}

			}
		}
		public override Task<bool> PlaySong (Song song, bool isVideo, bool forcePlay = false)
		{
			throw new NotImplementedException ();
		}

		public override async Task<bool> PrepareData (PlaybackData playbackData)
		{

			//Only reprep the same song twice if it changed between local and streamed
			if (IsPlayerItemValid &&
			    currentData?.SongPlaybackData?.CurrentTrack?.Id == playbackData.SongPlaybackData.CurrentTrack.Id &&
			   currentData?.SongPlaybackData?.IsLocal == playbackData.SongPlaybackData.IsLocal) {
				return true;
			}
			currentData = playbackData;
			var location = Bass.ChannelGetPosition (streamHandle);
			if (IsPlayerItemValid) {
				Stop ();
				RemoveHandles ();
			}
			return await Task.Run (() => {
				Bass.Start ();
				var data = playbackData.SongPlaybackData;
				if (data.IsLocal) {
					streamHandle = Bass.CreateStream (data.Uri.LocalPath, Flags: BassFlags.AutoFree | BassFlags.Prescan);

				} else {
					var handle = GCHandle.Alloc (playbackData.DownloadHelper, GCHandleType.Pinned);
					var downloaderPointer = GCHandle.ToIntPtr (handle);
					streamHandle = Bass.CreateStream (StreamSystem.Buffer, BassFlags.AutoFree, fileProcs, downloaderPointer);
				}
				if (streamHandle == 0) {
					var error = Bass.LastError;
					Console.WriteLine (error); 
					return false;
				}
				endSync = Bass.ChannelSetSync (streamHandle, SyncFlags.End, 0, OnTrackEnd, IntPtr.Zero);
				bufferSync = Bass.ChannelSetSync (streamHandle, SyncFlags.Stalled, 0, OnBuffering, IntPtr.Zero);
				if (location > 0) {
					Bass.ChannelSetPosition (streamHandle, location);
				}
				SetState ();
				return true;
			});
		}

		public override void Seek (double seconds)
		{
			if (!IsPlayerItemValid) {
				return;
			}

			var location = Bass.ChannelSeconds2Bytes (streamHandle, seconds);
			Bass.ChannelSetPosition (streamHandle, location);
		}

		void OnFileClose (IntPtr user)
		{
			Stop ();
			RemoveHandles ();
		}

		long OnFileLength (IntPtr user)
		{
			try {
				var downloader = GCHandle.FromIntPtr (user).Target as DownloadHelper;
				return downloader?.Length ?? 0;
			} catch (Exception ex) {
				LogManager.Shared.Report (ex);
			}

			return 0;
		}

		int OnFileRead (IntPtr buffer, int length, IntPtr user)
		{
			var data = new byte [length];
			var read = 0;
			try {
				var downloader = GCHandle.FromIntPtr (user).Target as DownloadHelper;
				read = downloader?.Read (data, 0, length) ?? 0;
			} catch (Exception ex) {
				LogManager.Shared.Report (ex);
			}
			if (read > 0)
				Marshal.Copy (data, 0, buffer, read);
			return read;
		}


		bool OnFileSeek (long offset, IntPtr user)
		{
			return true;
		}


		void OnTrackEnd (int handle, int channel, int data, IntPtr user)
		{

		}

		void OnBuffering (int handle, int channel, int data, IntPtr user)
		{
			State = Models.PlaybackState.Buffering;
		}

		void RemoveHandles ()
		{
			if (StreamIsValid) {
				Bass.ChannelRemoveSync (streamHandle, bufferSync);
				Bass.ChannelRemoveSync (streamHandle, endSync);
			}
			streamHandle = 0;

		}

		int fxStream;
		int fxEq;
		public override void ApplyEqualizer (Equalizer.Band [] bands)
		{
			if (!IsPlayerItemValid)
				return;
			if (fxStream == streamHandle) {
				for (int i = 0; i < bands.Length; i++) {
					UpdateBand (i, bands [i].Gain);
				}
				return;
			}
			fxStream = streamHandle;

			fxEq = Bass.ChannelSetFX (fxStream, EffectType.PeakEQ, 1);
			if (fxEq == 0) {
				fxStream = 0;
				Console.WriteLine (Bass.LastError);
				return;
			}

			eq.fQ = 0f;
			eq.fBandwidth = .6f;
			eq.lChannel = FXChannelFlags.All;

			for (int i = 0; i < bands.Length; i++) {
				eq.lBand = i;
				eq.fCenter = bands [i].Center;
				eq.fGain = bands [i].Gain;
				Console.WriteLine (eq.fCenter);
				Bass.FXSetParameters (fxEq, eq);
			}
		}
		readonly PeakEQParameters eq = new PeakEQParameters ();
		public override void ApplyEqualizer ()
		{
			ApplyEqualizer (Equalizer.Shared.Bands);
		}

		public override void UpdateBand (int band, float gain)
		{
			if (fxEq == 0)
				return;
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