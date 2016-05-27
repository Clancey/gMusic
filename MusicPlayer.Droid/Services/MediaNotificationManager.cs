
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Java.Lang;
using Java.Util.Logging;
using MusicPlayer.Droid.UI;

namespace MusicPlayer.Droid.Services
{
	[BroadcastReceiver]
	[IntentFilter(new[] { ActionPlay, ActionPause, ActionStopCasting, ActionNext, ActionPrevious })]
	public class MediaNotificationManager : BroadcastReceiver, IMediaControllerCallBack
	{
		const int NotificationId = 412;
		const int RequestCode = 100;

		public const string ActionPause = "com.IIS.gMusic.Pause";
		public const string ActionPlay = "com.IIS.gMusic.Play";
		public const string ActionPrevious = "com.IIS.gMusic.Previous";
		public const string ActionNext = "com.IIS.gMusic.Next";
		public const string ActionStopCasting = "com.IIS.gMusic.StopCasting";

		public MusicService Service { get; set; }

		MediaSessionCompat.Token sessionToken;
		MediaControllerCompat controller;
		MediaControllerCompat.TransportControls transportControls;
		PlaybackStateCompat playbackState;
		MediaMetadataCompat metadata;

		NotificationManagerCompat NotificationManager;

		PendingIntent pauseIntent;
		PendingIntent playIntent;
		PendingIntent previousIntent;
		PendingIntent nextIntent;

		PendingIntent stopCastingIntent;

		MediaControllerCallBack callback = new MediaControllerCallBack();

		int notificationColor;
		bool started;


		public MediaNotificationManager()
		{

		}
		public MediaNotificationManager(MusicService service)
		{
			callback.Parent = this;
			Service = service;
			UpdateSessionToken();

			NotificationManager = NotificationManagerCompat.From(service);

			var pkg = service.PackageName;

			pauseIntent = PendingIntent.GetBroadcast(service,RequestCode,new Intent(ActionPause).SetPackage(pkg),PendingIntentFlags.CancelCurrent );
			playIntent = PendingIntent.GetBroadcast(service, RequestCode, new Intent(ActionPlay).SetPackage(pkg), PendingIntentFlags.CancelCurrent);
			previousIntent = PendingIntent.GetBroadcast(service, RequestCode, new Intent(ActionPrevious).SetPackage(pkg), PendingIntentFlags.CancelCurrent);
			nextIntent = PendingIntent.GetBroadcast(service, RequestCode, new Intent(ActionNext).SetPackage(pkg), PendingIntentFlags.CancelCurrent);
			stopCastingIntent = PendingIntent.GetBroadcast(service, RequestCode, new Intent(ActionStopCasting).SetPackage(pkg), PendingIntentFlags.CancelCurrent);
			NotificationManager.CancelAll();

		}

		public void StartNotification()
		{
			if (started)
				return;
			notificationColor = ResourceHelper.GetThemeColor(Service.ApplicationContext, Resource.Attribute.colorPrimary, Color.DarkGray);
			metadata = controller.Metadata;
			playbackState = controller.PlaybackState;

			var notification = CreateNotification();
			if (notification == null)
				return;

			started = true;
			controller.RegisterCallback(callback);
			var filter = new IntentFilter();
			filter.AddAction(ActionNext);
			filter.AddAction(ActionPause);
			filter.AddAction(ActionPlay);
			filter.AddAction(ActionPrevious);
			filter.AddAction(ActionStopCasting);

			Service.RegisterReceiver(this, filter);
			Service.StartForeground(NotificationId,notification);
		}

		public void StopNotification()
		{
			if(!started)
				return;
			started = false;

			controller.UnregisterCallback(callback);
			try
			{
				NotificationManager.Cancel(NotificationId);
				Service.UnregisterReceiver(this);
			}
			catch (IllegalArgumentException ex)
			{

			}
			Service.StopForeground(true);
		}
		public override void OnReceive(Context context, Intent intent)
		{
			string action = intent.Action;
			Console.WriteLine($"Recieved intent with action {action}");
			switch (action)
			{
				case ActionPlay:
					transportControls.Play();
					break;
				case ActionNext:
					transportControls.SkipToNext();
					break;
				case ActionPause:
					transportControls.Pause();
					break;
				case ActionPrevious:
					transportControls.SkipToPrevious();
					break;
				case ActionStopCasting:
					var i = new Intent(context, typeof (MusicService));
					i.SetAction(MusicService.ActionCmd);
					i.PutExtra(MusicService.CmdName, MusicService.CmdStopCasting);
					Service.StartService(i);
					break;
			}
			Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();
		}

		void UpdateSessionToken()
		{
			var freshToken = Service.SessionToken;
			if (freshToken == null || sessionToken == freshToken)
				return;

			controller?.UnregisterCallback(callback);

			sessionToken = freshToken;
			controller = new MediaControllerCompat(Service, sessionToken);
			transportControls = controller.GetTransportControls();
			
			if(started)
				controller.RegisterCallback(callback);
		}

		PendingIntent CreateContentIntent(MediaDescriptionCompat description)
		{
			var intent = new Intent(Service, typeof (MusicPlayerActivity));
			intent.SetFlags(ActivityFlags.SingleTop);
			intent.PutExtra(MusicPlayerActivity.EXTA_START_FULLSCREEN, true);
			if (description != null)
			{
				intent.PutExtra(MusicPlayerActivity.CurrentMediaDescription, description);
			}
			return PendingIntent.GetActivity(Service,RequestCode,intent,PendingIntentFlags.CancelCurrent);
		}

		public void OnPlaybackStateChanged(PlaybackStateCompat state)
		{
			playbackState = state;
			if (state.State == PlaybackStateCompat.StateStopped || state.State == PlaybackStateCompat.StateNone)
			{
				StopNotification();
			}
			else
			{
				var notification = CreateNotification();
				if(notification != null)
					NotificationManager.Notify(NotificationId,notification);
			}
		}

		public void OnMetadataChanged(MediaMetadataCompat metadata)
		{
			this.metadata = metadata;
			var notification = CreateNotification();
			if (notification != null)
				NotificationManager.Notify(NotificationId, notification);

		}

		public void OnSessionDestroyed()
		{
			try
			{
				UpdateSessionToken();
			}
			catch (RemoteException e)
			{
				System.Console.WriteLine(e);
			}
		}

		Notification CreateNotification()
		{
			if (metadata == null || playbackState == null)
				return null;

			var notificationBuilder = new NotificationCompat.Builder(Service);
			var playButtonPosition = 0;

			if ((playbackState.Actions & PlaybackStateCompat.ActionSkipToPrevious) != 0)
			{
				notificationBuilder.AddAction(Resource.Drawable.ic_skip_previous_white_24dp, "Previous", previousIntent);
				playButtonPosition = 1;
			}

			AddPlayPauseButton(notificationBuilder);

			if ((playbackState.Actions & PlaybackStateCompat.ActionSkipToNext) != 0)
			{
				notificationBuilder.AddAction(Resource.Drawable.ic_skip_next_white_24dp, "Next", nextIntent);
			}

			var description = metadata.Description;
			string fetchArtUrl = null;
			Bitmap art = null;

			if (description.IconUri != null)
			{
				var artUrl = description.IconUri.ToString();
				//TODO: get art from cache
				if (art == null)
				{
					fetchArtUrl = artUrl;
					art = BitmapFactory.DecodeResource(Service.Resources, Resource.Drawable.ic_default_art);
				}
			}
			else
			{
				art = BitmapFactory.DecodeResource(Service.Resources, Resource.Drawable.ic_default_art);
			}

			notificationBuilder.SetStyle(new Android.Support.V7.App.NotificationCompat.MediaStyle()
				.SetShowActionsInCompactView(new[] {playButtonPosition}).SetMediaSession(sessionToken))
			                   .SetColor(notificationColor)
				.SetSmallIcon(Resource.Drawable.ic_notification)
				.SetVisibility(NotificationCompat.VisibilityPublic)
				.SetUsesChronometer(true)
				.SetContentIntent(CreateContentIntent(description))
				.SetContentTitle(description.Title)
				.SetContentText(description.Subtitle)
				.SetLargeIcon(art);

			if (controller != null && controller.Extras != null)
			{
				var castName = controller.Extras.GetString(MusicService.ExtraConnectedCast);
				if (string.IsNullOrWhiteSpace(castName))
				{
					var castInfo = $"Casting to {castName}";
					notificationBuilder.SetSubText(castInfo);
					notificationBuilder.AddAction(Resource.Drawable.ic_close_black_24dp, "Stop Casting", stopCastingIntent);
				}
			}

			SetNotificationPlaybackState(notificationBuilder);

			if (!string.IsNullOrWhiteSpace(fetchArtUrl))
			{
				//TODO: Fetch url
			}

			return notificationBuilder.Build();
		}

		void AddPlayPauseButton(NotificationCompat.Builder builder)
		{
			var label = "";
			int icon;
			PendingIntent intent;

			if (playbackState.State == PlaybackStateCompat.StatePlaying)
			{
				label = "Pause";
				icon = Resource.Drawable.ic_allmusic_black_24dp;
				intent = pauseIntent;
			}
			else
			{
				label = "Play";
				icon = Resource.Drawable.ic_allmusic_black_24dp;
				intent = playIntent;
			}
			builder.AddAction(new NotificationCompat.Action(icon, label, intent));
		}

		void SetNotificationPlaybackState(NotificationCompat.Builder builder)
		{
			if (playbackState == null || !started)
			{
				Service.StopForeground(true);
				return;
			}

			if (playbackState.State == PlaybackStateCompat.StatePlaying)
			{
				builder.SetWhen(SystemClock.CurrentThreadTimeMillis() - playbackState.Position)
					.SetShowWhen(true)
					.SetUsesChronometer(true);
			}
			else
			{
				builder.SetWhen(0)
					.SetShowWhen(false)
					.SetUsesChronometer(false);
			}
			builder.SetOngoing(playbackState.State == PlaybackStateCompat.StatePlaying);
		}
	}
}