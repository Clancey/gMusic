#if !FORMS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicPlayer.Api;
using MusicPlayer.Managers;
using System.Timers;
using Localizations;

namespace MusicPlayer.ViewModels
{
	public class SearchViewModel
    {
	    SearchListViewModel[] viewModels;

	    public SearchListViewModel[] ViewModels
	    {
		    get { return viewModels ?? (viewModels = GenerateViewModels()); }
		    set { viewModels = value; }
	    }

	    SearchListViewModel[] GenerateViewModels()
	    {
		    var models = new List<SearchListViewModel>();
		    var apis = ApiManager.Shared.SearchableServiceTypes;
			models.AddRange(apis.Select(x=> new SearchListViewModel
			{
				ServiceType = x,
				Title = ApiManager.ServiceTitle(x),
			}));
			models.Add(new LocalSearchListViewModel
			{
				Title = Strings.Local,
				ServiceType = ServiceType.FileSystem
			});

		    return models.ToArray();
	    }

		public SearchListViewModel[] GetNewModels()
		{
			var existingModel = viewModels?.ToList () ?? new List<SearchListViewModel> ();
			if (!existingModel.Any ())
				return ViewModels;
			var models = new List<SearchListViewModel> ();
			var apis = ApiManager.Shared.SearchableServiceTypes;
			foreach (var api in apis) {
				if (existingModel.Any (x => x.ServiceType == api))
					continue;
				models.Add (new SearchListViewModel {
					ServiceType = api,
					Title = ApiManager.ServiceTitle (api),
				});
			}
			existingModel.AddRange (models);
			ViewModels = existingModel.ToArray ();
			return models.ToArray();


		}

	    public void Search(string text)
	    {
			searchText = text;
			searchTimer?.Stop ();
			ViewModels.ForEach(x =>
			{
				SearchModel(text,x);
			});
	    }

		string searchText;
		Timer searchTimer;
		public void KeyPressed(string text)
		{
			searchText = text;
			if (searchTimer == null) {
				searchTimer = new Timer (1500);
				searchTimer.Elapsed += (s, e) => App.RunOnMainThread(()=> Search (searchText));
			} else
				searchTimer.Stop ();
			searchTimer.Start ();
		}

	    async void SearchModel(string text, SearchListViewModel model)
	    {
		    var localModel = model as LocalSearchListViewModel;
		    if (localModel != null)
		    {
				localModel.Search(text);
                return;
		    }

			var api = ApiManager.Shared.GetMusicProvider(model.ServiceType);
		    model.Results = null;
		    model.IsSearching = true;
		    var result = await api.Search(text);
			if (searchText == text)
				model.Results = result;
	    }
		
    }
}
#endif