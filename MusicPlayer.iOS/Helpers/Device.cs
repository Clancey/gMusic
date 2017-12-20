using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using UIKit;

namespace MusicPlayer
{
	internal static class Device
	{
		static Version version = Version.Parse(UIDevice.CurrentDevice.SystemVersion);
		public static bool IsIos8 => version.Major >= 8;
		public static bool IsIos9 => version.Major >= 9;
		public static bool IsIos10 => version.Major >= 10;

		public static bool IsIos11 => version.Major >= 11;

		public static bool HasIntegratedTwitter => !IsIos11;

		public static bool IsIos7_1 => version > new Version(7, 1);

		public static string Name { get; } = UIKit.UIDevice.CurrentDevice.Name;

		public static bool IsSim { get; } = ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR;
		public static string AppVersion()
		{
			var build = NSBundle.MainBundle.InfoDictionary.ValueForKey((NSString)"CFBundleVersion");
			var version = NSBundle.MainBundle.InfoDictionary.ValueForKey((NSString)"CFBundleShortVersionString");
			return $"{version} ({build})";
		}
	}
}