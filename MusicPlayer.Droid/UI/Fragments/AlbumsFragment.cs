using System;
using Android.App;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer
{
	public class AlbumsFragment : ListFragment
	{
		AlbumViewModel Model;
		public AlbumsFragment ()
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
			if (ListAdapter == null) {
				ListAdapter = Model = new AlbumViewModel
				{
					ListView = ListView,
					Context = App.Context,
				};
				Model.CellFor += (Album item) => {
					return new AlbumCell{Album = item};
				};
			}
			this.SetListShown (true);
		}
	}
}

