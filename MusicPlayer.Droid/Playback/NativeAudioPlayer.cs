using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using MusicPlayer.Data;
using MusicPlayer.Droid.Playback;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using NotificationManager = MusicPlayer.Managers.NotificationManager;
using DownloadManager = MusicPlayer.Managers.DownloadManager;
using System.Threading;
using MusicPlayer.Droid.Services;
using MusicPlayer.Playback;

namespace MusicPlayer.Playback
{
	public partial class NativeAudioPlayer : BaseModel
	{
		public static MusicService Context { get; set; }
		public PlaybackState State { get; set; }

		MediaPlayerPlayer player;

		public NativeAudioPlayer()
		{
			player = new MediaPlayerPlayer(Context)
			{
				Parent = this,
			};
		}

		internal static void NativeInit(MusicService applicationContext)
		{
			Context = applicationContext;
		}


		public Song CurrentSong { get; internal set; }
		public double CurrentTime { get; internal set; }
		public double Duration { get; internal set; }
		public float Progress { get; internal set; }
		public float[] AudioLevels { get; internal set; }

		internal void Pause()
		{
			player.Pause();
		}


		public async Task  Play()
		{
			if (CurrentSong != null)
			{
				await player.Play(CurrentSong);
				return;
			}
		}
		internal async Task<bool> PlaySong(string id)
		{
			var song = Database.Main.GetObject<Song>(id);
			return await player.Play(song);
			//return await player.Play(song);

		}
		internal async Task<bool> PlaySong(Song song)
		{
			CurrentSong = song;
			Pause();
			App.Context.SupportMediaController.GetTransportControls().PlayFromMediaId(song.Id, null);
			return await Task.FromResult(true);
			//return await player.Play(song);

		}
		public void UpdateBand (int band, float gain)
		{

		}

		public void ApplyEqualizer (Equalizer.Band [] bands)
		{

		}

		public void ApplyEqualizer ()
		{

		}

		internal void QueueTrack(Track currentTrack)
		{
			//throw new NotImplementedException();
		}

		internal void Seek(float v)
		{
			//throw new NotImplementedException();
		}

		internal void PrepareFirstTrack(Song song, bool currentPlaybackIsVideo)
		{
			//throw new NotImplementedException();
		}

		internal async Task<bool> PlaySong(Song song, bool playVideo)
		{
			CurrentSong = song;
			Pause();
			App.Context.SupportMediaController.GetTransportControls().PlayFromMediaId(song.Id, null);
			return await player.Play(song);
		}


		bool isVideo;

		public static bool VerifyVideo(string file)
		{
			return VerifyIsMovie(file);
		}

		public static bool VerifyIsMovie(string file)
		{
			try
			{
				Android.Media.MediaExtractor extractor = new Android.Media.MediaExtractor();
				extractor.SetDataSource(file);
				var trackCount = extractor.TrackCount;
				for (int i = 0; i < trackCount; i++)
				{
					var trackFormat = extractor.GetTrackFormat(i);
					var mime = trackFormat.GetString(MediaFormat.KeyMime);
					if (mime.Contains("video"))
						return true;
					Console.WriteLine(mime);
				}
				return false;
				//				using(MediaMetadataRetriever reader = new MediaMetadataRetriever()){
				//
				//					reader.SetDataSource (file);
				//					var frame = reader.GetFrameAtTime(300);
				//					var hasVideo = reader.ExtractMetadata (MetadataKey.HasVideo);
				//					var hasAudio = reader.ExtractMetadata (MetadataKey.HasAudio);
				//					var frameSize = reader.ExtractMetadata(MetadataKey.VideoHeight);
				//					var bitrate = reader.ExtractMetadata(MetadataKey.Bitrate);
				//					return !string.IsNullOrEmpty(hasVideo);
				////				using (var player = MusicPlayer.Create (App.Context,Android.Net.Uri.FromFile(new Java.IO.File(file)))) {
				////					player.si
				////					var size = Math.Max (player.VideoHeight, player.VideoWidth);
				////					if (size > 0)
				////						return true;
				//				}
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		internal void CleanupSong (Song currentSong)
		{
			throw new NotImplementedException ();
		}
	}
}

