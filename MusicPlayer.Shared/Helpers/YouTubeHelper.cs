using System;
using YoutubeExtractor;
using System.Collections.Generic;


using System.Linq;
using MusicPlayer.Data;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public static partial class YouTubeHelper
	{
		public static VideoInfo GetVideoInfo (IEnumerable<VideoInfo> videos, bool isVideo)
		{
			var setting = Settings.VideoStreamQuality;
			var sortedVideos = videos.Where (info => (info.VideoType == VideoType.Mobile || info.VideoType == VideoType.Mp4) && info.AudioType != AudioType.Unknown && !info.Is3D).ToList ();

			if (isVideo) {
				sortedVideos = sortedVideos.OrderByDescending (x => x.Resolution).ThenByDescending (info => info.AudioBitrate).ToList ();
			} else {
				sortedVideos = sortedVideos.OrderByDescending (info => info.AudioBitrate).ThenBy (x => x.Resolution).ToList ();
			}

			switch (setting) {
			case StreamQuality.High:
				return sortedVideos.First ();
			case StreamQuality.Medium:
				var count = (int)sortedVideos.Count / 2;
				return sortedVideos [count];
			default:
				return sortedVideos.Last ();

			}
		}

		public static bool IsYoutube(string url)
		{
			try{
				var videoUrl = url;
				var isYoutubeUrl = DownloadUrlResolver.TryNormalizeYoutubeUrl(videoUrl, out videoUrl);
				return isYoutubeUrl;
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}
			return false;

		}

	}
}
