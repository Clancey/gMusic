using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicPlayer
{
	internal static class Device
	{
		public static string Name {get;} = Android.OS.Build.User.Normalize();

	}
}