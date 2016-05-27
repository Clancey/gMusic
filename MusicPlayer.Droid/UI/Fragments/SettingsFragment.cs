using System;
using SimpleTables;
using Android.App;
using MusicPlayer.Data;
using SimpleTables.Cells;

namespace MusicPlayer
{
	public class SettingsFragment : ListFragment
	{
		ListViewModel<Cell> Model;
		public SettingsFragment ()
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
				ListAdapter = Model = new ListViewModel<Cell> (){
					Context = App.Context,
					ListView = ListView,
					Items = new System.Collections.Generic.List<Cell>{
						new StringCell("Repair Database",()=>{
							//Database.Main.FixOffline();
						}),
					}
				};
				Model.CellFor += (item) => {
					return item;
				};
			}
			this.SetListShown (true);
		}
	}
}

