using System;
using MusicPlayer.Models;
using MusicPlayer.Managers;
using System.Threading;
using System.IO;
namespace MusicPlayer.Playback
{
	public class PlaybackData
	{
		public string SongId { get; set; }
		public SongPlaybackData SongPlaybackData { get; set; }
		public DownloadHelper DownloadHelper { get; set; }
		public CancellationTokenSource CancelTokenSource { get; set; } = new CancellationTokenSource ();
		public string MimeType
		{
			get
			{
				if (DownloadHelper != null)
					return DownloadHelper.MimeType;
				//TODO: more robust MimeType
				return SongPlaybackData.CurrentTrack.MediaType == MediaType.Video ? "video/mpeg" : "audio/mpeg";
			}
		}
		public Stream DataStream => SongPlaybackData.IsLocal ? (Stream)File.OpenRead(SongPlaybackData.Uri.LocalPath) : DownloadHelper;
	}
}
