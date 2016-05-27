using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer.iOS.ViewControllers
{
	class OnlinePlaylistViewController : BaseModelViewController<OnlinePlaylistViewModel,OnlinePlaylistEntry>
	{
		public OnlinePlaylistViewController()
		{
			Model = new OnlinePlaylistViewModel();
		}

		public OnlinePlaylist Playlist
		{
			get { return Model.Playlist; }
			set
			{
				Model.Playlist = value;
				Title = Playlist.Name;
			}
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			Model.Refresh();
		}
	}
}
