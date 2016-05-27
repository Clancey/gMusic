using System;
using System.Collections.Generic;
using System.Text;
using MonoTouch.Dialog;

namespace MusicPlayer.iOS
{
	class SettingsBooleanElement : BooleanElement
	{
		public SettingsBooleanElement(string title, bool value, Action<bool> valueChanged) : base(title, value)
		{
		}
	}
}
