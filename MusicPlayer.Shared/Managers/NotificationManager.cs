using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using SimpleTables;

namespace MusicPlayer.Managers
{
	internal class NotificationManager : ManagerBase<NotificationManager>
	{
		public event EventHandler<EventArgs<Song>> CurrentSongChanged;

		public void ProcCurrentSongChanged(Song song)
		{
			CurrentSongChanged?.InvokeOnMainThread(this, song);
		}


		public event EventHandler<EventArgs<PlaybackState>> PlaybackStateChanged;

		public void ProcPlaybackStateChanged(PlaybackState state)
		{
			PlaybackStateChanged?.InvokeOnMainThread(this, state);
		}


		public class SongDowloadEventArgs : EventArgs
		{
			public SongDowloadEventArgs(string songId, float percent)
			{
				SongId = songId;
				Percent = percent;
			}

			public string SongId { get; set; }
			public float Percent { get; set; }
		}

		public event EventHandler<SongDowloadEventArgs> SongDownloadPulsed;

		public void ProcSongDownloadPulsed(string songId, float percent)
		{
			SongDownloadPulsed?.InvokeOnMainThread(this, new SongDowloadEventArgs(songId, percent));
		}


		public event EventHandler ToggleMenu;

		public void ProcToggleMenu()
		{
			ToggleMenu?.InvokeOnMainThread(this);
		}

		public event EventHandler<EventArgs<TrackPosition>> CurrentTrackPositionChanged;

		public void ProcCurrentTrackPositionChanged(TrackPosition position)
		{
			CurrentTrackPositionChanged?.InvokeOnMainThread(this, position);
		}

		public event EventHandler<EventArgs<float[]>> UpdateVisualizer;

		public void ProcUpdateVisualizer(float[] values)
		{
			UpdateVisualizer?.InvokeOnMainThread(this, values);
		}

		public event EventHandler<EventArgs<bool>> ShuffleChanged;

		public void ProcShuffleChanged(bool value)
		{
			ShuffleChanged?.InvokeOnMainThread(this, value);
		}


		public event EventHandler<EventArgs<RepeatMode>> RepeatChanged;

		public void ProcRepeatChanged(RepeatMode value)
		{
			RepeatChanged?.InvokeOnMainThread(this, value);
		}

		public event EventHandler CurrentPlaylistChanged;

		public void ProcCurrentPlaylistChanged()
		{
			CurrentPlaylistChanged?.InvokeOnMainThread(this);
		}

		public event EventHandler SongDatabaseUpdated;

		public void ProcSongDatabaseUpdated()
		{
			SongDatabaseUpdated?.InvokeOnMainThread(this);
		}

		public event EventHandler RadioDatabaseUpdated;

		public void ProcRadioDatabaseUpdated()
		{
			RadioDatabaseUpdated?.InvokeOnMainThread(this);
		}

		public event EventHandler PlaylistsDatabaseUpdated;

		public void ProcPlaylistDatabaseUpdated()
		{
			PlaylistsDatabaseUpdated?.InvokeOnMainThread(this);
		}

		public event EventHandler EqualizerEnabledChanged;

		public void ProcEqualizerEnabledChanged()
		{
			EqualizerEnabledChanged?.InvokeOnMainThread(this);
		}

		public event EventHandler EqualizerChanged;

		public void ProcEqualizerChanged()
		{
			EqualizerChanged?.InvokeOnMainThread(this);
		}

		public event EventHandler<EventArgs<string>> GoToAlbum;

		public void ProcGoToAlbum(string albumId)
		{
			GoToAlbum?.InvokeOnMainThread(this, albumId);
		}

		public event EventHandler<EventArgs<string>> GoToArtist;

		public void ProcGoToArtist(string artistId)
		{
			GoToArtist?.InvokeOnMainThread(this, artistId);
		}

		public event EventHandler ToggleNowPlaying;

		public void ProcToggleNowPlaying()
		{
			ToggleNowPlaying?.InvokeOnMainThread(this);
		}

		public event EventHandler CloseNowPlaying;

		public void ProcCloseNowPlaying()
		{
			CloseNowPlaying?.InvokeOnMainThread(this);
		}


		public event EventHandler DownloaderStarted;
		public void ProcDownloaderStarted()
		{
			DownloaderStarted?.InvokeOnMainThread(this);
		}

		public event EventHandler<EventArgs<string>> FailedDownload;
		public void ProcFailedDownload (string songId)
		{
			FailedDownload?.InvokeOnMainThread (this, songId);
		}

		public event EventHandler<EventArgs<bool>> VideoPlaybackChanged;

		public void ProcVideoPlaybackChanged(bool isVideo)
		{
			VideoPlaybackChanged?.InvokeOnMainThread(this,isVideo);
		}

		public event EventHandler ToggleFullScreenVideo;

		public void ProcToggleFullScreenVideo()
		{
			ToggleFullScreenVideo?.InvokeOnMainThread(this);
        }
		
		public event EventHandler OfflineChanged;
		public void ProcOfflineChanged()
		{
			OfflineChanged?.InvokeOnMainThread(this);
			ProcRadioDatabaseUpdated();
			ProcSongDatabaseUpdated();
			ProcPlaylistDatabaseUpdated();
        }

		public event EventHandler<EventArgs<string>> FailedToDownloadTrack;
		public void ProcFailedToDownloadTrack(string trackId)
		{
			FailedToDownloadTrack?.InvokeOnMainThread(this,trackId);
		}

		public event EventHandler VolumeChanged;
		public void ProcVolumeChanged()
		{
			VolumeChanged?.InvokeOnMainThread(this);
		}

		public event EventHandler StyleChanged;
		public void ProcStyleChanged()
		{
			#if __IOS__
			iOS.Style.SetStyle();
			#endif
			StyleChanged?.InvokeOnMainThread(this);
		}
		public event EventHandler ConsoleChanged;
		public void ProcConsoleChanged()
		{
			ConsoleChanged?.InvokeOnMainThread(this);
		}
	}
}