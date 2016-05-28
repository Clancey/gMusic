using System;
using MusicPlayer.Managers;
using UIKit;
using System.Threading.Tasks;
using System.Linq;
namespace MusicPlayer.iOS
{
	public class ScreenManager : ManagerBase<ScreenManager>
	{
		public ScreenManager()
		{
			UIScreen.Notifications.ObserveDidConnect((object sender, Foundation.NSNotificationEventArgs e)  => {
				StartWindow();
			});
			UIScreen.Notifications.ObserveDidDisconnect ((object sender, Foundation.NSNotificationEventArgs e) => {
				StopWindow();
			});
			UIScreen.Notifications.ObserveModeDidChange ((object sender, Foundation.NSNotificationEventArgs e) => {
				StartWindow();
			});

		}
		public void Init()
		{
			StartWindow ();
		}

		UIWindow Window;
		public virtual async Task StartWindow()
		{
			await Task.Delay (500);
			if (UIScreen.Screens.Length < 2)
				return;
			var screen = UIScreen.Screens.Last ();
			if (Window == null)
				Window = new UIWindow (screen.Bounds);
			else
				Window.Bounds = screen.Bounds;
			Window.Tag = 1;
			var style = Window.GetStyle ();
			Window.Screen = screen;
			Window.TintColor = style.AccentColor;
			if (Window.RootViewController == null)
				Window.RootViewController = new Car.CarHeadViewController ();
			Window.Hidden = false;
			UIApplication.SharedApplication.IdleTimerDisabled = true;
		}

		public virtual void StopWindow()
		{
			UIApplication.SharedApplication.IdleTimerDisabled = false;
			if (Window == null)
				return;
			Window.Hidden = true;
			UIApplication.SharedApplication.Windows[0].RootViewController.DismissModalViewController(true);
		}


		public virtual void WillTerminate()
		{
			
		}
		public virtual void OnActivated()
		{
			StartWindow ();
		}
		public virtual void DidEnterBackground()
		{
			
		}
		public virtual void OnResignActivation()
		{

		}
		public virtual void WillEnterForeground()
		{
			
		}

	}
}

