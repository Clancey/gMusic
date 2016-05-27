
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.Res;
using MusicPlayer.Droid.UI;

namespace MusicPlayer.Droid
{
	public class NowPlayingActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Intent newIntent = null;
			UiModeManager uiModeManager = (UiModeManager)GetSystemService(Context.UiModeService);
			if (uiModeManager.CurrentModeType == Configuration.UiModeTypeTelevision)
			{
				//newIntent = new Intent(this, TvPlaybackActivity.class);
			}
			else
			{
				newIntent = new Intent(this, typeof(MusicPlayerActivity));
			}
			StartActivity(newIntent);
			Finish();
		}
	}
}

