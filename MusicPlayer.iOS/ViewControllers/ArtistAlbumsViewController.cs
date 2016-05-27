using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer.iOS.ViewControllers
{
	internal class ArtistAlbumsViewController : BaseTableViewController
	{
		ArtistAlbumsViewModel model;

		public Action<Album> AlbumSelected { get; set; }

		public ArtistAlbumsViewController()
		{
			model = new ArtistAlbumsViewModel();
		}

		public Artist Artist
		{
			set
			{
				model.Artist = value;
				Title = value.Name;
			}
		}

		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = model;
		}

		public override void SetupEvents()
		{
			base.SetupEvents();
			model.ItemSelected += (object sender, SimpleTables.EventArgs<MusicPlayer.Models.Album> e) =>
			{
				if (AlbumSelected != null)
				{
					AlbumSelected(e.Data);
					return;
				}
				NavigationController.PushViewController(new AlbumDetailsViewController
				{
					Album = e.Data
				}, true);
			};
		}

		public override void TeardownEvents()
		{
			base.TeardownEvents();
			model.ClearEvents();
		}
	}
}