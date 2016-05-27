using System;
using System.Collections.Generic;
using System.Text;
using Localizations;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer.iOS.ViewControllers
{
	class OnlineArtistDetailsViewController : BaseModelViewController<OnlineArtistDetailsViewModel,MediaItemBase>
	{
		public OnlineArtistDetailsViewController()
		{
			Model = new OnlineArtistDetailsViewModel();
		}

		public Artist Artist
		{
			get { return Model.Artist; }
			set
			{
				Model.Artist = value;
				Title = value.Name;
			}
		}

		public override void SetupEvents()
		{
			base.SetupEvents();
			Model.ItemSelected += Model_ItemSelected;
		}

		private async void Model_ItemSelected(object sender, SimpleTables.EventArgs<MediaItemBase> e)
		{
			//Check online first
			var onlineSong = e.Data as OnlineSong;
			if (onlineSong != null)
			{
				if (!await MusicManager.Shared.AddTemp(onlineSong))
				{
					App.ShowAlert(Strings.Sorry, Strings.ThereWasAnErrorPlayingTrack);
					return;
				}
				await PlaybackManager.Shared.PlayNow(onlineSong, onlineSong.TrackData.MediaType == MediaType.Video);
				return;
			}

			var song = e.Data as Song;
			if (song != null)
			{
				await PlaybackManager.Shared.PlayNow(song);
				return;
			}

			var album = e.Data as Album;
			if (album != null)
			{
				var vc = new AlbumDetailsViewController
				{
					Album = album,
				};

				this.NavigationController.PushViewController(vc, true);
				return;
			}

			var onlineArtist = e.Data as OnlineArtist;
			if (onlineArtist != null)
			{
				var vc = new OnlineArtistDetailsViewController
				{
					Artist = onlineArtist,
				};
				this.NavigationController.PushViewController(vc, true);
				return;
			}

			var artist = e.Data as Artist;
			if (artist != null)
			{
				var vc = new ArtistDetailViewController
				{
					Artist = artist,
				};
				this.NavigationController.PushViewController(vc, true);
				return;
			}
		}
	}
}
