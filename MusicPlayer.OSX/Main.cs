using AppKit;

namespace MusicPlayer.OSX
{
	static class MainClass
	{
		static void Main (string[] args)
		{

			NSApplication.CheckForIllegalCrossThreadCalls = false;
			Xamarin.Insights.Initialize (ApiConstants.InsightsApiKey, "1.0", "com.iis.gmusic.mac");
			NSApplication.Init ();
			NSApplication.Main (args);
		}
	}
}
