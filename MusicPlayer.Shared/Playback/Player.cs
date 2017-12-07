using System;
using System.Threading.Tasks;
using System.IO;
using MusicPlayer.Models;
namespace MusicPlayer.Playback
{
	public abstract class Player : IDisposable
	{
		public Action<PlaybackState> StateChanged { get; set; }

		PlaybackState state;
		public virtual PlaybackState State {
			get => state;
			set {
				if (value == state)
					return;
				state = value;
				StateChanged?.Invoke (state);
			}
		}

		public Action<Player> Finished { get; set; }

		public Action<double> PlabackTimeChanged { get; set; }

		public abstract float Volume { get; set; }

		public virtual string CurrentSongId { get; set; }

		public abstract Task<bool> PlaySong (Song song, bool isVideo, bool forcePlay = false);

		public abstract Task<bool> PrepareData (PlaybackData playbackData, bool isPlaying);

		public abstract bool Play ();

		public abstract void Pause ();

		public abstract void Stop();

		public abstract void Seek (double time);

		public abstract float Rate { get; }

		public virtual float [] AudioLevels { get; set; }

		public abstract double CurrentTimeSeconds ();

		public abstract double Duration ();

		public float Progress => (float)CurrentTimeSeconds().SafeDivideByZero(Duration());

		public abstract void Dispose ();

		public abstract void ApplyEqualizer (Equalizer.Band [] bands);

		public abstract void ApplyEqualizer ();

		public abstract void UpdateBand (int band, float gain);

		public abstract bool IsPlayerItemValid { get; }

		public virtual bool IsPrepared { get; set; }

		public virtual void DisableVideo ()
		{
		}
		public virtual void EnableVideo ()
		{
		}
	}
}
