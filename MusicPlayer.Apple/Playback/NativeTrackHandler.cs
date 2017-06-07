using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using MediaPlayer;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using MusicPlayer.Models;

namespace MusicPlayer.Playback
{
	internal partial class NativeTrackHandler : ManagerBase<NativeTrackHandler>
	{
		public NativeTrackHandler()
		{
			NotificationManager.Shared.CurrentSongChanged += (sender, args) => UpdateSong(args.Data);
			NotificationManager.Shared.CurrentTrackPositionChanged += (sender, args) => UpdateProgress(args.Data);
		}

		public void Init()
		{
			if (string.IsNullOrWhiteSpace(Settings.CurrentSong))
				return;
			UpdateSong(Database.Main.GetObject<Song, TempSong>(Settings.CurrentSong));
			OnInit ();
		}


		MPNowPlayingInfo nowPlayingInfo;
		MPMediaItemArtwork artwork;

		public void UpdateSong(Song song)
		{
			if (song == null)
				return;
			try
			{
				nowPlayingInfo = new MPNowPlayingInfo
				{
					Title = song?.Name ?? "",
					Artist = song?.Artist ?? "",
					AlbumTitle = song?.Album ?? "",
					Genre = song?.Genre ?? "",
					Artwork = CreateDefaultArtwork(),
				};
				SetAdditionInfo (song, nowPlayingInfo);

				artwork = null;
				FetchArtwork(song);
				OnSongChanged (song);
				App.RunOnMainThread(() => MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		MPMediaItemArtwork defaultResizableArtwork;
		MPMediaItemArtwork DefaultResizableArtwork {
			get => defaultResizableArtwork ?? (defaultResizableArtwork = new MPMediaItemArtwork (new CoreGraphics.CGSize (9999, 9999), (CoreGraphics.CGSize arg) => {
				return Images.GetDefaultAlbumArt (NMath.Max (arg.Height, arg.Width));
			}));
		}


		double lastTime = -1;

		public void UpdateProgress(TrackPosition position)
		{
			try
			{
				if (nowPlayingInfo == null)
					return;
				if (Math.Abs(position.CurrentTime - lastTime) < 1)
					return;
				lastTime = position.CurrentTime;
				if (artwork != null && (int) lastTime%10 == 0)
					nowPlayingInfo.Artwork = artwork;
				nowPlayingInfo.ElapsedPlaybackTime = position.CurrentTime;
				nowPlayingInfo.PlaybackDuration = position.Duration;
				App.RunOnMainThread(() => MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}