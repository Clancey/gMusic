using System;
using MusicPlayer.iOS.ViewControllers;
using MusicPlayer.Models;
using UIKit;
using MusicPlayer.ViewModels;
using Foundation;
using MusicPlayer.Managers;
using MusicPlayer.Data;

namespace MusicPlayer.iOS.ViewControllers
{
	[Register("SongViewController")]
	class SongViewController : BaseModelViewController<SongViewModel,Song>
	{
		public SongViewController()
		{
			Model = new SongViewModel();
			Title = Model.Title;
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Images.GetShuffleImage(24), UIBarButtonItemStyle.Plain, this, new ObjCRuntime.Selector("Shuffle"));
		}

		[Export("Shuffle")]
		public async void Shuffle()
		{
			Settings.ShuffleSongs = true;
			await PlaybackManager.Shared.Play(null, Model.GroupInfo);
		}
	}
}