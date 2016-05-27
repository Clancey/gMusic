//using System;
//using MusicPlayer.Models;
//using SimpleTables;

//namespace MusicPlayer
//{
//	public partial class Playback
//	{
//		static void init()
//		{
//			//throw new NotImplementedException ();
////			AVAudioSession.Notifications.ObserveRouteChange((sender, args) =>
////				{
////					if(args.Reason == AVAudioSessionRouteChangeReason.OldDeviceUnavailable)
////						Pause();
////					Console.WriteLine("Route Changed");
////				});
//		}
//		static void SharedOnCurrentSongChanged(object sender, EventArgs<Song> eventArgs)
//		{

//			//throw new NotImplementedException ();
////			try
////			{
////				nowPlayingInfo = new MPNowPlayingInfo();
////				var song = eventArgs.Data;
////				nowPlayingInfo.Title = song.Title;
////				nowPlayingInfo.Artist = song.Artist;
////				nowPlayingInfo.AlbumTitle = song.Album ?? "NO TITLE";
////				nowPlayingInfo.AlbumTrackNumber = song.Track;
////				nowPlayingInfo.Composer = song.Composer;
////				nowPlayingInfo.DiscCount = song.TotalDiscs;
////				nowPlayingInfo.DiscNumber = song.Disc;
////				nowPlayingInfo.Genre = song.Genre;
////				nowPlayingInfo.Artwork = artowrk;
////				nowPlayingInfo.PlaybackDuration = song.DurationInSeconds();
////				App.InvokeOnMainThread(()=>MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo);
////			}
////			catch (Exception ex)
////			{
////				Console.WriteLine(ex);
////			}
//		}

//		public static void UpdateTime()
//		{
//			//throw new NotImplementedException ();
////			try
////			{
////				if (nowPlayingInfo == null)
////					return;
////				var currentTime = AudioPlayer.Shared.CurrentTime;
////				if (Math.Abs(currentTime - lastTime) < 1)
////					return;
////				lastTime = currentTime;
////				if(artowrk != null && (int)lastTime %10 == 0)
////					nowPlayingInfo.Artwork = artowrk;
////				nowPlayingInfo.ElapsedPlaybackTime = currentTime;
////				if(AudioPlayer.Shared.Progress > .5 || currentTime > 30000)
////					scrobble(AudioPlayer.Shared.CurrentSong);
////				UpdateNowPlaying();
////			}
////			catch (Exception ex)
////			{
////				Console.WriteLine(ex);
////			}
//		}


//	}
//}

