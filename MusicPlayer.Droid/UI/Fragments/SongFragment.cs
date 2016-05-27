using System;
using Android.App;
using Android.Content;
using MusicPlayer.Droid;
using MusicPlayer.ViewModels;
using NotificationManager = MusicPlayer.Managers.NotificationManager;

namespace MusicPlayer
{
	public class SongFragment : ListFragment
	{
		SongViewModel Model;
		public SongFragment ()
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
			this.ListView.FastScrollEnabled = true;

			if (ListAdapter == null) {
				ListAdapter = Model = new SongViewModel
				{
					Context = App.Context,
					ListView = ListView,
				};
				Model.CellFor += (item) => new SongCell { Song = item };

			}
			this.SetListShown (true);
		}

		public override void OnAttach(Context context)
		{
			base.OnAttach(context);
			Attached();
		}

		public override void OnAttach(Activity activity)
		{
			base.OnAttach(activity);
			Attached();
		}

		void Attached()
		{
			if(Model != null)
				Model.CellFor += (item) => new SongCell { Song = item };
			NotificationManager.Shared.SongDatabaseUpdated += SharedOnSongDatabaseUpdated;
		}

		void SharedOnSongDatabaseUpdated(object sender, EventArgs eventArgs)
		{
			Model.ReloadData();
		}

		public override void OnDetach ()
		{
			base.OnDetach ();
			Model.ClearEvents ();
			NotificationManager.Shared.SongDatabaseUpdated -= SharedOnSongDatabaseUpdated;
		}
	}
}

