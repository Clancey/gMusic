using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using MediaPlayer;
using MusicPlayer.iOS;
using MusicPlayer.Managers;
using UIKit;
using MusicPlayer.Data;

namespace MusicPlayer.Playback
{
	internal static class RemoteControlHandler
	{
		public static void Init ()
		{
#if __IOS__
			if (Device.IsIos7_1)
#endif
			{
				InitCommandCenter();
			}
		}
		static NSObject thumbsUpObject;
		static NSObject thumbsDownObject;
		static void InitCommandCenter()
		{
			var center = MPRemoteCommandCenter.Shared;
			center.BookmarkCommand.Enabled = false;
			//center.LikeCommand.Enabled = true;
			center.NextTrackCommand.Enabled = true;
			//center.RatingCommand.Enabled = true;
			center.SeekBackwardCommand.Enabled = true;
			center.SeekForwardCommand.Enabled = true;
			center.PreviousTrackCommand.Enabled = true;
			center.PlayCommand.Enabled = true;
			center.TogglePlayPauseCommand.Enabled = true;
			center.PauseCommand.Enabled = true;
			center.ChangePlaybackRateCommand.Enabled = true;
			center.ChangePlaybackPositionCommand.Enabled = true;
			//center.DislikeCommand.Enabled = true;
			center.SkipBackwardCommand.Enabled = true;
			center.SkipForwardCommand.Enabled = true;
			center.StopCommand.Enabled = true;

			SetupThumbsUp ();

			center.PlayCommand.AddTarget((evt) =>
			{
				PlaybackManager.Shared.Play();
				return MPRemoteCommandHandlerStatus.Success;
			});

			center.ChangePlaybackPositionCommand.AddTarget ((evt) => {
				var cmd = evt as MPChangePlaybackPositionCommandEvent;
				Console.WriteLine (cmd.PositionTime);
				PlaybackManager.Shared.NativePlayer.SeekTime (cmd.PositionTime);
				return MPRemoteCommandHandlerStatus.Success;
			});
			center.ChangePlaybackRateCommand.AddTarget((evt) =>
			{
				var change = evt.Command as MPChangePlaybackRateCommand;

				return MPRemoteCommandHandlerStatus.CommandFailed;
			});
			center.PauseCommand.AddTarget((evt) =>
			{
				PlaybackManager.Shared.Pause();
				return MPRemoteCommandHandlerStatus.Success;
			});

			center.TogglePlayPauseCommand.AddTarget((evt) =>
			{
				PlaybackManager.Shared.PlayPause();
				return MPRemoteCommandHandlerStatus.Success;
			});

			center.NextTrackCommand.AddTarget((evt) =>
			{
				PlaybackManager.Shared.NextTrack();
				return MPRemoteCommandHandlerStatus.Success;
			});
			center.PreviousTrackCommand.AddTarget((evt) =>
			{
				PlaybackManager.Shared.Previous();
				return MPRemoteCommandHandlerStatus.Success;
			});
			//center.RatingCommand.AddTarget((evt) =>
			//{
			//	evt.
			//	return MPRemoteCommandHandlerStatus.Success;
			//});
			center.SeekBackwardCommand.AddTarget((evt) =>
			{
				return MPRemoteCommandHandlerStatus.Success;
			});
			center.SeekForwardCommand.AddTarget((evt) =>
			{
				return MPRemoteCommandHandlerStatus.Success;
			});
			//center.SkipBackwardCommand.AddTarget((evt) =>
			//{
			//	return MPRemoteCommandHandlerStatus.Success;
			//});
			center.StopCommand.AddTarget((evt) =>
			{
				PlaybackManager.Shared.Pause();
				return MPRemoteCommandHandlerStatus.Success;
			});
		}

		public static void SetupThumbsUp()
		{
			var center = MPRemoteCommandCenter.Shared;
			if (Settings.ThubsUpOnLockScreen ) {
				thumbsDownObject = center.DislikeCommand.AddTarget ((evt) => {
					MusicManager.Shared.ThumbsDown (MusicManager.Shared.GetCurrentSong ());
					return MPRemoteCommandHandlerStatus.Success;
				});

				thumbsUpObject = center.LikeCommand.AddTarget((evt) =>
					{
						MusicManager.Shared.ThumbsUp(MusicManager.Shared.GetCurrentSong());
						return MPRemoteCommandHandlerStatus.Success;
					});
				return;
			}

			if (thumbsDownObject == null)
				return;
			center.DislikeCommand.RemoveTarget (thumbsDownObject);
			thumbsDownObject = null;
			center.LikeCommand.RemoveTarget (thumbsUpObject);
			thumbsUpObject = null;
		}
	}
}