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
			

		}

		UIWindow Window;
		public async Task StartWindow()
		{
			//TODO: Add external display
		}

		public void StopWindow()
		{
			UIApplication.SharedApplication.IdleTimerDisabled = false;
			if (Window == null)
				return;
			Window.Hidden = true;
			UIApplication.SharedApplication.Windows[0].RootViewController.DismissModalViewController(true);
		}



		public void WillTerminate()
		{
			
		}
		public void OnActivated()
		{
			
		}
		public void DidEnterBackground()
		{
			
		}
		public void OnResignActivation()
		{

		}
		public void WillEnterForeground()
		{
			
		}

	}
}

