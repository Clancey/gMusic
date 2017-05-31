using System;
using System.Threading.Tasks;
#if BASS
using ManagedBass;
using ManagedBass.DirectX8;
using MusicPlayer.Models;
using System.IO;
using System.Threading;
using MusicPlayer.Playback;
using AudioToolbox;
using ObjCRuntime;
using System.Runtime.InteropServices;
using MusicPlayer.Managers;
namespace MusicPlayer
{
	/// <summary>
	/// Wrapper around OutputQueue and AudioFileStream to allow streaming of various filetypes
	/// </summary>
	public class BassPlayer : Player
	{
		ManagedBass.PlaybackState state;
		int streamHandle;
		SyncProcedure bufferSync;
		int bufferSyncHandle;
		SyncProcedure endSync;
		int endSyncHandle;
		FileProcedures fileProcs;
		PlaybackData currentData;

		public override bool IsPlayerItemValid  => StreamIsValid; 

		bool StreamIsValid => streamHandle != 0;

		public override float Rate => throw new NotImplementedException ();

		public override float Volume { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

		public override double CurrentTimeSeconds () => StreamIsValid ? Bass.ChannelBytes2Seconds (streamHandle, Bass.ChannelGetPosition (streamHandle)) : 0;

		public override void Dispose ()
		{
			RemoveHandles ();
			Stop ();
		}

		public override double Duration ()
		{
			throw new NotImplementedException ();
		}

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
			Bass.Start ();
			Bass.ChannelPlay (streamHandle);
			Bass.ChannelSlideAttribute (streamHandle, ChannelAttribute.Volume, 1, 20);
			throw new NotImplementedException ();
		}

		public override async Task<bool> PlaySong (Song song, bool isVideo, bool forcePlay = false)
		{
			throw new NotImplementedException ();
		}

		public override async Task<bool> PrepareData (PlaybackData playbackData)
		{
			throw new NotImplementedException ();
		}

		public override void Seek (double time)
		{
			throw new NotImplementedException ();
		}

		[MonoPInvokeCallback (typeof (FileCloseProcedure))]
		static void OnFileClose (IntPtr user)
		{

		}

		[MonoPInvokeCallback (typeof (FileLengthProcedure))]
		static long OnFileLength (IntPtr user)
		{
			try {
				var downloader = GCHandle.FromIntPtr (user).Target as DownloadHelper;
				return downloader?.Length ?? 0;
			} catch (Exception ex) {
				LogManager.Shared.Report (ex);
			}

			return 0;
		}

		[MonoPInvokeCallback (typeof (FileReadProcedure))]
		static int OnFileRead (IntPtr buffer, int length, IntPtr user)
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


		[MonoPInvokeCallback (typeof (FileSeekProcedure))]
		static bool OnFileSeek (long offset, IntPtr user)
		{
			return true;
		}


		[MonoPInvokeCallback (typeof (SyncProcedure))]
		static void OnTrackEnd (int handle, int channel, int data, IntPtr user)
		{

		}

		void RemoveHandles ()
		{
			if (StreamIsValid) {
				Bass.ChannelRemoveSync (streamHandle, bufferSyncHandle);
				Bass.ChannelRemoveSync (streamHandle, endSyncHandle);
			}
			streamHandle = 0;

		}


		public override void ApplyEqualizer (Equalizer.Band [] bands)
		{
			//foreach (var band in bands) {
			//	if (Bass.FXGetParameters ().BASS_FXGetParameters (fxEq [slider.Tag], eq)) {
			//		eq.fGain = slider.Value / 10;
			//		Bass.BASS_FXSetParameters (fxEq [slider.Tag], eq);
			//	}
			//}
		}

		public override void ApplyEqualizer ()
		{
			throw new NotImplementedException ();
		}
	}
}


#endif