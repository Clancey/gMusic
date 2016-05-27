using System;
using MusicPlayer.Cells;
using NGraphics;
using UIKit;
using MusicPlayer.Managers;
using MusicPlayer.Playback;

namespace MusicPlayer.iOS
{
	class MenuSubtextSwitch : MenuSwitch
	{
		string subtext;

		public MenuSubtextSwitch(string caption, string subtext, bool value)
			: base(caption, value)
		{
			this.subtext = subtext;
		}

		public MenuSubtextSwitch(string caption, string subtext, string svg, bool value)
			: base(caption, value)
		{
			this.subtext = subtext;
			image = svg.LoadImageFromSvg(new Size(28, 28), UIImageRenderingMode.AlwaysTemplate);
			NotificationManager.Shared.EqualizerChanged += NotificationManager_Shared_EqualizerChanged;
			;
		}

		void NotificationManager_Shared_EqualizerChanged(object sender, EventArgs e)
		{
			var cell = currentcell?.Target as UITableViewCell;
			if (cell == null)
				return;
			cell.DetailTextLabel.Text = Equalizer.Shared.CurrentPreset?.Name ?? "";
		}

		WeakReference currentcell;

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = base.GetCell(tv);
			cell.DetailTextLabel.Text = subtext ?? "";
			currentcell = new WeakReference(cell);
			return cell;
		}

	}
}

