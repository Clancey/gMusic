using System;
namespace MusicPlayer.Server
{
	public class PathAttribute : Attribute
	{
		public PathAttribute (string path)
		{
			Path = path;
		}
		public string Path { get; set; }
	}
}

