using System;
using NUnit.Framework;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using TestCloud.iOS;

namespace TestCloud.iOS
{
	[TestFixture (Platform.iOS)]
	public class PlayMusicTest : BaseTestFixture
	{

		public PlayMusicTest(Platform platform) : base(platform)
		{

		}
		public static void PlaySong(IApp app)
		{
			MenuTests.GotoSongs (app);
			app.Tap(x => x.Class("MusicPlayer_Cells_MediaItemCellView").Index(2));
			//app.Screenshot ("Playback started");
//			WaitForPlaybackPercent (app, 10);
//			app.Screenshot ("Played Half song");
		}
		[Test]
		public void PlaySong()
		{
			MenuTests.GotoSongs (App);
			App.Tap (x => x.All().ClassFull ("MusicPlayer_Cells_MediaItemCellView").Index(2));
			App.Screenshot ("Playback started");
			WaitForPlaybackPercent (App, 10);
			App.Screenshot ("Played Half song");
		}

		#if gMusic
		[Test]
		public void PlayAllAccessSong()
		{
			
			SearchTest.SearchFor (App,"Selfie");
			//getallviews (App);
			App.Tap (x => x.ClassFull("gMusic_SongCell_Cell").Marked ("#Selfie"));
			App.Screenshot ("Playback started");
			WaitForPlaybackPercent (App, .1);
			App.Screenshot ("Played Half song");
		}


		[Test]
		public void PlayLedZeplin()
		{
			SearchTest.SearchFor (App,"Thunder");
			App.Tap (x => x.All().ClassFull ("gMusic_SongCell_Cell").Index(0));
			App.Screenshot ("Playback started");
			WaitForPlaybackPercent (App, .1);
			App.Screenshot ("Played Half song");
		}

		[Test]
		public void PlayFromSearch()
		{
			SearchTest.SearchFor (App, "Led Zeppelin");
			App.Tap (x => x.All().ClassFull ("gMusic_SongCell_Cell").Index(0));
			App.Screenshot ("Playback started");
			WaitForPlaybackPercent (App, .1);
			App.Screenshot ("Played Half song");
		}

		#endif

		public static void WaitForPlaybackPercent(IApp app, int seconds)
		{
			app.Tap(x => x.Class("MusicPlayer_iOS_TwoLabelView").Id("NowPlayingBar"));
			app.Screenshot("Now Playing Screen");
			app.WaitFor (() => {
				var text = app.Query(x => x.Marked("CurrentTime")).FirstOrDefault()?.Text; //(double)app.Query<float> (x => new AppTypedSelector<float> ( x.Class("UIKit_OBSlider").Id("Progress"),new []{ "value"})).FirstOrDefault ();
				if (string.IsNullOrWhiteSpace(text))
					return false;
				var valString = text.Split(':').LastOrDefault();
				int val;
				int.TryParse(valString, out val);
				return val > seconds;
			},retryFrequency:new TimeSpan(0,0,10),timeout: new TimeSpan(0,6,0));
		}

	
	}

}

