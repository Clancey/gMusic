using System;
using Android.App;
using Android.Content;
using Android.Views;
using MusicPlayer.Managers;

namespace MusicPlayer
{ 
	[BroadcastReceiver (Label = "Remote Control Receiver")]
	[IntentFilter (new[] { Intent.ActionMediaButton })]
	public class RemoteControlReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{

			KeyEvent keyEvent = (KeyEvent) intent.Extras.Get(Intent.ExtraKeyEvent);
			if (keyEvent.Action != KeyEventActions.Down)
				return;

			switch (keyEvent.KeyCode) {
			case Keycode.Headsethook:
			case Keycode.MediaPlayPause:
				PlaybackManager.Shared.PlayPause ();
				break;
			case Keycode.MediaPlay:
				PlaybackManager.Shared.Pause ();
				break;
			case Keycode.MediaPause:
				PlaybackManager.Shared.Pause ();
				break;
			case Keycode.MediaStop:
				PlaybackManager.Shared.Pause ();
				break;
			case Keycode.MediaNext:
				PlaybackManager.Shared.NextTrack ();
				break;
			case Keycode.MediaPrevious:
				PlaybackManager.Shared.Previous ();
				break;
			}
		}
	}
}

