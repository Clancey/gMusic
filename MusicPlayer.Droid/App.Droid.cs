using System;
using Android.Content;
using Android.App;
using Android.Views;
using System.Threading.Tasks;
using MusicPlayer.Droid.UI;

namespace MusicPlayer
{
	public static partial class App
	{
		public static void ShowMessage (string title, string message, string close = "Ok")
		{

		}

		internal static MusicPlayerActivity Context { get; set; }

		public static ISurfaceHolder SurfaceHolder;
		public static Action<Fragment> DoPushFragment;
		public static void PushFragment(Fragment fragment)
		{
			DoPushFragment (fragment);
		}

		public static async Task NativeStart()
		{

		}
	}
}

