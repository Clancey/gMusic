using AppKit;

namespace MusicPlayer.OSX
{
	static class MainClass
	{
		static void Main (string[] args)
		{

			NSApplication.CheckForIllegalCrossThreadCalls = false;
			NSApplication.Init ();
			NSApplication.Main (args);
		}
	}
}
