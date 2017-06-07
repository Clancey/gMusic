﻿using System;
using MusicPlayer.Managers;
using Foundation;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using CoreFoundation;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ObjCRuntime;
using MediaPlayer;

namespace MusicPlayer
{
	public static class KeyboardControlHandler
	{
		public static void Init ()
		{
			SPMediaKeyTap.Shared.Init ();
			SPMediaKeyTap.Shared.RecievedMediaKeyEvent = KeyPressed;
			SPMediaKeyTap.Shared.StartWatchingMediaKeys ();
		}
		static void KeyPressed(SPMediaKeyTap.MediaKey key)
		{
			switch (key) {
			case SPMediaKeyTap.MediaKey.Play:
				PlaybackManager.Shared.PlayPause ();
				return;
			case SPMediaKeyTap.MediaKey.Next:
			case SPMediaKeyTap.MediaKey.Fast:
				PlaybackManager.Shared.NextTrack ();
				return;
			case SPMediaKeyTap.MediaKey.Previous:
			case SPMediaKeyTap.MediaKey.Rewind:
				PlaybackManager.Shared.Previous ();
				return;
			}
		}
	}



	public class SPMediaKeyTap : NSObject
	{
		public SPMediaKeyTap()
		{

		}
		public SPMediaKeyTap(IntPtr handle) : base(handle)
		{

		}
		public static SPMediaKeyTap Shared { get; set; } = new SPMediaKeyTap();

		public Action<MediaKey> RecievedMediaKeyEvent { get; set; }

		public void Init ()
		{
			StartWatchingAppSwitching ();
		}

		public static bool UseMediaKeys {
			get {
				return true;
				return !Debugger.IsAttached;
			}
		}

		static string[] DefaultMediaKeyUsers = new string[] {
			NSBundle.MainBundle.BundleIdentifier,
			@"com.spotify.client",
			@"com.apple.iTunes",
			@"com.apple.QuickTimePlayerX",
			@"com.apple.quicktimeplayer",
			@"com.apple.iWork.Keynote",
			@"com.apple.iPhoto",
			@"org.videolan.vlc",
			@"com.apple.Aperture",
			@"com.plexsquared.Plex",
			@"com.soundcloud.desktop",
			@"org.niltsh.MPlayerX",
			@"com.ilabs.PandorasHelper",
			@"com.mahasoftware.pandabar",
			@"com.bitcartel.pandorajam",
			@"org.clementine-player.clementine",
			@"fm.last.Last.fm",
			@"fm.last.Scrobbler",
			@"com.beatport.BeatportPro",
			@"com.Timenut.SongKey",
			@"com.macromedia.fireworks", // the tap messes up their mouse input
			@"at.justp.Theremin",
			@"ru.ya.themblsha.YandexMusic",
			@"com.jriver.MediaCenter18",
			@"com.jriver.MediaCenter19",
			@"com.jriver.MediaCenter20",
			@"co.rackit.mate",
			@"com.ttitt.b-music",
			@"com.beardedspice.BeardedSpice",
			@"com.plug.Plug",
			@"com.plug.Plug2",
			@"com.netease.163music",
		};

		NSObject appSwitchObserver;
		NSObject appTerminated;
		public CFMachPort eventPort;
		CFRunLoop tapThread;
		CFRunLoopSource eventPortSource;

		static  uint NX_SYSDEFINED = 14;
		const int SPSystemDefinedEventMediaKeys = 8;

		static NSString kMediaKeyUsingBundleIdentifiersDefaultsKey = (NSString)"SPApplicationsNeedingMediaKeys";
		static NSString kIgnoreMediaKeysDefaultsKey = (NSString)"SPIgnoreMediaKeys";

		public void StartWatchingAppSwitching ()
		{
			appSwitchObserver = NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver (NSWorkspace.DidActivateApplicationNotification, AppSwitched);
			appTerminated = NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver (NSWorkspace.DidTerminateApplicationNotification, AppTerminated);
			//Carbon.InstallApplicationEventHandler (AppSwitched, new CarbonEventTypeSpec[]{new CarbonEventTypeSpec(CarbonEventApple. });
		}

		public void StopWatchingAppSwitching ()
		{
			if (appSwitchObserver != null)
				NSWorkspace.SharedWorkspace.NotificationCenter.RemoveObserver (appSwitchObserver);
			if (appTerminated != null)
				NSWorkspace.SharedWorkspace.NotificationCenter.RemoveObserver (appTerminated);
		}

		public void StartWatchingMediaKeys ()
		{
			StopWatchingMediaKeys ();
			var sysMask = (CGEventMask)NX_SYSDEFINED;
			eventPort = CreateTap (CGEventTapLocation.Session, CGEventTapPlacement.HeadInsert, CGEventTapOptions.Default, 16384, tapEventCallback, this.Handle);
			eventPortSource = eventPort.CreateRunLoopSource ();
			Task.Run (() => {
				eventTapThread ();
			});
			ShouldInterceptMediaKeyEvents = true;
		}

		[DllImport (Constants.ApplicationServicesCoreGraphicsLibrary)]
		public extern static IntPtr CGEventTapCreate (CGEventTapLocation location, CGEventTapPlacement place, CGEventTapOptions options, uint mask, CoreGraphics.CGEvent.CGEventTapCallback cback, IntPtr data);

		static CFMachPort CreateTap (CGEventTapLocation location, CGEventTapPlacement place, CGEventTapOptions options, uint mask, CoreGraphics.CGEvent.CGEventTapCallback cback, IntPtr data)
		{
			var r = CGEventTapCreate (location, place, options, mask, cback, data);
			if (r == IntPtr.Zero)
				return null;
			return new CFMachPort (r);
		}

		public void StopWatchingMediaKeys ()
		{
			if (tapThread != null) {
				tapThread.Stop ();
				tapThread = null;
			}

			if (eventPort != null) {
				eventPort.Invalidate ();
				eventPort.Dispose ();
				eventPort = null;
			}

			if (eventPortSource != null) {
				eventPortSource.Invalidate ();
				eventPortSource.Dispose ();
				eventPortSource = null;
			}
		}

		bool pauseTapOnTapThread;
		public bool PauseTapOnTapThread {
			get {
				return pauseTapOnTapThread;
			}
			set {
				pauseTapOnTapThread = value;
				if (value)
					CGEvent.TapEnable (eventPort);
				else
					CGEvent.TapDisable (eventPort);
			}
		}

		bool shouldInterceptMediaKeyEvents = true;
		public bool ShouldInterceptMediaKeyEvents {
			get {
				return shouldInterceptMediaKeyEvents;
			}
			set {
				var oldSetting = shouldInterceptMediaKeyEvents;
				shouldInterceptMediaKeyEvents = value;
				if (tapThread == null || oldSetting == value)
					return;
				var grab = this.Grab ();
				PauseTapOnTapThread = value;
				Task.Run (() => {
					grab.Invoke();
				});
			}
		}


		void eventTapThread ()
		{
			tapThread = CFRunLoop.Current;
			tapThread.AddSource (eventPortSource, CFRunLoop.ModeCommon);
			tapThread.Run ();
		}

		public void RecievedEvent(MediaKey mediaKey)
		{
			RecievedMediaKeyEvent?.Invoke (mediaKey);
		}


		const long NX_KEYTYPE_PLAY = 16;
		const long NX_KEYTYPE_NEXT = 17;
		const long NX_KEYTYPE_PREVIOUS = 18;
		const long NX_KEYTYPE_FAST = 19;
		const long NX_KEYTYPE_REWIND = 20;
		const long kCGEventTapDisabledByTimeout = 0xFFFFFFFE;
		const long kCGEventTapDisabledByUser = 0xFFFFFFFF;
		public enum MediaKey
		{
			Play = 16,
			Next = 17,
			Previous = 18,
			Fast = 19,
			Rewind = 20,

		}


		static long lastKeyCode;
		static IntPtr tapEventCallback (IntPtr proxy, CGEventType type, IntPtr evtHandle, IntPtr info)
		{
			using (new NSAutoreleasePool ()) {
				var evt = new CGEvent (evtHandle);
				if (!SPMediaKeyTap.Shared.ShouldInterceptMediaKeyEvents)
					return evtHandle;
				switch (type) {
				//kCGEventTapDisabledByTimeout
				case  (CGEventType)kCGEventTapDisabledByTimeout:
					CGEvent.TapEnable (SPMediaKeyTap.Shared.eventPort);
					return evtHandle;
				case (CGEventType)kCGEventTapDisabledByUser:
					return evtHandle;
				}

				NSEvent nsEvent = null;
				try{
					nsEvent = NSEvent.EventWithCGEvent(evtHandle);
				}
				catch(Exception ex) {
					return evtHandle;
				}

				if (nsEvent.Subtype != SPSystemDefinedEventMediaKeys)
					return evtHandle;
				long keyCode = ((nsEvent.Data1 & 0xFFFF0000) >> 16);

				var keyFlags = (nsEvent.Data1 & 0x0000FFFF);

				var keyState = (((keyFlags & 0xFF00) >> 8)) == 0xA;
				if (keyCode == 10 && keyFlags == 6972) {

					switch (nsEvent.Data2) {
					case 786608: // Play / Pause on OS < 10.10 Yosemite
					case 786637: // Play / Pause on OS >= 10.10 Yosemite
						Console.WriteLine (@"Play/Pause bluetooth keypress detected...sending corresponding media key event");
						SPMediaKeyTap.Shared.RecievedEvent (MediaKey.Play);
						break;
					case 786611: // Next
						Console.WriteLine (@"Next bluetooth keypress detected...sending corresponding media key event");
						SPMediaKeyTap.Shared.RecievedEvent (MediaKey.Next);
						break;
					case 786612: // Previous
						Console.WriteLine (@"Previous bluetooth keypress detected...sending corresponding media key event");
						SPMediaKeyTap.Shared.RecievedEvent (MediaKey.Previous);
						break;
					case 786613: // Fast-forward
						Console.WriteLine (@"Fast-forward bluetooth keypress detected...sending corresponding media key event");
						SPMediaKeyTap.Shared.RecievedEvent (MediaKey.Fast);
						break;
					case 786614: // Rewind
						Console.WriteLine (@"Rewind bluetooth keypress detected...sending corresponding media key event");
						SPMediaKeyTap.Shared.RecievedEvent (MediaKey.Rewind);
						break;
					default:
						// TODO make this popup a message in the UI (with a link to submit the issue and a "don't show this message again" checkbox)
						LogManager.Shared.Log($"Unknown bluetooth key received: keyCode:{keyCode} keyFlags:{keyFlags} keyState:{keyState} {nsEvent.Data2}");
						return evtHandle;
					}			
				} else {
					if (keyCode != NX_KEYTYPE_PLAY && keyCode != NX_KEYTYPE_FAST && keyCode != NX_KEYTYPE_REWIND && keyCode != NX_KEYTYPE_PREVIOUS && keyCode != NX_KEYTYPE_NEXT)
						return evtHandle;
					//These always come in pairs
					if (lastKeyCode != keyCode) {
						SPMediaKeyTap.Shared.BeginInvokeOnMainThread (() => {
							SPMediaKeyTap.Shared.RecievedEvent ((MediaKey)keyCode);
						});
						lastKeyCode = keyCode;
					} else {
						lastKeyCode = 0;
					}
				}
				evt.Dispose ();
				return IntPtr.Zero;
			}
		}

		void AppSwitched (NSNotification notification)
		{

		}

		void AppTerminated (NSNotification notification)
		{

		}
	}

	public class SPInvocationGrabber : NSObject
	{
		NSObject obj;
		NSInvocation invocation;

		public SPInvocationGrabber (NSObject obj)
		{
			this.obj = obj;
		}

		void RunInBackground ()
		{
			using (new NSAutoreleasePool ()) {

				try {
					Invoke ();
				} catch (Exception ex) {

				}
			}
		}

		public bool BackgroundAfterForward, OnMainAfterForward, WaitUntilDone;

		public void ForwardInvocation (NSInvocation anInvocation)
		{
			anInvocation.Target = obj;
			invocation = anInvocation;
			if (BackgroundAfterForward) {
				Task.Run (()=> RunInBackground());
				return;
			}

			if (WaitUntilDone)
				this.InvokeOnMainThread (Invoke);
			else
				this.BeginInvokeOnMainThread (Invoke);

		}


		public void Invoke ()
		{
			try {
				this.invocation.Invoke ();

			} catch (Exception ex) {
				Console.WriteLine (ex);
			} finally {
				this.invocation = null;
				obj = null;
			}
		}

	}

	public static class SPInvokationExtensions
	{
		public static SPInvocationGrabber Grab (this NSObject obj)
		{
			return new SPInvocationGrabber (obj);
		}

		public static SPInvocationGrabber InvokeAfter (this NSObject obj, double seconds)
		{
			var grabber = obj.Grab ();
			NSTimer.CreateScheduledTimer (seconds, (t) => grabber.Invoke ());
			return grabber;
		}

		public static SPInvocationGrabber InvokeAfter (this NSObject obj, TimeSpan span)
		{
			var grabber = obj.Grab ();
			NSTimer.CreateScheduledTimer (span, (t) => grabber.Invoke ());
			return grabber;
		}

		public static SPInvocationGrabber NextRunLoop (this NSObject obj)
		{
			return obj.InvokeAfter (0);
		}

		public static SPInvocationGrabber InBackground (this NSObject obj)
		{
			var grabber = obj.Grab ();
			grabber.BackgroundAfterForward = true;
			return grabber;
		}

		public static SPInvocationGrabber OnMain (this NSObject obj, bool isAsync)
		{
			var grabber = obj.Grab ();
			grabber.OnMainAfterForward = true;
			grabber.WaitUntilDone = !isAsync;
			return grabber;
		}
	}
}

