using System;
using MusicPlayer.Playback;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using MusicPlayer.Data;
namespace MusicPlayer.Playback
{	
	partial class NativeAudioPlayer
	{
		public readonly Dictionary<string, PlaybackData> CurrentData = new Dictionary<string, PlaybackData> ();
		public readonly Dictionary<string, string> SongIdTracks = new Dictionary<string, string> ();

		internal async Task<PlaybackData> GetPlaybackDataForWebServer(string id)
		{
			var data = GetPlaybackData(id);
			if (data != null && data.DownloadHelper != null)
				return data;
			var video = id == currentSong?.Id ? isVideo : false;
			var song = Database.Main.GetObject<Song, TempSong>(id);
			var result = await PrepareSong(song, video);
			return result.Item2;

				
		}

		internal PlaybackData GetPlaybackData (string id, bool create = true)
		{
			lock (CurrentData) {
				PlaybackData data;
				if (!CurrentData.TryGetValue (id, out data) && create)
					CurrentData [id] = data = new PlaybackData {
						SongId = id,
					};
				return data;
			}
		}


		Dictionary<Tuple<string, bool>, Task<Tuple<bool, PlaybackData>>> prepareTasks = new Dictionary<Tuple<string, bool>, Task<Tuple<bool, PlaybackData>>> ();
		public async Task<Tuple<bool, PlaybackData>> PrepareSong (Song song, bool playVideo = false)
		{
			var tuple = new Tuple<string, bool> (song.Id, playVideo);
			Task<Tuple<bool, PlaybackData>> task;
			lock (prepareTasks) {
				if (!prepareTasks.TryGetValue (tuple, out task) || task.IsCompleted)
					prepareTasks [tuple] = task = prepareSong (song, playVideo);
			}
			var result = await task;
			lock (prepareTasks) {
				prepareTasks.Remove (tuple);
			}
			return result;
		}

		double lastSeconds;
		async Task<Tuple<bool, PlaybackData>> prepareSong (Song song, bool playVideo = false)
		{
			try {
				isVideo = playVideo;
				LogManager.Shared.Log ("Preparing Song", song);
				var data = GetPlaybackData (song.Id);
				var playbackData = data.SongPlaybackData = await MusicManager.Shared.GetPlaybackData (song, playVideo);
				if (playbackData == null)
					return new Tuple<bool, PlaybackData> (false, data);
				if (data.CancelTokenSource.IsCancellationRequested)
					return new Tuple<bool, PlaybackData> (false, data);

				if (song == CurrentSong) {
					Settings.CurrentTrackId = playbackData.CurrentTrack.Id;
					isVideo = playbackData.CurrentTrack.MediaType == MediaType.Video;
					Settings.CurrentPlaybackIsVideo = isVideo;
					NotificationManager.Shared.ProcVideoPlaybackChanged (isVideo);
				}


				if (playbackData.IsLocal || playbackData.CurrentTrack.ServiceType == MusicPlayer.Api.ServiceType.iPod) {
					NotificationManager.Shared.ProcSongDownloadPulsed (song.Id, 1f);
				} else {
					NotificationManager.Shared.ProcSongDownloadPulsed(song.Id, 0f);
					data.SongPlaybackData = playbackData;
					data.DownloadHelper = await DownloadManager.Shared.DownloadNow (playbackData.CurrentTrack.Id, playbackData.Uri);
					if (data.CancelTokenSource.IsCancellationRequested)
						return new Tuple<bool, PlaybackData> (false, data);
					LogManager.Shared.Log ("Loading online Track", data.SongPlaybackData.CurrentTrack);
					SongIdTracks [data.SongPlaybackData.CurrentTrack.Id] = song.Id;
				}

				lastSeconds = -1;
				var success = !data.CancelTokenSource.IsCancellationRequested;

				LogManager.Shared.Log("Finished Preparing Song", song);
				return new Tuple<bool, PlaybackData> (true, data);
			} catch (Exception ex) {
				LogManager.Shared.Report (ex);
				return new Tuple<bool, PlaybackData> (false, null);
			}
		}
	}
}
