using System;
using MusicPlayer.Models;
using MusicPlayer.Managers;
using System.Threading;
using System.IO;
using MusicPlayer.Data;
namespace MusicPlayer.Playback
{
	public class PlaybackData
	{
		public string SongId { get; set; }
		public SongPlaybackData SongPlaybackData { get; set; }
		public DownloadHelper DownloadHelper { get; set; }
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
		Stream fileStream;
		public Stream DataStream
		{
			get
			{
				if (!(DownloadHelper?.IsDisposed ?? true))
				{
					return DownloadHelper;
				}
				if (!SongPlaybackData.IsLocal)
				{
					SongPlaybackData = MusicManager.Shared.GetPlaybackData(SongPlaybackData.Song, SongPlaybackData.IsVideo).Result;
				}
				return SongPlaybackData.IsLocal ? (Stream)File.OpenRead(SongPlaybackData.Uri.LocalPath) : DownloadHelper;
			}
		}

	}
}
