using System;
using Android.App;
using MusicPlayer.ViewModels;

namespace MusicPlayer
{
	public class PlaylistFragment : ListFragment
	{
		PlaylistViewModel Model;
		public PlaylistFragment ()
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
				ListAdapter = Model = new PlaylistViewModel
				{
					ListView = ListView,
					Context = App.Context,
				};
				Model.CellFor += (item) => {
					return new PlaylistCell{Playlist = item};
				};
			}
			this.SetListShown (true);
		}
	}
}

