using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayer.Models
{
	internal class TrackPosition
	{
		public double CurrentTime { get; set; }

		public double Duration { get; set; }
		public double RemaingTime => Duration - CurrentTime;
		public string CurrentTimeString => Format(TimeSpan.FromSeconds(CurrentTime));
		public string RemainingTimeString => Format(TimeSpan.FromSeconds(RemaingTime));
		public float Percent => (float) (Duration == 0 ? 0 : CurrentTime/Duration);

		string Format(TimeSpan timeSpan)
		{
			return timeSpan.Hours > 0 ? $"{timeSpan:h\\:mm\\:ss}" : $"{timeSpan:mm\\:ss}";
		}
	}
}