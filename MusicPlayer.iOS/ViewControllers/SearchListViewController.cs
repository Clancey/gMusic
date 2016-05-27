using System;
using System.Collections.Generic;
using System.Text;
using Localizations;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;
using SimpleTables;

namespace MusicPlayer.iOS.ViewControllers
{
	class SearchListViewController : UITableViewController
	{
		public SearchListViewController()
		{
			this.Title = Strings.Search;
		}

		public SearchListViewModel Model
		{
			get { return model; }
			set
			{
				model = value;
				Title = model.Title;
			}
		}

		UISearchBar searchBar;
		SearchListViewModel model;

		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = Model;
			TableView.SectionIndexMinimumDisplayRowCount = 30;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			model.ItemSelected += ModelOnItemSelected;
			TableView.ReloadData ();
			this.StyleViewController();
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			model.ClearEvents();
		}

		async void ModelOnItemSelected(object sender, EventArgs<MediaItemBase> eventArgs)
		{
			//Check online first
			var onlineSong = eventArgs.Data as OnlineSong;
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

			var song = eventArgs.Data as Song;
			if (song != null)
			{
				await PlaybackManager.Shared.PlayNow(song);
				return;
			}

			var album = eventArgs.Data as Album;
			if (album != null)
			{
				var vc = new AlbumDetailsViewController
				{
					Album = album,
				};

				this.NavigationController.PushViewController(vc,true);
				return;
			}

			var onlineArtist = eventArgs.Data as OnlineArtist;
			if (onlineArtist != null)
			{
				var vc = new OnlineArtistDetailsViewController
				{
					Artist = onlineArtist,
				};
				this.NavigationController.PushViewController(vc, true);
				return;
			}

			var artist = eventArgs.Data as Artist;
			if (artist != null)
			{
				var vc = new ArtistDetailViewController
				{
					Artist = artist,
				};
				this.NavigationController.PushViewController(vc,true);
				return;
			}

			var onlineRadio = eventArgs.Data as OnlineRadioStation;
			if (onlineRadio != null)
			{
				using (new Spinner(Strings.CreatingStation))
				{
					var statsion = await MusicManager.Shared.CreateRadioStation(onlineRadio);
					await PlaybackManager.Shared.Play(statsion);
				}
				return;
			}
			var radio = eventArgs.Data as RadioStation;
			if(radio != null)
			{
				await PlaybackManager.Shared.Play(radio);
				return;
			}

			var onlinePlaylist = eventArgs.Data as OnlinePlaylist;
			if (onlinePlaylist != null)
			{
				var vc = new OnlinePlaylistViewController()
				{
					Playlist = onlinePlaylist,
				};
				this.NavigationController.PushViewController(vc, true);
				return;
			}
			App.ShowNotImplmented();
		}
	}
}
