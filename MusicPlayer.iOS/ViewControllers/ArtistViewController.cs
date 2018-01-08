using MusicPlayer.Data;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using SimpleDatabase;
using System.Linq;

namespace MusicPlayer.iOS.ViewControllers
{
	public class ArtistViewController : BaseTableViewController
	{
		ArtistViewModel model;

		public ArtistViewController()
		{
			model = new ArtistViewModel();
			Title = model.Title;
		}

		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = model;
		}

		public GroupInfo GroupInfo
		{
			get { return model.GroupInfo; }
			set { model.GroupInfo = value; }
		}

		public override void SetupEvents()
		{
			base.SetupEvents();
			model.ItemSelected += (object sender, SimpleTables.EventArgs<Artist> e) =>
			{
				GoToArtist(e.Data);
			};
		}


		public override void TeardownEvents()
		{
			base.TeardownEvents();
			model.ClearEvents();
		}

		public void GoToArtist(string artistId)
		{
			var artist = Database.Main.GetObject<Artist, TempArtist>(artistId);
			if (artist is TempArtist)
			{
				var onlineId = Database.Main.Query<ArtistIds>("select * from TempArtistIds where ArtistId = ?",artistId).FirstOrDefault();
				if(onlineId == null)	
					onlineId = Database.Main.Query<ArtistIds>("select * from ArtistIds where ArtistId = ?", artistId).FirstOrDefault();
				artist = new OnlineArtist(artist.Name, artist.NameNorm)
				{
					OnlineId = onlineId.Id,
				};
			}
			GoToArtist(artist);
		}
		public void GoToArtist(Artist artist)
		{
			if (artist == null)
				return;

			NavigationController.PushViewController(new ArtistDetailViewController
			{
				Artist = artist
			}, true);
		}
	}
}