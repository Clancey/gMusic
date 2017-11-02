using System;
using ObjCRuntime;
using ManagedBass;
using System.Collections.Generic;
using MusicPlayer.Managers;
namespace MusicPlayer.Playback
{
	public static class BassFileProceduresManager
	{
		static Dictionary<IntPtr, FileProcedures> FileProcs = new Dictionary<IntPtr, FileProcedures>();
		static Dictionary<IntPtr, SyncProcedure> SyncProcs = new Dictionary<IntPtr, SyncProcedure>();
		public static void ClearProcedure(IntPtr user)
		{
			if(FileProcs.ContainsKey(user))
				FileProcs.Remove(user);
			if (SyncProcs.ContainsKey(user))
				SyncProcs.Remove(user);
		}
#if !__IOS__
		public static (FileProcedures proc, IntPtr user) CreateProcedure(FileProcedures proc, IntPtr user = default(IntPtr))
		{
			return (proc,user);
		}
		public static (SyncProcedure proc, IntPtr user) CreateProcedure(SyncProcedure proc, IntPtr user = default(IntPtr))
		{
			return (proc,user);
		}
#else
		static FileProcedures FileProcWrapper = new FileProcedures
		{
			Close = OnFileClose,
			Length = OnFileLength,
			Read = OnFileRead,
			Seek = OnFileSeek,
		};

		static int currentFileProc = 0;
		static int currenSyncProc = 0;
		public static (FileProcedures proc, IntPtr user) CreateProcedure(FileProcedures proc, IntPtr user = default(IntPtr))
		{
			if (user == default(IntPtr))
			{
				user = new IntPtr(currentFileProc++);
			}
			if (currentFileProc > 1000)
				currentFileProc = 0;
			FileProcs[user] = proc;
			return (FileProcWrapper, user);
		}
		static SyncProcedure SyncProcWrapper = new SyncProcedure(OnSyncProc);
		public static (SyncProcedure proc, IntPtr user) CreateProcedure(SyncProcedure proc, IntPtr user = default(IntPtr))
		{
			if (user == default(IntPtr))
			{
				user = new IntPtr(currenSyncProc++);
			}
			if (currenSyncProc > 1000)
				currenSyncProc = 0;
			SyncProcs[user] = proc;
			return (SyncProcWrapper,user);
		}


		[MonoPInvokeCallback(typeof(FileCloseProcedure))]
		static void OnFileClose(IntPtr user)
		{
			if (!FileProcs.TryGetValue(user, out var proc))
				return;
			proc.Close(user);
		}

		[MonoPInvokeCallback(typeof(FileLengthProcedure))]
		static long OnFileLength(IntPtr user)
		{
			try
			{

				if (!FileProcs.TryGetValue(user, out var proc))
					return 0;
				return proc.Length(user);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}

			return 0;
		}

		[MonoPInvokeCallback(typeof(FileReadProcedure))]
		static int OnFileRead(IntPtr buffer, int length, IntPtr user)
		{
			try
			{
				if (!FileProcs.TryGetValue(user, out var proc))
					return 0;
				return proc.Read(buffer,length,user);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(FileSeekProcedure))]
		static bool OnFileSeek(long offset, IntPtr user)
		{
			try
			{
				if (!FileProcs.TryGetValue(user, out var proc))
					return false;
				return proc.Seek(offset, user);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		[MonoPInvokeCallback(typeof(SyncProcedure))]
		public static void OnSyncProc(int handle, int channel, int data, IntPtr user)
		{
			if (!SyncProcs.TryGetValue(user, out var proc))
				return;
			proc.Invoke(handle,channel,data, user);
		}

#endif
	}
}
