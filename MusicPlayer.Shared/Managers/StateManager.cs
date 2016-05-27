using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Data;
using MusicPlayer.Models;

namespace MusicPlayer.Managers
{
	internal class StateManager : ManagerBase<StateManager>
	{
		public int GlobalEqualizerPreset {
			get {
				return Settings.EqualizerPreset;
			}
			set {
				Settings.EqualizerPreset = value;
			}
		}

		public bool EqualizerEnabled {
			get { return Settings.EqualizerEnabled; }
			set { Settings.EqualizerEnabled = value; }
		}
	}
}