using System;
using NUnit.Framework;
using Xamarin.UITest;
using System.Linq;

namespace TestCloud.iOS
{

	[TestFixture (Platform.iOS)]
	public class SearchTest : BaseTestFixture
	{
		public SearchTest(Platform platform) : base(platform)
		{

		}

		[Test]
		public void Search ()
		{

			App.Repl ();
			App.Tap(x=> x.Marked("menu"));
			App.Tap(x=> x.Marked("Search"));
			App.Screenshot("Search");
			App.EnterText(x=> x.Marked("searchBar"),"blink");
			App.Screenshot("Searching");
			App.WaitForNoElement (x => x.Marked ("In progress"));
			App.Screenshot("Search Results");
		}
	}
}

