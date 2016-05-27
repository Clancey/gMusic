using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Util;
using Android.Views;
using Android.Widget;
using MusicPlayer.Droid.Services;
using Fragment = Android.App.Fragment;

namespace MusicPlayer.Droid.UI
{
	public class PlaybackControlsFragment : Fragment, IMediaControllerCallBack
	{
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		ImageButton playPauseButton;
		TextView title;
		TextView subTitle;
		TextView extraInfo;
		ImageView albumArt;
		string currentArtUrl;
		MediaControllerCallBack callback;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = inflater.Inflate(Resource.Layout.fragment_playback_controls, container, false);
			playPauseButton = view.FindViewById<ImageButton>(Resource.Id.play_pause);
			playPauseButton.Enabled = true;
			playPauseButton.Click += PlayPauseButton_Click;

			title = view.FindViewById<TextView>(Resource.Id.title);
			subTitle = view.FindViewById<TextView>(Resource.Id.artist);
			extraInfo = view.FindViewById<TextView>(Resource.Id.extra_info);
			albumArt = view.FindViewById<ImageView>(Resource.Id.album_art);

			view.Click += View_Click;

			return view;
		}

		private void PlayPauseButton_Click(object sender, EventArgs e)
		{
			var controller = ((FragmentActivity)Activity)?.SupportMediaController;
			var state = (controller?.PlaybackState?.State ?? PlaybackStateCompat.StateNone);
			switch (state)
			{
				case PlaybackStateCompat.StatePaused:
				case PlaybackStateCompat.StateStopped:
				case PlaybackStateCompat.StateNone:
					PlayMedia();
					return;
				case PlaybackStateCompat.StatePlaying:
				case PlaybackStateCompat.StateBuffering:
				case PlaybackStateCompat.StateConnecting:
					PauseMedia();
					return;
			}
		}

		void PlayMedia()
		{
			((FragmentActivity) Activity)?.SupportMediaController?.GetTransportControls()?.Play();
		}

		void PauseMedia()
		{
			((FragmentActivity)Activity)?.SupportMediaController?.GetTransportControls()?.Pause();
		}
		private void View_Click(object sender, EventArgs e)
		{
			var intent = new Intent(Activity,typeof	(FullScreenPlayerActivity)).SetFlags(ActivityFlags.SingleTop);
			var controller = ((FragmentActivity) Activity)?.SupportMediaController;
			var metaData = controller?.Metadata;
			if (metaData != null)
				intent.PutExtra(MusicPlayerActivity.CurrentMediaDescription, metaData);
			StartActivity(intent);
		}

		public override void OnStart()
		{
			base.OnStart();
			OnConnected();
		}

		public override void OnStop()
		{
			base.OnStop();
			if(callback != null)
				((FragmentActivity)Activity)?.SupportMediaController?.UnregisterCallback(callback);
		}

		public void OnConnected()
		{
			var controller = ((FragmentActivity)Activity)?.SupportMediaController;
			if (controller == null)
				return;
			OnMetadataChanged(controller.Metadata);
			OnPlaybackStateChanged(controller.PlaybackState);
			controller.RegisterCallback(callback = new MediaControllerCallBack {Parent = this});

		}

		public void OnPlaybackStateChanged(PlaybackStateCompat state)
		{
			if (Activity == null || state == null)
				return;
			var enablePlay = false;
			switch (state.State)
			{
				case PlaybackStateCompat.StatePaused:
				case PlaybackStateCompat.StateStopped:
					enablePlay = true;
					break;
				case PlaybackStateCompat.StateError:
					Toast.MakeText(Activity,state.ErrorMessageFormatted,ToastLength.Long).Show();
					break;
			}
			playPauseButton.SetImageDrawable(ContextCompat.GetDrawable(Activity,enablePlay ? Resource.Drawable.ic_play_arrow_black_36dp : Resource.Drawable.ic_pause_black_36dp));
			setExtraInfo();
		}

		void setExtraInfo()
		{
			var controller = ((FragmentActivity)Activity)?.SupportMediaController;
			var castName = controller?.Extras?.GetString(MusicService.ExtraConnectedCast);
			var extra = string.IsNullOrWhiteSpace(castName) ? "" : $"Casting to {castName}";
			extraInfo.Text = extra;
			extraInfo.Visibility = string.IsNullOrWhiteSpace(extra) ? ViewStates.Gone : ViewStates.Visible;
		}

		public void OnMetadataChanged(MediaMetadataCompat metadata)
		{
			if (Activity == null || metadata == null)
				return;
			title.Text = metadata?.Description?.Title ?? "";
			subTitle.Text = metadata.Description?.Subtitle ?? "";

			var artUrl = metadata?.Description?.IconUri?.ToString();
			if (artUrl != currentArtUrl)
			{
				currentArtUrl = artUrl;
				var art = metadata?.Description?.IconBitmap;
				if(art != null)
					albumArt.SetImageBitmap(art);
				//TODO: Fetch artwork
			}


		}

		public void OnSessionDestroyed()
		{

		}
	}
}