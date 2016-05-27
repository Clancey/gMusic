using System;
using System.IO;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

//[assembly: TestCloudApiKey("560a260a4c7544bf5076b037cbd04c18")]
namespace TestCloud.iOS
{
	public static class AppInitializer
	{
		public static IApp App { get; set; }

		public static IApp StartApp (Platform platform)
		{
			// TODO: If the iOS or Android app being tested is included in the solution 
			// then open the Unit Tests window, right click Test Apps, select Add App Project
			// and select the app projects that should be tested.
			if (platform == Platform.Android) {
				return App = ConfigureApp
					.Android
				// TODO: Update this path to point to your Android app and uncomment the
				// code if the app is not included in the solution.
				//.ApkFile ("../../../Droid/bin/Debug/xamarinforms.apk")
					.StartApp ();
			}

			return App = ConfigureApp
				.iOS
			// TODO: Update this path to point to your iOS app and uncomment the
			// code if the app is not included in the solution
			//.InstalledApp("com.IIS.MusicPlayer.iOS")
				.AppBundle ("../../../../MusicPlayer.iOS/bin/iPhoneSimulator/Debug/MusicPlayeriOS.app")
				.EnableLocalScreenshots()
				.StartApp ();
		}
	}
}

