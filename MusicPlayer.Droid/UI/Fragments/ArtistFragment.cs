using System;
using Android.App;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using SimpleDatabase;

namespace MusicPlayer
{
	public class ArtistFragment : ListFragment
	{
		ArtistViewModel Model;
		public ArtistFragment ()
		{

		}

		public override void OnCreate (Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RetainInstance = true;
		}

		public override void OnViewCreated (Android.Views.View view, Android.OS.Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);
			if (ListAdapter == null)
			{
				ListAdapter = Model = new ArtistViewModel()
				{
					Context = App.Context,
					ListView = ListView,
				};
				Model.CellFor += (Artist item) =>
				{
					return new ArtistCell { Artist = item };
				};
				Model.ItemSelected += (sender, e) =>
				{
					App.DoPushFragment(new ArtistDetailsFragment { Artist = e.Data });
				};
			}
			else
			{
				Model.Context = Activity;
				Model.ListView = ListView;
			}
			this.SetListShown (true);
		}
	}
}

