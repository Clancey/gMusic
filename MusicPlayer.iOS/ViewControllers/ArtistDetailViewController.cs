using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Localizations;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using UIKit;
using MusicPlayer.Data;

namespace MusicPlayer.iOS.ViewControllers
{
	internal class ArtistDetailViewController : TopTabBarController
	{
		//ArtistAlbumsViewModel model;
		ArtistAlbumsViewController albumsController;
		OnlineArtistDetailsViewController onlineController;

		public ArtistDetailViewController()
		{
			HeaderHeight = 44;
		}

		public Artist Artist
		{
			set
			{
				Title = value.Name;
				SetupViewControllers(value);
			}
		}

		public void SetupViewControllers(Artist artist)
		{
			var onlineArtist = artist as OnlineArtist;
			if (onlineArtist != null)
			{
				ViewControllers = new[]
				{
					onlineController = new OnlineArtistDetailsViewController
					{
						Artist = artist,
						Title= Strings.Online,
					},
				};
				return;
			}
			var vcs = new List<UIViewController>();
			vcs.Add(albumsController = new ArtistAlbumsViewController
			{
				Artist = artist,
				Title = Strings.Albums
			});

			vcs.Add(new ArtistSongsViewController
			{
				Artist = artist,
				Title = Strings.Songs
			});
			if (!Settings.DisableAllAccess) {
				vcs.Add (onlineController = new OnlineArtistDetailsViewController {
					Artist = artist,
					Title = Strings.Online,
				});
			}

			ViewControllers = vcs.ToArray();
		}

		//public override void LoadView()
		//{
		//	base.LoadView();
		//	TableView.Source = model;
		//}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			if(albumsController != null)
				albumsController.AlbumSelected = (a) =>
				{
					NavigationController.PushViewController(new AlbumDetailsViewController
					{
						Album = a
					}, true);
				};
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			if(albumsController != null)
				albumsController.AlbumSelected = null;
		}
	}
}