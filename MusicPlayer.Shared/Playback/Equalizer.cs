using System;
using System.Collections.Generic;
using System.Linq;
using SimpleDatabase;
using System.Diagnostics;
using System.Threading.Tasks;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using MusicPlayer.Models;

namespace MusicPlayer.Playback
{
	public partial class Equalizer
	{
		public Equalizer()
		{
			Bands = TenBandPreset();
			LoadPresets();
			CurrentPreset = StateManager.Shared.GlobalEqualizerPreset > 0
				? Presets.FirstOrDefault(x => x.Id == StateManager.Shared.GlobalEqualizerPreset)
				: Presets.FirstOrDefault(x => x.Name == "Flat") ?? Presets.FirstOrDefault();
		}

		static Equalizer shared;

		public readonly Band[] Bands = new Band[0];

		EqualizerPreset currentPreset;

		public EqualizerPreset CurrentPreset
		{
			get { return currentPreset; }
			set
			{
				var oldPreset = currentPreset;
				if (currentPreset == value)
					return;
				currentPreset = value;
				if (oldPreset != null || oldPreset == value)
				{
					StateManager.Shared.GlobalEqualizerPreset = currentPreset.Id;
					NotificationManager.Shared.ProcEqualizerChanged();
				}
			}
		}

		public List<EqualizerPreset> Presets = new List<EqualizerPreset>();
		bool active = StateManager.Shared.EqualizerEnabled;

		public bool Active
		{
			get { return active; }
			set
			{
				active = value;
				StateManager.Shared.EqualizerEnabled = active;
				if (active)
					ApplyEqualizer ();
				else
					Clear ();

			}
		}

		public static Equalizer Shared
		{
			get { return shared ?? (shared = new Equalizer()); }
			set { shared = value; }
		}

		public void LoadPresets(bool force = false)
		{
			if (Presets.Count == 0 || force)
			{
				var groupInfo = new GroupInfo {OrderBy = "Name"};
				Presets = Database.Main.GetObjects<EqualizerPreset>(groupInfo).OrderBy(x => x.Name).ToList();
				if (Presets.Count == 0 || force)
				{
					foreach (var p in DefaultPresets)
					{
						p.Save();
					}
					Presets = Database.Main.GetObjects<EqualizerPreset>(groupInfo);
				}


				EqualizerPreset preset = Presets.FirstOrDefault(x => x.Id == StateManager.Shared.GlobalEqualizerPreset);
				if (preset != null)
					ApplyPreset(preset);
			}
		}

		public void Clear ()
		{
			for (int i = 0; i < Bands.Length; i++) {
				UpdateBand (i, 0);
			}
		}
		public async Task ApplyEqualizer()
		{
			if (!StateManager.Shared.EqualizerEnabled)
				return;
			LoadPresets();
			if (string.IsNullOrEmpty(Settings.CurrentSong))
				return;

#if iPod
			var curEq = await GetPreset(AudioPlayer.Shared.CurrentSong) ?? await GetDefault(AudioPlayer.Shared.CurrentSong) ?? CurrentPreset; 
			ApplyPreset(curEq);
			CurEqId = curEq.GlobalId;
#else
#endif
			PlaybackManager.Shared.NativePlayer.ApplyEqualizer (Bands);
		}

		public void UpdateBand(int band, float gain, bool slider = false)
		{
#if iPod
			if(slider && CurrentPreset.GlobalId != CurEqId)
				ApplyPreset (CurrentPreset);
#endif

			PlaybackManager.Shared.NativePlayer.UpdateBand (band, gain);
		}

		public void ApplyPreset(EqualizerPreset preset)
		{
			CurrentPreset = preset;
			for (int i = 0; i < preset.Values.Count(); i++)
			{
				var gain = (float) preset.Values[i].Value;
				Bands[i].Gain = gain;
				UpdateBand(i, gain);
			}
		}

		#region classes

		public static Band[] TenBandPreset()
		{
			return new[]
			{
				new Band {Center = 32},
				new Band {Center = 64},
				new Band {Center = 125},
				new Band {Center = 250},
				new Band {Center = 500},
				new Band {Center = 1000},
				new Band {Center = 2000},
				new Band {Center = 4000},
				new Band {Center = 8000},
				new Band {Center = 16000},
			};
		}


		public Band[] ThreeBandPreset()
		{
			return new[]
			{
				new Band {Center = 100},
				new Band {Center = 1000},
				new Band {Center = 8000},
			};
		}

		public static List<EqualizerPreset> DefaultPresets => new List<EqualizerPreset>()
		{
			new EqualizerPreset()
			{
				Name = "Acoustic",
				GlobalId = "705c1dc4-534a-4ffd-ac8c-186bf863a8fb",
				DoubleValues = new double[10]
				{
					4.5,
					4.5,
					3.5,
					.5,
					1.5,
					1,
					3,
					3.5,
					3,
					1.5,
				}
			},
			new EqualizerPreset()
			{
				Name = "Bass Booster",
				GlobalId = "fa35bd60-0492-4573-b3c6-c50ba73e6ce6",
				DoubleValues = new double[10]
				{
					4.5,
					3.5,
					3,
					2.5,
					1,
					-.5,
					0,
					0,
					0,
					0,
				}
			},
			new EqualizerPreset()
			{
				Name = "Bass Reducer",
				GlobalId = "f579326d-7cae-4984-8c4e-9c4435bee35a",
				DoubleValues = new double[10]
				{
					-4.5,
					-3.5,
					-3,
					-2.5,
					-1,
					-.5,
					0,
					0,
					0,
					0,
				}
			},
			new EqualizerPreset()
			{
				Name = "Classical",
				GlobalId = "f75736dd-f282-4508-9d03-48eba2025a1f",
				DoubleValues = new double[10]
				{
					4.5,
					3,
					2.5,
					2,
					-2,
					-2,
					0,
					1.5,
					3,
					3,
				}
			},
			new EqualizerPreset()
			{
				Name = "Dance",
				GlobalId = "571a5773-afc5-4ac3-908d-41172ebfe787",
				DoubleValues = new double[10]
				{
					3,
					6,
					4.5,
					0,
					1.5,
					3,
					4.5,
					4,
					3,
					0,
				}
			},
			new EqualizerPreset()
			{
				Name = "Deep",
				GlobalId = "7456a18e-ce64-4106-9227-0af572c5fc34",
				DoubleValues = new double[10]
				{
					4.5,
					3,
					1.5,
					1,
					2.5,
					2,
					1,
					-3,
					-4,
					-4.5,
				}
			},
			new EqualizerPreset()
			{
				Name = "Electronic",
				GlobalId = "459f15bc-bcce-40c5-8804-953bff168086",
				DoubleValues = new double[10]
				{
					4,
					3,
					1,
					-1,
					-3,
					1.5,
					0,
					1,
					4,
					4.5,
				}
			},
			new EqualizerPreset()
			{
				Name = "Flat",
				GlobalId = "e109b353-d0a7-44aa-b529-cc3953f9a108",
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
			},
			new EqualizerPreset()
			{
				Name = "Hip-Hop",
				GlobalId = "f30f2ab1-377e-44e0-8e16-88c34f582db9",
				DoubleValues = new double[10]
				{
					4.5,
					4,
					1,
					2.5,
					-1.5,
					-1.5,
					1,
					-1,
					1.5,
					2.5,
				}
			},
			new EqualizerPreset()
			{
				Name = "Jazz",
				GlobalId = "863370bc-62e9-4933-8c8b-3fa113dbf325",
				DoubleValues = new double[10]
				{
					4,
					2,
					1,
					1.5,
					-2.5,
					-2.5,
					-1,
					1,
					2,
					3,
				}
			},
			new EqualizerPreset()
			{
				Name = "Latin",
				GlobalId = "1194cf10-7639-4274-aaab-f83c671b8365",
				DoubleValues = new double[10]
				{
					4,
					2.5,
					-1,
					-1,
					-2,
					-2,
					-2,
					0,
					2.5,
					4,
				}
			},
			new EqualizerPreset()
			{
				Name = "Loudness",
				GlobalId = "bb54fc35-080e-4083-b071-4e0ca51c61d1",
				DoubleValues = new double[10]
				{
					5.5,
					4,
					-.5,
					-.5,
					-3,
					-.5,
					-1.5,
					-5.5,
					4.5,
					1,
				}
			},
			new EqualizerPreset()
			{
				Name = "Lounge",
				GlobalId = "d6839709-756a-49e4-8b4c-9e95712cfa95",
				DoubleValues = new double[10]
				{
					-4,
					-2,
					-1,
					1,
					3,
					2,
					0,
					-2,
					1.5,
					.5,
				}
			},
			new EqualizerPreset()
			{
				Name = "Piano",
				GlobalId = "de544872-c0c1-4635-b926-3ea6ec9e887e",
				DoubleValues = new double[10]
				{
					2.5,
					1.5,
					-.5,
					2,
					2.5,
					1,
					3,
					3.5,
					2.5,
					3,
				}
			},
			new EqualizerPreset()
			{
				Name = "Pop",
				GlobalId = "24e49867-2070-48bc-a15b-9ff7018c2a36",
				DoubleValues = new double[10]
				{
					-2,
					-1.5,
					-.5,
					1.5,
					3,
					3,
					1.5,
					0,
					-1.5,
					-2,
				}
			},
			new EqualizerPreset()
			{
				Name = "R&B",
				GlobalId = "edb0d3a9-44d5-4247-9a67-522d485c0274",
				DoubleValues = new double[10]
				{
					2.5,
					7,
					5.5,
					1,
					-3,
					-2,
					2,
					2.5,
					2.5,
					3,
				}
			},
			new EqualizerPreset()
			{
				Name = "Rock",
				GlobalId = "cd4c5888-5cec-4146-a7f2-e8489f8b860b",
				DoubleValues = new double[10]
				{
					4.5,
					4,
					3,
					1,
					-1,
					-1.5,
					0,
					2.5,
					3,
					3.5
				}
			},
			new EqualizerPreset()
			{
				Name = "Small Speakers",
				GlobalId = "7ec3aff2-4be3-43f1-92f3-605b8d8daba8",
				DoubleValues = new double[10]
				{
					5,
					3.5,
					3,
					2,
					1,
					-.5,
					-1.5,
					-3,
					-4,
					-4.5,
				}
			},
			new EqualizerPreset()
			{
				Name = "Spoken Word",
				GlobalId = "cd46e475-e875-42e8-a1d4-00e9eb6ca648",
				DoubleValues = new double[10]
				{
					-4,
					-1,
					-.5,
					.5,
					3,
					4,
					4.5,
					4,
					2,
					0,
				}
			},
			new EqualizerPreset()
			{
				Name = "Treble Booster",
				GlobalId = "c54d7422-86db-42e3-8ce7-613e29f79e5d",
				DoubleValues = new double[10]
				{
					0,
					0,
					0,
					0,
					0,
					1,
					2,
					3,
					4,
					5,
				}
			},
			new EqualizerPreset()
			{
				Name = "Treble Reducer",
				GlobalId = "31b44168-4a5e-4cc8-bbf4-0c623f0851ce",
				DoubleValues = new double[10]
				{
					0,
					0,
					0,
					0,
					0,
					-1.5,
					-3,
					-4,
					-4.5,
					-6,
				}
			},
			new EqualizerPreset()
			{
				Name = "Vocal Booster",
				GlobalId = "f05b233f-d1f3-4e7e-8b30-16f08c3d98f3",
				DoubleValues = new double[10]
				{
					-2,
					-3,
					-3,
					1,
					3,
					3,
					2.5,
					1,
					0,
					-2.5,
				}
			},
		};

		public class Band
		{
			public float Center { get; set; }

			public float Gain { get; set; }

			public override string ToString()
			{
				if (Center < 1000)
					return string.Format("{0}", Center);
				return string.Format("{0}K", Center/1000);
			}
		}

		#endregion

		public void Reset()
		{
			Presets.Clear();
			LoadPresets(true);

			CurrentPreset = StateManager.Shared.GlobalEqualizerPreset > 0
				? Presets.FirstOrDefault(x => x.Id == StateManager.Shared.GlobalEqualizerPreset)
				: Presets.FirstOrDefault(x => x.Name == "Flat") ?? Presets.FirstOrDefault();
		}
	}
}