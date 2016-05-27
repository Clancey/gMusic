using System;
using Android.App;
using Android.Widget;
using System.Timers;
using System.Collections.Generic;
using YoutubeExtractor;

namespace MusicPlayer.Droid
{
	public class SearchFragment : Fragment
	{
		//ListViewModel<MediaItem> Model;
		public SearchFragment ()
		{

		}
//		AutoCompleteTextView searchField;
//		ListView ListView;
//		Timer SearchTimer;
//		public override Android.Views.View OnCreateView (Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
//		{

//			var view = inflater.Inflate (Resource.Layout.Search, null, false);
//			searchField = view.FindViewById<AutoCompleteTextView> (Resource.Id.searchField);
//			searchField.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) => {
//				ProcSearch();
//			};
//			ListView = view.FindViewById<ListView> (Resource.Id.ListView);
//			return view;
//		}
//		public void ProcSearch()
//		{
//			if (SearchTimer == null) {
//				SearchTimer = new Timer (1500);
//				SearchTimer.Elapsed += (object sender, ElapsedEventArgs e) => {
//					App.InvokeOnMainThread(()=>{
//						Search(searchField.Text);
//					});

//				};
//			}
//			SearchTimer.Stop ();
//			SearchTimer.Start ();

//		}

//		public override void OnCreate (Android.OS.Bundle savedInstanceState)
//		{
//			base.OnCreate (savedInstanceState);
//			RetainInstance = true;
//		}

//		public override void OnViewCreated (Android.Views.View view, Android.OS.Bundle savedInstanceState)
//		{
//			base.OnViewCreated (view, savedInstanceState);
//			if (ListView.Adapter == null) {
//				ListView.Adapter = Model = new ListViewModel<MediaItem> (App.Context,ListView);
//				Model.CellFor += (item) => {
//					return new MediaItemCell{MediaItem = item};
//				};
//				Model.ItemSelected += async (object sender, SimpleTables.EventArgs<MediaItem> e) => {
//					WebviewFragment.ShowDownloadLinkMenu(e.Data);
//				};
//			}
//		}
//		WebSearchResult model;
//		public async void Search(string text)
//		{
//			try{
//				model = await Api.Current.WebSearch(text);

//				if(model != null)
//				{
//					Model.Items = model.YoutubeResults;
////					var vc = ViewControllers[0] as MediaItemResultViewController;
////					vc.LoadMore = ()=> loadMoreSoundCloud(text);
////					vc.SetResults(model.SoundCloud);
////
////					var vc2 = ViewControllers[1] as MediaItemResultViewController;
////					vc2.LoadMore = ()=> loadMoreYoutube(text,model.YoutubeNextToken);
////					vc2.SetResults(model.YoutubeResults);
//				}
//			}
//			catch(Exception ex) {
//				Console.WriteLine (ex);
//			}
//			finally {
//				//songResultsViewController.Model.IsSearching = albumResultsViewController.Model.IsSearching = artistResultsViewController.Model.IsSearching = false;
//			}

//		}
	}
}

