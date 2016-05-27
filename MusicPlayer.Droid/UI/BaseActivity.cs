using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Views;
using Android.Widget;
using MusicPlayer.Droid.Services;
using MusicPlayer.Managers;
using SimpleTables;

namespace MusicPlayer.Droid.UI
{
	[Activity(Label = "BaseActivity")]
	public class BaseActivity : ActionBarCastActivity, IConnectionCallback, IMediaControllerCallBack
	{

		public static MediaBrowserCompat MediaBrowser { get; private set; }
		PlaybackControlsFragment ControlsFragment;
		MediaControllerCallBack callBack;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			if ((int)Build.VERSION.SdkInt >= 21)
			{
				//TODO: Fix color
				var taskDesc = new ActivityManager.TaskDescription(Title,
					BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.ic_launcher_white), Color.DarkGray);
				SetTaskDescription(taskDesc);
			}
			MediaBrowser = new MediaBrowserCompat(this,new ComponentName(this, Java.Lang.Class.FromType((typeof(MusicService)))), new MediaBrowserCallBack {Parent = this}, null );
		}

		protected override void OnStart()
		{
			base.OnStart();
			ControlsFragment = FragmentManager.FindFragmentById<PlaybackControlsFragment>(Resource.Id.fragment_playback_controls);
			if(ControlsFragment == null)
				throw new Exception("Missing fragment id 'Controls'");
			HidePlaybackControls();
			MediaBrowser.Connect();
			Managers.NotificationManager.Shared.PlaybackStateChanged += PlaybackStateChanged;
		}

		void PlaybackStateChanged(object sender, EventArgs<Models.PlaybackState> e)
		{
			if (IsDestroyed)
			{
				Managers.NotificationManager.Shared.PlaybackStateChanged -= PlaybackStateChanged;
				return;
			}
			if (ShouldShowControls())
				ShowPlaybackControls();
			else HidePlaybackControls();
		}

		protected override void OnStop()
		{
			base.OnStop();
			SupportMediaController?.UnregisterCallback(callBack);
			MediaBrowser.Disconnect();
		}

		protected void ShowPlaybackControls()
		{
			if (this.IsDestroyed)
				return;
			try
			{
				//TODO: if online check;
				FragmentManager.BeginTransaction()
					.SetCustomAnimations(Resource.Animator.slide_in_from_bottom, Resource.Animator.slide_out_to_bottom,
						Resource.Animator.slide_in_from_bottom, Resource.Animator.slide_out_to_bottom)
					.Show(ControlsFragment)
					.Commit();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		protected void HidePlaybackControls()
		{
			try{
			FragmentManager.BeginTransaction().Hide(ControlsFragment).Commit();
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		protected bool ShouldShowControls()
		{
			var controller = SupportMediaController;
			if (controller?.Metadata == null || controller?.PlaybackState == null)
				return false;
			var state = controller.PlaybackState.State;
			switch (state)
			{
				case PlaybackStateCompat.StateError:
				case PlaybackStateCompat.StateNone:
				case PlaybackStateCompat.StateStopped:
					return false;
				default:
					return true;
			}
		}

		void ConnectToSession(MediaSessionCompat.Token token)
		{
			var controller = new MediaControllerCompat(this,token);
			this.SupportMediaController = controller;
			controller.RegisterCallback(callBack = new MediaControllerCallBack {Parent = this});
			if(ShouldShowControls())
				ShowPlaybackControls();
			else
				HidePlaybackControls();
			ControlsFragment?.OnConnected();
			OnMediaControllConnected();

		}
		public virtual void OnMediaControllConnected()
		{
			
		}
		public void OnConnected()
		{
			try
			{
				ConnectToSession(MediaBrowser.SessionToken);
			}
			catch (RemoteException e)
			{
				Console.WriteLine(e);
				HidePlaybackControls();
			}
		}

		public virtual void OnConnectionFailed()
		{
			Console.WriteLine("Failed!!");
		}

		public virtual void OnConnectionSuspended()
		{
		}

		public void OnPlaybackStateChanged(PlaybackStateCompat state)
		{
			//Console.WriteLine("************");
			//Console.WriteLine("***************");
			//Console.WriteLine($"Base Activity: Playback State: {state.State} - position {state.Position}");
			//NativeAudioPlayer.Context?.Session?.SetPlaybackState(state);
			//Console.WriteLine("***************");
			//Console.WriteLine("******");
			if (ShouldShowControls())
				ShowPlaybackControls();
			else
				HidePlaybackControls();
		}

		public void OnMetadataChanged(MediaMetadataCompat metadata)
		{
			NativeAudioPlayer.Context?.Session?.SetMetadata(metadata);
		}

		public void OnSessionDestroyed()
		{
			Managers.NotificationManager.Shared.PlaybackStateChanged -= PlaybackStateChanged;
		}
	}

}