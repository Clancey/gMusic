using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Views;
using Android.Widget;
using MusicPlayer.Data;
using MusicPlayer.Droid.Playback;
using MusicPlayer.Managers;
using MusicPlayer.Models;

namespace MusicPlayer.Droid.Services
{
	[Service]
	public class MusicService : MediaBrowserServiceCompat
	{
		public static MusicService Shared { get; set; }

		public const string ExtraConnectedCast = "com.IIS.gMusic.CAST_NAME";
		public const string ActionCmd = "com.IIS.gMusic.ACTION_CMD";
		public const string CmdName = "CMD_NAME";
		public const string CmdPause = "CMD_PAUSE";
		public const string CmdStopCasting = "CMD_STOP_CASTING";
		public const int StopDelay = 30000;

		public MediaSessionCompat Session;
		public MediaNotificationManager MediaNotificationManager;
		Bundle sessionExtras;
		//DelayedStopHandler

		Android.Support.V7.Media.MediaRouter mediaRouter;
		//PackageValidator
		bool isConectedToCar;
		BroadcastReceiver carConnectionReciever;
		MediaSessionCallback sessionCallback;

		public override void OnCreate()
		{
			base.OnCreate();
			Shared = this;
            NativeAudioPlayer.NativeInit(this);
			PlaybackManager.Shared.Init();
			Session = new MediaSessionCompat(this, "MusicService");
			SessionToken = Session.SessionToken;
			Session.SetCallback(sessionCallback = new MediaSessionCallback());
			Session.SetFlags(MediaSessionCompat.FlagHandlesMediaButtons |
			                 MediaSessionCompat.FlagHandlesTransportControls);
			var  context = ApplicationContext;
			var intent = new Intent(ApplicationContext, typeof(NowPlayingActivity));
			var pi = PendingIntent.GetActivity(context, 99, intent, PendingIntentFlags.UpdateCurrent);
			Session.SetSessionActivity(pi);

			sessionExtras = new Bundle();

			//		CarHelper.setSlotReservationFlags(mSessionExtras, true, true, true);
			//        WearHelper.setSlotReservationFlags(mSessionExtras, true, true);
			//        WearHelper.setUseBackgroundFromTheme(mSessionExtras, true);

			Session.SetExtras(sessionExtras);


			try
			{
				MediaNotificationManager = new MediaNotificationManager(this);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				throw ex;
			}

			//VideoCastManager.getInstance().addVideoCastConsumer(mCastConsumer);
			mediaRouter = Android.Support.V7.Media.MediaRouter.GetInstance(ApplicationContext);

			//registerCarConnectionReceiver();
			Managers.NotificationManager.Shared.PlaybackStateChanged += PlaybackStateChanged;
		}

		public void PlaybackStateChanged(object sender, SimpleTables.EventArgs<Models.PlaybackState> args)
		{
			switch (args.Data)
			{
				case Models.PlaybackState.Paused:
				case Models.PlaybackState.Stopped:
					PlaybackStoped();
					break;
				default:
					PlaybackStart();
					break;
			}
		}

		void PlaybackStart()
		{

			if (!Session.Active)
				Session.Active = true;
			// The service needs to continue running even after the bound client (usually a
			// MediaController) disconnects, otherwise the music playback will stop.
			// Calling startService(Intent) will keep the service running until it is explicitly killed.
			StartService(new Intent(ApplicationContext,  typeof(MusicService)));
		}
		void PlaybackStoped()
		{
			// Reset the delayed stop handler, so after STOP_DELAY it will be executed again,
			// potentially stopping the service.
			//mDelayedStopHandler.removeCallbacksAndMessages(null);
			//mDelayedStopHandler.sendEmptyMessageDelayed(0, STOP_DELAY);
			//stopForeground(true);

		}
		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			if (intent != null)
			{
				var action = intent.Action;
				var command = intent.GetStringExtra(CmdName);
				if (action == ActionCmd)
				{
					if (command == CmdPause)
						PlaybackManager.Shared.Pause();
					else if (command == CmdStopCasting)
					{
						//VideoCastManager.getInstance().disconnect();
					}
				}
				else
					MediaButtonReceiver.HandleIntent(Session, intent);
				// Reset the delay handler to enqueue a message to stop the service if
				// nothing is playing.
				//mDelayedStopHandler.removeCallbacksAndMessages(null);
				//mDelayedStopHandler.sendEmptyMessageDelayed(0, STOP_DELAY);
			}
			return StartCommandResult.Sticky;
		}
		public override void OnDestroy()
		{
			//unregisterCarConnectionReceiver();
			//// Service is being killed, so make sure we release our resources
			//mPlaybackManager.handleStopRequest(null);
			MediaNotificationManager.StopNotification();
			//VideoCastManager.getInstance().removeVideoCastConsumer(mCastConsumer);
			//mDelayedStopHandler.removeCallbacksAndMessages(null);
			Session.Release();
			base.OnDestroy();
		}
		public override BrowserRoot OnGetRoot(string p0, int p1, Bundle p2)
		{

			return new BrowserRoot("__ROOT__", null);
		}

		public override void OnLoadChildren(string p0, Result p1)
		{
			
		}

	}
}