using System;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class EqualizerMenuElement : MenuSwitchElement
	{
		public EqualizerMenuElement ()
		{
			Subtext = MusicPlayer.Playback.Equalizer.Shared.CurrentPreset?.Name;
			Value = MusicPlayer.Playback.Equalizer.Shared.Active;
			NotificationManager.Shared.EqualizerChanged += (object sender, EventArgs e) => {
				Subtext = MusicPlayer.Playback.Equalizer.Shared.CurrentPreset?.Name;
				Cell?.UpdateValues();
			};
			NotificationManager.Shared.EqualizerEnabledChanged += (object sender, EventArgs e) => {
				Value = MusicPlayer.Playback.Equalizer.Shared.Active;
				Cell?.UpdateValues();
			};
		}
		WeakReference _cell;
		MenuSwitchCell Cell {
			get {
				return _cell?.Target  as MenuSwitchCell;
			}
			set {
				_cell = new WeakReference(value);
			}
		}
		public override AppKit.NSView GetView (AppKit.NSTableView tableView, Foundation.NSObject sender)
		{
			var view = base.GetView (tableView, sender);
			Cell = view as MenuSwitchCell;
			return view;
		}
	}
}

