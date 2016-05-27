using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using Xamarin.UITest;
using TestCloud.iOS;

namespace ScreenShotTaker
{
	class MainClass
	{
		const string simulatorFolder = "/Users/Clancey/Library/Developer/CoreSimulator/Devices";
		static string BasDir = "";
		static string ScreenShotFolder = "";

		public static void Main (string[] args)
		{ 
			Console.WriteLine ("Getting Simulators");
			var simulators = GetSimulators ().Where (x => x.Version == "com.apple.CoreSimulator.SimRuntime.iOS-9-1").ToList ();
			Console.WriteLine ("Found {0} simulators", simulators.Count);
			Console.WriteLine ("Creating Directories");
			CreateDirectories (simulators);
			simulators.Skip(2
			).ToList().ForEach (RunTests);
		}

		static void RunTests (Simulator sim)
		{
			try {
				ss = 1;
				Console.WriteLine ("Running {0}", sim.Name);
				var app = ConfigureApp.Debug ().iOS.EnableLocalScreenshots ()
				                      .AppBundle ("../../../../MusicPlayer.iOS/bin/iPhoneSimulator/Debug/MusicPlayeriOS.app")
				.DeviceIdentifier (sim.UDID).StartApp ();
				if(sim.Name.Contains("iPad"))
					app.SetOrientationLandscape();
				AppInitializer.App = app;
				//PlayMusicTest.PlaySong (app);
				LoginTest.CheckLogin();
				MenuTests.GotoArtist (app);
				sim.TakeScreenshot (app);
				Console.WriteLine ("Took Screenshot {0} - {1}", sim.Name, "Artist");

				MenuTests.GotoAlbums (app);
				sim.TakeScreenshot (app);
				Console.WriteLine ("Took Screenshot {0} - {1}", sim.Name, "Albums");

				MenuTests.GotoGenres (app);
				sim.TakeScreenshot (app);
				Console.WriteLine ("Took Screenshot {0} - {1}", sim.Name, "Genres");

				MenuTests.GotoSongs (app);
				sim.TakeScreenshot (app);
				Console.WriteLine ("Took Screenshot {0} - {1}", sim.Name, "Songs");

				MenuTests.GotoPlaylists (app);
				sim.TakeScreenshot (app);
				Console.WriteLine ("Took Screenshot {0} - {1}", sim.Name, "Playlists");

				MenuTests.GotoEqualizer (app);
				sim.TakeScreenshot (app);
				Console.WriteLine ("Took Screenshot {0} - {1}", sim.Name, "Equalizer");
			} catch (Exception ex) {

				Console.WriteLine (ex);
			}

		}

		static int ss = 1;

		static void CreateDirectories (List<Simulator> simulators)
		{
			BasDir = Path.GetDirectoryName (System.Reflection.Assembly.GetExecutingAssembly ().GetName ().CodeBase).Replace ("file:", "");
			ScreenShotFolder = Path.Combine (BasDir, "ScreenShots");
			Directory.CreateDirectory (ScreenShotFolder);

			simulators.ForEach (sim => Directory.CreateDirectory (sim.Directory));

		}

		static List<Simulator> GetSimulators ()
		{
			return Directory.EnumerateFiles (simulatorFolder, "device.plist", SearchOption.AllDirectories).Select (ProcessPlist).ToList ();
		}

		static Simulator ProcessPlist (string plist)
		{
			var plistDoc = new XmlDocument ();
			plistDoc.Load (plist);
			var dictNodes = plistDoc.GetElementsByTagName ("dict").Item (0).ChildNodes;
			var nodes = Enumerable.Range (0, dictNodes.Count / 2).Select (x => {
				var i = x * 2;
				return new Tuple<string,string> (dictNodes.Item (i).InnerText, dictNodes.Item (i + 1).InnerText);
			}).ToDictionary (x => x.Item1, x => x.Item2);
			return new Simulator {
				Name = nodes ["name"],
				UDID = nodes ["UDID"],
				Version = nodes ["runtime"],
			};
		}

		class Simulator
		{
			public string Name { get; set; }

			public string UDID { get; set; }

			public string Version { get; set; }

			public string Directory {
				get { return Path.Combine (ScreenShotFolder, Name); }
			}

			public void TakeScreenshot (IApp app)
			{
				app.Screenshot (ss.ToString ());
				var ssname = string.Format ("screenshot-{0}.png", ss);
				var newFile = Path.Combine (ScreenShotFolder, Name, ssname);
				if (File.Exists (newFile))
					File.Delete (newFile);
				File.Move (Path.Combine (BasDir, ssname), newFile);
				ss++;
			}
		}
	}
}
