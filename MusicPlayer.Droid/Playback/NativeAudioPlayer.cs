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

namespace MusicPlayer
{
	public class NativeAudioPlayer : BaseModel
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
		public async Task<Tuple<bool, string>> prepareSong(Song song, bool playVideo = false)
		{
			try
			{
				isVideo = playVideo;
				LogManager.Shared.Log("Preparing Song", song);
				var data = GetPlaybackData(song.Id);
				var playbackData = await MusicManager.Shared.GetPlaybackData(song, playVideo);
				if (playbackData == null)
					return new Tuple<bool, string>(false, null);
				if (data.CancelTokenSource.IsCancellationRequested)
					return new Tuple<bool, string>(false, null);

				var playerItem = "";

				if (song == CurrentSong)
				{
					Settings.CurrentTrackId = playbackData.CurrentTrack.Id;
					isVideo = playbackData.CurrentTrack.MediaType == MediaType.Video;
					Settings.CurrentPlaybackIsVideo = isVideo;
					NotificationManager.Shared.ProcVideoPlaybackChanged(isVideo);
				}
				if (playbackData.IsLocal || playbackData.CurrentTrack.ServiceType == MusicPlayer.Api.ServiceType.iPod)
				{
					if (playbackData.Uri == null)
						return new Tuple<bool, string>(false, null);
					LogManager.Shared.Log("Local track found", song);
					var url = string.IsNullOrWhiteSpace(playbackData?.CurrentTrack?.FileLocation) ? playbackData.Uri.AbsoluteUri : playbackData.CurrentTrack.FileLocation;
					playerItem = url;
					NotificationManager.Shared.ProcSongDownloadPulsed(song.Id, 1f);
				}
				else
				{
					data.SongPlaybackData = playbackData;
					data.DownloadHelper = await DownloadManager.Shared.DownloadNow(playbackData.CurrentTrack.Id, playbackData.Uri);
					if (data.CancelTokenSource.IsCancellationRequested)
						return new Tuple<bool, string>(false, null);
					LogManager.Shared.Log("Loading online Track", data.SongPlaybackData.CurrentTrack);
					//SongIdTracks[data.SongPlaybackData.CurrentTrack.Id] = song.Id;
					//NSUrlComponents comp =
					//	new NSUrlComponents(
					//		NSUrl.FromString(
					//			$"http://localhost/{playbackData.CurrentTrack.Id}.{data.SongPlaybackData.CurrentTrack.FileExtension}"), false);
					//comp.Scheme = "streaming";
					//if (comp.Url != null)
					//{
					//	var asset = new AVUrlAsset(comp.Url, new NSDictionary());
					//	asset.ResourceLoader.SetDelegate(LoaderDelegate, DispatchQueue.MainQueue);
					//	playerItem = new AVPlayerItem(asset);
					//}
					//if (data.CancelTokenSource.IsCancellationRequested)
					//	return new Tuple<bool, AVPlayerItem>(false, null);

					//await playerItem.WaitStatus();
				}
				//lastSeconds = -1;
				var success = !data.CancelTokenSource.IsCancellationRequested;

				return new Tuple<bool, string>(success, playbackData.Uri.AbsoluteUri);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				return new Tuple<bool, string>(false, null);
			}
		}


		public readonly Dictionary<string, PlaybackData> CurrentData = new Dictionary<string, PlaybackData>();
		public readonly Dictionary<string, string> SongIdTracks = new Dictionary<string, string>();
		public class PlaybackData
		{
			public string SongId { get; set; }
			public SongPlaybackData SongPlaybackData { get; set; }
			public DownloadHelper DownloadHelper { get; set; }
			public CancellationTokenSource CancelTokenSource { get; set; } = new CancellationTokenSource();
		}
		internal PlaybackData GetPlaybackData(string id, bool create = true)
		{
			lock (CurrentData)
			{
				PlaybackData data;
				if (!CurrentData.TryGetValue(id, out data) && create)
					CurrentData[id] = data = new PlaybackData
					{
						SongId = id,
					};
				return data;
			}
		}
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

	}
}

