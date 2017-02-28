using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using MusicPlayer.Forms;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using MusicPlayer.Managers;
using Localizations;
using BigTed;

namespace MusicPlayer.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{

		public const int AppId = 708727021;
		public const string AppName = "gMusic";
		nint playingBackground;
		public UIApplicationShortcutItem LaunchedShortcutItem { get; set; }
		public override bool FinishedLaunching(UIApplication app, NSDictionary launchOptions)
		{
			bool handled = true;
			MobileCenter.Start(ApiConstants.MobileCenterApiKey,
			                   typeof(Analytics), typeof(Crashes));
			global::Xamarin.Forms.Forms.Init();

			// Code for starting up the Xamarin Test Cloud Agent
#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
#endif
			if (launchOptions != null && UIApplication.LaunchOptionsShortcutItemKey != null)
			{
				LaunchedShortcutItem = launchOptions[UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
				handled = (LaunchedShortcutItem == null);
			}


			var screenBounds = UIScreen.MainScreen.Bounds;
			Images.MaxScreenSize = (float)NMath.Max(screenBounds.Width, screenBounds.Height);
			SetUpApp(app);
			app.BeginReceivingRemoteControlEvents();
			LoadApplication(new App());

			var s =  base.FinishedLaunching(app, launchOptions);
			return s;
		}

//		#if DEBUG
//		UIWindow carTestWindow;
//		public void TestCarInterface()
//		{

//			carTestWindow = new UIWindow(UIScreen.MainScreen.Bounds);
//			carTestWindow.Tag = 1;
//			var style = carTestWindow.GetStyle();
//			carTestWindow.TintColor = style.AccentColor;
//			if (carTestWindow.RootViewController == null)
//				carTestWindow.RootViewController = new Car.CarHeadViewController();

//			carTestWindow.Hidden = false;
//		}
//#endif


		public void SetUpApp(UIApplication app)
		{
			SimpleAuth.OnePassword.Activate();
			ApiManager.Shared.Load();
			App.AlertFunction = (t, m) => { new UIAlertView(t, m, null, "Ok").Show(); };
			App.Invoker = app.BeginInvokeOnMainThread;
			App.OnPlaying = () =>
			{
				if (playingBackground != 0)
					return;
				playingBackground = app.BeginBackgroundTask(() =>
				{
					app.EndBackgroundTask(playingBackground);
					playingBackground = 0;
				});
			};
			App.OnStopped = () =>
			{
				if (playingBackground == 0)
					return;
				app.EndBackgroundTask(playingBackground);
				playingBackground = 0;
			};

			App.OnShowSpinner = (title) => { BTProgressHUD.ShowContinuousProgress(title, ProgressHUD.MaskType.Clear); };

			App.OnDismissSpinner = BTProgressHUD.Dismiss;

#pragma warning disable 4014
			App.Start();
#pragma warning restore 4014
			//SetupHockeyApp();
		}

		public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
		{
			BackgroundDownloadManager.Shared.Init();
			BackgroundDownloadManager.Shared.RepairFromBackground(sessionIdentifier, completionHandler);
		}
		public override void OnResignActivation(UIApplication application)
		{
			PlaybackManager.Shared.NativePlayer.DisableVideo();
		}
		public override void OnActivated(UIApplication application)
		{
			PlaybackManager.Shared.NativePlayer.EnableVideo();
			HandleShortcut(LaunchedShortcutItem);
			LaunchedShortcutItem = null;
			//ScreenManager.Shared.OnActivated();
		}

		public override void WillEnterForeground(UIApplication application)
		{
			//window.RootViewController.ViewDidAppear(true);
			//ScreenManager.Shared.WillEnterForeground();
		}

		public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
		{
			NSObject idObj;
			if (!userActivity.UserInfo.TryGetValue(new NSString("kCSSearchableItemActivityIdentifier"), out idObj))
			{
				return false;
			}
			var id = idObj.ToString();
			PlaybackManager.Shared.PlaySong(id);
			return true;
		}


		bool HandleShortcut(UIApplicationShortcutItem shortcutItem)
		{
			if (shortcutItem == null)
				return false;

			switch (shortcutItem.Type)
			{
				//play
				case "com.IIS.MusicPlayer.iOS.000":
					PlaybackManager.Shared.Play();
					return true;
				case "com.IIS.MusicPlayer.iOS.001":

					//var vm = new MusicPlayer.ViewModels.RadioStationViewModel { IsIncluded = false };
					//var items = vm.RowsInSection(0);
					//if (items == 0)
					//	return true;
					//var radio = vm.ItemFor(0, 0);
					//Settings.ShowOfflineOnly = false;
					//PlaybackManager.Shared.Play(radio);
					return true;
			}

			return false;
		}

		public override void DidEnterBackground(UIApplication application)
		{
			//ScreenManager.Shared.DidEnterBackground();
		}
	}
}
