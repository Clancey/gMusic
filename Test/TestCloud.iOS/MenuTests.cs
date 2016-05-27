using NUnit.Framework;
using System;
using System.Linq;
using Xamarin.UITest;
using System.Threading;

namespace TestCloud.iOS
{
	[TestFixture (Platform.iOS)]
	public class MenuTests : BaseTestFixture
	{
		public MenuTests(Platform platform) : base(platform)
		{

		}
		//[Test ()]
		public void ShowMenu ()
		{
			ShowMenu (App);
		}
		const string menuOpenElement = "CloseMenuButton";
		public static void ShowMenu(IApp app)
		{
//			WaitForMenuState (app, false);
		//	app.Screenshot ("App Started");

			app.Tap (x => x.Button ("menu"));
			Thread.Sleep (500);
			app.Screenshot ("Menu");
		}

//		static void WaitForMenuState(IApp app,bool open)
//		{
//			if (open) {
//				app.Repl ();
//				app.WaitForElement (x => x.All ().Id (menuOpenElement));
//			}
//			else {
//				app.WaitForNoElement (x => x.All ().Id (menuOpenElement));
//			}
//		}

		public static void GoToTab(IApp app,string tab)
		{
			ShowMenu (app);
			app.Tap (x => x.Text (tab).ClassFull("UITableViewCell"));
			app.Screenshot (string.Format("Tap on {0}",tab));
		}
		//[Test ()]
		public void GotoArtist()
		{
			GotoArtist (App);
		}
		public static void GotoArtist(IApp App)
		{
			GoToTab (App,"Artists");
		}

		//[Test ()]
		public void GotoSearch()
		{
			GotoArtist (App);
		}
		public static void GotoSearch(IApp App)
		{
			GoToTab (App,"Search");
		}

		[Test]
		public void GotoAlbums()
		{
			GotoAlbums (App);
		}
		public static void GotoAlbums(IApp App)
		{
			GoToTab (App,"Albums");
		}
		#if gMusic

		[Test]
		public void GotoRadio()
		{
			GotoRadio (App);
		}
		#endif

		public static void GotoRadio(IApp App)
		{
			GoToTab (App,"Radio");
		}

		public static void GotoEqualizer(IApp App)
		{
			GoToTab (App,"Equalizer");
		}

		[Test]
		public void GotoSongs()
		{
			GotoSongs (App);
		}
		public static void GotoSongs(IApp App)
		{
			GoToTab (App,"Songs");
		}

		[Test]
		public void GotoGenres()
		{
			GotoGenres (App);
		}
		public static void GotoGenres(IApp App)
		{
			GoToTab (App,"Genres");
		}

		[Test]
		public void GotoPlaylists()
		{
			GotoPlaylists (App);
		}

		public static void GotoPlaylists(IApp App)
		{
			GoToTab (App,"Playlists");
		}

		//[Test ()]
		public void GotoArtistDetails()
		{
			GotoArtist ();
			App.Tap(x => x.Class("MusicPlayer_iOS_TwoLabelView"));
			App.Screenshot ("Artist Details");

		}

		[Test]
		public void GoToArtistAlbum ()
		{
			GotoArtistDetails ();
			App.Tap (x => x.ClassFull("UITableViewCell"));
			App.Screenshot ("Album Details");
		}

		public void PlayAlbum()
		{

		}

		#if gMusic
		[Test ()]
		public void GotoAllAccess()
		{
			GotoArtistDetails ();
			var items = App.Query (x=> x.ClassFull("UIButtonLabel")).ToList();
			App.SwipeRight ();
			Console.WriteLine (items.Count);
		}
		#endif
	}
}

