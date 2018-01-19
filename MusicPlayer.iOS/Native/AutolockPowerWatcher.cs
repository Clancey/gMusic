using System;
using MusicPlayer.Managers;
using Plugin.Battery;
using UIKit;
using MusicPlayer.Data;
namespace MusicPlayer.iOS
{
	public class AutolockPowerWatcher : ManagerBase<AutolockPowerWatcher>
	{
		public AutolockPowerWatcher()
		{
			CrossBattery.Current.BatteryChanged += (s,e) =>{
				CheckStatus();
			};
		}
		public void CheckStatus()
		{
			UIApplication.SharedApplication.IdleTimerDisabled = ScreenManager.Shared.IsRunning ||
				Settings.DisableAutoLock && CrossBattery.Current.PowerSource != Plugin.Battery.Abstractions.PowerSource.Battery;
		}
	}
}
