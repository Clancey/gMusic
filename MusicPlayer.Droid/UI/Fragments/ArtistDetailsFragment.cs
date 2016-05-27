using System;
using Android.App;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using SimpleDatabase;

namespace MusicPlayer
{
	public class ArtistDetailsFragment : ListFragment
	{
		AlbumViewModel Model;
		public ArtistDetailsFragment ()
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
				Model.CellFor += (item) => {
					return new AlbumCell{Album = item};
				};
				if (groupInfo != null)
					GroupInfo = groupInfo;
			}
			this.SetListShown (true);
		}
		GroupInfo groupInfo;
		public GroupInfo GroupInfo {
			get { return Model.GroupInfo; }
			set { 
				groupInfo = value;
				if (Model == null)
					return;
				Model.GroupInfo = value;
				Model.OfflineGroupInfo = null;
			}
		}
		Artist artist;
		public Artist Artist {
			get { return artist; }
			set {
				artist = value;
				GroupInfo = new GroupInfo {
					Filter = "Id in (select distinct AlbumId from song where ArtistId = ?)",
					OrderBy = "Year, NameNorm",
					Params = artist.Id,
				};
			}
		}
	}
}

