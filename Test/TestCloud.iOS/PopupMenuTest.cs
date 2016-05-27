using System;
using NUnit.Framework;
using Xamarin.UITest;

namespace TestCloud.iOS
{
	[TestFixture (Platform.iOS)]
	public class PopupMenuTest : BaseTestFixture
	{

		public PopupMenuTest(Platform platform) : base(platform)
		{

		}
		//[Test]
		//public void LongPressOnArtist ()
		//{
		//	MenuTests.GotoArtist (App);
		//	App.Tap (x => x.Class("MusicPlayer_iOS_SimpleButton").Marked("more").Index(0));
		//	App.Screenshot ("Artist Menu");
		//	App.Tap (x => x.Marked ("Shuffle"));
		//	App.Screenshot("Shuffled Playback");
		//}
	}
}

