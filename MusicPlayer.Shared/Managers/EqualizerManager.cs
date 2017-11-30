using System;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using System.Linq;
using MusicPlayer.Data;
using MusicPlayer.Playback;

namespace MusicPlayer
{
	public class EqualizerManager : ManagerBase<EqualizerManager>
	{
		public EqualizerManager ()
		{
		}

		public event Action EqualizerReloaded;

		public void ReloadPresets()
		{
			Equalizer.Shared.Presets.Clear();
			Equalizer.Shared.LoadPresets();
			EqualizerReloaded?.InvokeOnMainThread ();
		}
		public void SetGain(int tag, float gain)
		{
			try
			{
				if (Settings.EqualizerEnabled)
				{
					Equalizer.Shared.Bands[tag].Gain = gain;
					Equalizer.Shared.UpdateBand(tag, gain, true);
				}
				Equalizer.Shared.CurrentPreset.Values[tag].Value = gain;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		public void SaveCurrent()
		{
			var currentPreset = GetCurrent();
			if (currentPreset.Id > 0)
				currentPreset.Save();
		}

		public void AddPreset(string name)
		{
			var preset = new EqualizerPreset()
			{
				Name = name,
				DoubleValues = new double[10]
				{
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
				}

			};
			preset.Save();
			ReloadPresets();
		}
		public void Reset(EqualizerPreset preset)
		{
			var match = Equalizer.DefaultPresets.FirstOrDefault(x => x.GlobalId == preset.GlobalId) ?? new EqualizerPreset()
			{
				DoubleValues = new double[10]
				{
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
				}

			};
			for (var i = 0; i < preset.Values.Length; i++)
			{
				preset.Values[i].Value = match.Values[i].Value;
			}
			preset.Save();
			ReloadPresets();
		}
		public void Delete(EqualizerPreset preset)
		{
			preset.Delete();
			ReloadPresets();
		}

		public EqualizerPreset GetCurrent()
		{
			return Equalizer.Shared.CurrentPreset ?? new EqualizerPreset()
			{
				Name = "",
				DoubleValues = new double[10]
				{
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
				}
			};
		}
	}
}

