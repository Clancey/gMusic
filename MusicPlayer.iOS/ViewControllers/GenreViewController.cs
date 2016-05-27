using MusicPlayer.ViewModels;
using UIKit;

namespace MusicPlayer.iOS.ViewControllers
{
	public class GenreViewController : BaseTableViewController
	{
		GenreViewModel model;

		public GenreViewController()
		{
			model = new GenreViewModel();
			Title = model.Title;
		}

		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = model;
		}

		public override void SetupEvents()
		{
			base.SetupEvents();
			model.GoToArtist = artist =>
			{
				if (artist.AlbumCount > 1)
					NavigationController.PushViewController(new ArtistDetailViewController {Artist = artist}, true);
				else
					NavigationController.PushViewController(new AlbumDetailsViewController {Artist = artist}, true);
			};

			model.GoToArtistList =
				(genre, info) =>
				{
					NavigationController.PushViewController(new ArtistViewController {Title = genre.Name, GroupInfo = info}, true);
				};
		}

		public override void TeardownEvents()
		{
			base.TeardownEvents();
			model.ClearEvents();
		}
	}
}