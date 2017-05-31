using System;
using MusicPlayer.Models;
using MusicPlayer.Managers;
using System.Threading;
namespace MusicPlayer.Playback
{
	public class PlaybackData
	{
		public string SongId { get; set; }
		public SongPlaybackData SongPlaybackData { get; set; }
		public DownloadHelper DownloadHelper { get; set; }
		public CancellationTokenSource CancelTokenSource { get; set; } = new CancellationTokenSource ();
	}
}
