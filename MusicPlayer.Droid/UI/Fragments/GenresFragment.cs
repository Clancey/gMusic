using System;
using Android.App;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer
{
	public class GenresFragment : ListFragment
	{
		GenreViewModel Model;
		public GenresFragment ()
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
				ListAdapter = Model = new GenreViewModel
				{
					ListView = ListView,
					Context = App.Context,
				};
				Model.CellFor += (Genre item) => {
					return new GenreCell{Genre = item};
				};
			}
			this.SetListShown (true);
		}
	}
}

