using System;
using Foundation;

namespace MusicPlayer
{
	public static class Device
	{
		public static Version SystemVersion { get;} = GetSystemVersion();

		static Version GetSystemVersion()
		{
			var v = NSProcessInfo.ProcessInfo.OperatingSystemVersion;
			return new Version ((int)v.Major, (int)v.Minor, (int)v.PatchVersion);
		}

		public static bool IsSim { get;} = true;


		public static string Name {get;}// = NSHost.Current.Name;

		public static bool IsIos9 {get;} = SystemVersion > new Version(10,11);
	}
}

