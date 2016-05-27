using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayer
{
	internal class Spinner : IDisposable
	{
		public Spinner(string title)
		{
			App.ShowSpinner(title);
		}

		public void Dispose()
		{
			App.DismissSpinner();
		}
	}
}