using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using MusicPlayer.Managers;

namespace MusicPlayer.iOS
{
	public class Application
	{

		public static Xamarin.ITrackHandle AppStart;
		// This is the main entry point of the application.
		static void Main (string[] args)
		{

			Xamarin.Insights.Initialize (ApiConstants.InsightsApiKey);

			AppStart = Xamarin.Insights.TrackTime ("App Launch Time");
			UIApplication.CheckForIllegalCrossThreadCalls = false;
			System.Net.ServicePointManager.DefaultConnectionLimit = 50;
			NSString appClass = new NSString (@"MyUIApp");
			NSString delegateClass = new NSString (@"AppDelegate");
			UIApplication.Main (args, appClass, delegateClass);

		}
	}


	[Register ("MyUIApp")]
	public class MyUIApp : UIApplication
	{
		public override void SendEvent (UIEvent theEvent)
		{
			//Console.WriteLine(theEvent.Type);
			if (!Device.IsIos7_1 && theEvent.Type == UIEventType.RemoteControl) {

				//Console.WriteLine(theEvent.Subtype);
				switch (theEvent.Subtype) {

				case UIEventSubtype.RemoteControlPause:
					PlaybackManager.Shared.Pause ();
					break;
				case UIEventSubtype.RemoteControlPlay:
					PlaybackManager.Shared.Play ();
					break;
				case UIEventSubtype.RemoteControlTogglePlayPause:
					PlaybackManager.Shared.PlayPause ();
					break;
				case UIEventSubtype.RemoteControlPreviousTrack:
					PlaybackManager.Shared.Previous ();
					break;

				case UIEventSubtype.RemoteControlBeginSeekingForward:
				case UIEventSubtype.RemoteControlNextTrack:
					PlaybackManager.Shared.NextTrack ();
					break;

				default:
					break;
				}
			} else
				base.SendEvent (theEvent);
		}

		public override void RemoteControlReceived (UIEvent theEvent)
		{
			if (!Device.IsIos7_1 && theEvent.Type == UIEventType.RemoteControl) {

				Console.WriteLine (theEvent.Subtype);
				switch (theEvent.Subtype) {

				case UIEventSubtype.RemoteControlPause:
					PlaybackManager.Shared.Pause ();
					break;
				case UIEventSubtype.RemoteControlPlay:
					PlaybackManager.Shared.Play ();
					break;
				case UIEventSubtype.RemoteControlTogglePlayPause:
					PlaybackManager.Shared.PlayPause ();
					break;
				case UIEventSubtype.RemoteControlPreviousTrack:
					PlaybackManager.Shared.Previous ();
					break;

				case UIEventSubtype.RemoteControlBeginSeekingForward:
				case UIEventSubtype.RemoteControlNextTrack:
					PlaybackManager.Shared.NextTrack();
					break;

				default:
					break;
				}
			} else
				base.RemoteControlReceived (theEvent);
		}
	}
}
