//using System;
//using Android.Media;
//using System.IO;
//using Android.Views;
//using MusicPlayer.Models;

//namespace MusicPlayer
//{
//	public class MoviePlayer
//	{
//		MusicPlayer player;
//		public MoviePlayer ()
//		{
//			player = MusicPlayer.Create (App.Context, Android.Net.Uri.Empty);
////			player.Prepared += (object sender, EventArgs e) => {
////				prepared();
////			};
//			player.BufferingUpdate += (object sender, MusicPlayer.BufferingUpdateEventArgs e) => {

//			};
//			player.Completion += (object sender, EventArgs e) => {
//				AudioPlayer.Shared.CurrentState = AudioPlayer.State.Stoped;
//				Playback.SongIsOver(); // ();
//			};
//			player.Error += (object sender, MusicPlayer.ErrorEventArgs e) => {
//				Console.Error.WriteLine(e.What.ToString());
//				//AudioPlayer.Shared.CurrentState = AudioPlayer.State.Stoped;
//				//Playback.SongIsOver(); // ();
//			};
//		}

//		public void Seek(double seconds)
//		{
//			player.SeekTo ((int)seconds * 1000);
//		}
//		public int SessionId
//		{
//			get{ return player.AudioSessionId; }
//		}
//		public PlayerItem CurrentItem {get;set;}

//		public class PlayerItem
//		{

//		}
//		public void Pause()
//		{
//			player.Pause ();
//		}
//		public void Play()
//		{
//			player.Start ();
//		}
//		public double Seconds()
//		{
//			return player.Duration;
//		}

//		public double CurrentTimeSeconds()
//		{
//			return player.CurrentPosition;
//		}

//		public double Rate 
//		{
//			get{ return player.IsPlaying ? 1 : 0; }
//		}

//		public void PlayMovie(Song song)
//		{
//			try{
//				var exists = File.Exists(song.File);
//				player.Reset();
//				player.SetDataSource (App.Context,Android.Net.Uri.FromFile(new Java.IO.File( song.File)));
//				player.Prepare ();
//				player.Start ();
//				if (App.SurfaceHolder != null)
//					player.SetDisplay (App.SurfaceHolder);
//			}
//			catch(Exception ex) {
//				Logger.Log (ex);
//			}
//		}

//		public void SetSurface(ISurfaceHolder surface)
//		{
//			player.SetDisplay (surface);
//		}

//		void prepared()
//		{
//			if (App.SurfaceHolder != null)
//				player.SetDisplay (App.SurfaceHolder);
//			player.Start ();
//		}
//	}
//}

