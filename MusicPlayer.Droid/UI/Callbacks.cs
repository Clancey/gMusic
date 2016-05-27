using System;
using Android.OS;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using MusicPlayer.Managers;

namespace MusicPlayer.Droid
{


	class MediaControllerCallBack : MediaControllerCompat.Callback
	{
		WeakReference parent;

		public IMediaControllerCallBack Parent
		{
			get { return parent?.Target as IMediaControllerCallBack; }
			set { parent = new WeakReference(value); }
		}

		public override void OnPlaybackStateChanged(PlaybackStateCompat state)
		{
			Parent?.OnPlaybackStateChanged(state);
		}

		public override void OnMetadataChanged(MediaMetadataCompat metadata)
		{
			Parent?.OnMetadataChanged(metadata);
		}

		public override void OnSessionDestroyed()
		{
			base.OnSessionDestroyed();
			Parent?.OnSessionDestroyed();
		}
	}
	interface IMediaControllerCallBack
	{
		void OnPlaybackStateChanged(PlaybackStateCompat state);
		void OnMetadataChanged(MediaMetadataCompat metadata);

		void OnSessionDestroyed();
	}


    class MediaBrowserCallBack : MediaBrowserCompat.ConnectionCallback
	{
		WeakReference parent;

		public IConnectionCallback Parent
		{
			get { return parent?.Target as IConnectionCallback; }
			set { parent = new WeakReference(value); }
		}

		public override void OnConnected()
		{
			Parent?.OnConnected();
		}

		public override void OnConnectionFailed()
		{
			Parent?.OnConnectionFailed();
		}

		public override void OnConnectionSuspended()
		{
			Parent?.OnConnectionSuspended();
		}
	}
	interface IConnectionCallback
	{
		void OnConnected();

		void OnConnectionFailed();

		void OnConnectionSuspended();
	}

	class MediaSessionCallback : MediaSessionCompat.Callback
	{
		WeakReference parent;

		public IMediaControllerCallBack Parent
		{
			get { return parent?.Target as IMediaControllerCallBack; }
			set { parent = new WeakReference(value); }
		}
		public override void OnPlay()
		{
			PlaybackManager.Shared.Play();
		}
		public override void OnPlayFromMediaId(string mediaId, Android.OS.Bundle extras)
		{
			PlaybackManager.Shared.NativePlayer.PlaySong(mediaId);
			//base.OnPlayFromMediaId(mediaId, extras);
		}
		public override void OnPlayFromSearch(string query, Android.OS.Bundle extras)
		{
			base.OnPlayFromSearch(query, extras);
		}
		public override void OnStop()
		{
			PlaybackManager.Shared.Pause();
		}
		public override void OnPause()
		{
			PlaybackManager.Shared.Pause();
		}
		public override void OnSkipToNext()
		{
			PlaybackManager.Shared.NextTrack();
		}

		//public override void OnSetRating(RatingCompat rating)
		//{
		//	rating.
		//	MusicManager.Shared.ThumbsDown(MusicManager.Shared.GetCurrentSong());
		//}

		public override void OnSeekTo(long pos)
		{
			//PlaybackManager.Shared.NativePlayer.Duration
			//PlaybackManager.Shared.Seek();
		}

		public override void OnRewind()
		{
			PlaybackManager.Shared.Previous();
		}

		public override void OnSkipToPrevious()
		{
			PlaybackManager.Shared.PlayPrevious();
		}
	}



}

