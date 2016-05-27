using MusicPlayer.Cells;
using System;
using System.Collections.Generic;
using System.Text;
using MonoTouch.Dialog;

namespace MusicPlayer.iOS
{
	class SettingsElement : MenuElement
	{
		public SettingsElement(string title) : this (title,null)
		{
			Height = 35;
		}
		public SettingsElement (string title, Action tapped) : base(title,tapped)
		{
			Height = 35;
		}
		protected override void ApplyStyle(MenuElementCell cell)
		{
			base.ApplyStyle(cell);
			cell.TextLabel.TextColor = Style.DefaultStyle.MainTextColor;
			cell.ForceImage = false;
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.TextColor = Style.DefaultStyle.SubTextColor;
		}
	}
}
