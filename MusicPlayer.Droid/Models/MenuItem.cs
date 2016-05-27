using System;

namespace MusicPlayer
{
	public class MenuItem
	{
		public MenuItem ()
		{
		}
		public string Title { get; set;}
		public Action<MenuItem> Tapped {get;set;}
	}
}

