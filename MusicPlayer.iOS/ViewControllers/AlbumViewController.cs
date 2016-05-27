using System;
using MusicPlayer.Data;
using MusicPlayer.iOS.ViewControllers;
using MusicPlayer.Models;
using UIKit;
using MusicPlayer.ViewModels;

namespace MusicPlayer.iOS
{
	public class AlbumViewController : BaseTableViewController
	{
		public AlbumViewController()
		{
			model = new AlbumViewModel();
			Title = model.Title;
		}

		AlbumViewModel model;

		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = model;
		}

		public override void TeardownEvents()
		{
			base.TeardownEvents();
			model.ClearEvents();
		}

		public override void SetupEvents()
		{
			base.SetupEvents();
			model.ItemSelected += (object sender, SimpleTables.EventArgs<MusicPlayer.Models.Album> e) =>
			{
				GoToAlbum(e.Data);
			};
		}

		public void GoToAlbum(string albumId)
		{
			var album = Database.Main.GetObject<Album,TempAlbum>(albumId);
			GoToAlbum(album);
		}
		public void GoToAlbum(Album album)
		{
			NavigationController.PushViewController(new AlbumDetailsViewController
			{
				Album = album
			}, true);
		}
	}
}