using System;
using AVFoundation;
using System.Threading.Tasks;
using MusicPlayer.Managers;

namespace MusicPlayer.iOS.Playback
{
	public static class AVPlayerExtension
	{
		public static void Seek(this AVPlayer player, double seconds)
		{
			player.Seek(CoreMedia.CMTime.FromSeconds(seconds, 1));
		}
		public static double Seconds(this AVPlayer player, AVPlayerItem item)
		{
			if (player?.CurrentItem != item)
				return 0;
			var seconds = player?.CurrentItem?.Duration.Seconds ?? 0;
			return double.IsNaN(seconds) ? 0 : seconds;
		}
		public static double Seconds(this AVPlayer player)
		{
			var seconds = player?.CurrentItem?.Duration.Seconds ?? 0;
			return double.IsNaN(seconds) ? 0 : seconds;
		}

		public static double CurrentTimeSeconds(this AVPlayer player, AVPlayerItem item)
		{
			if(player?.CurrentItem != item)
				return 0;
			if (player?.CurrentTime == null)
				return 0;
			var seconds = player.CurrentTime.IsInvalid ? 0 : player.CurrentTime.Seconds;
			return double.IsNaN(seconds) ? 0 : seconds;

		}
        public static double CurrentTimeSeconds(this AVPlayer player)
		{
			if (player?.CurrentTime == null)
				return 0;
			var seconds = player.CurrentTime.IsInvalid ? 0 : player.CurrentTime.Seconds;
			return double.IsNaN(seconds) ? 0 : seconds;
		}

		public static async Task WaitLoadTracks(this AVPlayerItem item)
		{
			try
			{
				if (item.Tracks.Length > 0)
					return;
				var tcs = new TaskCompletionSource<bool>();
#pragma warning disable 4014
				Task.Run(async () =>
				{
#pragma warning restore 4014
					int count = 0;
					while (count <= 10)
					{
						await Task.Delay(100);
						if (item.Tracks.Length > 0)
						{
							tcs.TrySetResult(true);
							break;
						}
						count ++;
					}
					tcs.TrySetResult(false);
				});

				await tcs.Task;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		public static Task WaitStatus(this AVPlayerItem item)
		{
			return  item.Asset.LoadValuesTaskAsync(new[]{ "duration" ,"tracks"});
			
		}
	}
}