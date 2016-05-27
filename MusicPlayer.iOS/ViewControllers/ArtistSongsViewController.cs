using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer.iOS.ViewControllers
{
	class ArtistSongsViewController : BaseTableViewController
	{
		
		ArtistSongsViewModel model;

		public ArtistSongsViewController()
		{
			model = new ArtistSongsViewModel();
			Title = model.Title;
		}

		public Artist Artist
		{
			get { return model.Artist; }
			set
			{
				model.Artist = value;
				Title = model.Title;
			}
		}

		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = model;
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
		}
	}
}