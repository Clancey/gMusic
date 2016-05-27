using System;
using System.Threading.Tasks;

namespace MusicPlayer.Playback
{
	public interface IEqualizer
	{
		Task ApplyEqualizer(Equalizer.Band[] bands);
		void UpdateBand(int band, float gain);
		bool Active { get; set; }
		void Clear();
	}
}