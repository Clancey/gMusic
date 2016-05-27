using System;
using NUnit.Framework;
using Xamarin.UITest;
using System.Linq;

namespace TestCloud.iOS
{
	[TestFixture (Platform.iOS)]
	public class LoginTest : BaseTestFixture
	{

		public LoginTest(Platform platform) : base(platform)
		{

		}

		public override bool RequiresLogin {
			get {
				return false;
			}
		}
		public static void CheckLogin(bool screenshot = false)
		{
			var app = AppInitializer.App;
			if (!app.Query (x => x.Class ("UIWebView")).Any () && app.Query (x => x.Class ("MusicPlayer_iOS_SimpleButton").Marked ("Login")).Length == 0)
				return;
			if(app.Query(x => x.Class("MusicPlayer_iOS_SimpleButton").Marked("Login")).Any())
				app.Tap(x => x.Class("MusicPlayer_iOS_SimpleButton").Marked("Login"));
			app.Screenshot("Tapped on view MusicPlayer_iOS_SimpleButton");
			app.Tap(x => x.Class("UIWebView").Css("#Email"));
			if(screenshot)
				app.Screenshot("Tapped on view UIWebView");
			app.EnterText(x => x.Class("UIWebView").Css("#Email"), Constants.Username);
			if(screenshot)
				app.Screenshot("Entered 'gmusic@yourisolutions.com' into view UIWebView");
			app.EnterText(x => x.Class("UIWebView").Css("#Passwd"), Constants.Password);
			if(screenshot)
				app.Screenshot("Entered '<Enter Password>' into view UIWebView");
			app.Tap(x => x.Class("UIWebView").Css("#signIn"));
			if(screenshot)
				app.Screenshot("Tapped Signin");


			if(app.Query(x=> x.ClassFull ("BigTed_ProgressHUD")).Count() > 0)
				app.WaitForNoElement (x => x.ClassFull ("BigTed_ProgressHUD"));

			app.WaitForNoElement (x => x.Class ("MusicPlayer_iOS_SimpleButton").Marked ("Login"),timeout:new TimeSpan(0,5,0));


		}
		[Test]
		public void Login ()
		{
			CheckLogin (true);
		}
	}
}

