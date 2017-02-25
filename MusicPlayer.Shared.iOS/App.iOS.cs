using System;
using System.Threading.Tasks;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public partial class App
	{
		
		public static async Task NativeStart()
		{
			PlaybackManager.Shared.Init();
			await BackgroundDownloadManager.Shared.Init();
		}
	}
}

