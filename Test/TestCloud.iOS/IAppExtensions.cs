using System;
using Xamarin.UITest;
using System.Linq;
using System.Threading;

namespace TestCloud.iOS
{
	public static class IAppExtensions
	{
		public static void SignIn (this IApp app, bool screenshot = false)
		{
			if(screenshot)
				app.Screenshot ("Loading");

			var shouldLogin = app.Query (x => x.Class ("MusicPlayer_iOS_SimpleButton").Marked ("Login")).Any ();
			if (!shouldLogin)
				return;
			app.Tap(x => x.Class("MusicPlayer_iOS_SimpleButton").Marked("Login"));
			if(screenshot)
				app.Screenshot("Tapped on view MusicPlayer_iOS_SimpleButton");

			app.Tap(x => x.Class("UIWebView").Css("#Email"));
			if(screenshot)
				app.Screenshot("Tapped on view UIWebView");

			app.EnterText(x => x.Class("UIWebView").Css("#Email"), Constants.Password);
			if(screenshot)
				app.Screenshot("Entered UserName into view UIWebView with Text: 'gmusic'");

			app.EnterText(x => x.Class("UIWebView").Css("#Passwd"), Constants.Password);

			if(screenshot)
				app.Screenshot("Entered Password");
			app.Tap(x => x.Class("UIWebView").Css("#signIn"));
			if(screenshot)
				app.Screenshot("Tapped Login");
			
			if(app.Query(x=> x.ClassFull ("BigTed_ProgressHUD")).Count() > 0)
				app.WaitForNoElement (x => x.ClassFull ("BigTed_ProgressHUD"));

			app.WaitForElement (x => x.ClassFull ("UITableViewCell"));

			app.WaitForNoElement (x => x.Class ("MusicPlayer_iOS_SimpleButton").Marked ("Login"),timeout:new TimeSpan(0,5,0));
			if(screenshot)
				app.Screenshot ("Login complete");
		}

		public static void Tap(this IApp app, string marked)
		{
			app.Tap (x => x.Marked (marked));
		}
	}
}

