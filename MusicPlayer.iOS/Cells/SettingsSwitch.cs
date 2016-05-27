using MusicPlayer.Cells;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayer.iOS
{
	class SettingsSwitch : MenuSwitch
	{
		public SettingsSwitch(string text, bool value) : base(text, value)
		{

		}
		protected override void ApplyStyle(BooleanMenuElementCell cell)
		{
			base.ApplyStyle(cell);
			cell.ForceImage = false;
			cell.TextLabel.TextColor = Style.DefaultStyle.MainTextColor;
			cell.DetailTextLabel.TextColor = Style.DefaultStyle.MainTextColor;
		}
	}
}
