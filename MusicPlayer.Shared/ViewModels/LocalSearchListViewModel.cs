using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Data;
using MusicPlayer.Models;
using SimpleDatabase;

namespace MusicPlayer.ViewModels
{
    class LocalSearchListViewModel : SearchListViewModel
    {
	    string searchString;
	    public async void Search(string query)
	    {
			searchString = $"%{query}%";
		    Results = null;
		    var result = new SearchResults();
		    result.Songs =
			    await
				    Database.Main.QueryAsync<Song>("select * from song where Name like (?) or Artist  like (?)" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), searchString,searchString);
			result.Artist =
					await Database.Main.QueryAsync<Artist>("select * from Artist where Name like (?)" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), searchString);
			result.Albums =
					await Database.Main.QueryAsync<Album>("select * from Album where Name like (?)" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), searchString);
			result.Playlists =
					await Database.Main.QueryAsync<Playlist>("select * from Playlist where Name like (?)" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), searchString);
			Results = result;
	    }

		public GroupInfo GetArtist()
		{
			return new GroupInfo() { Filter = "Name like (@SearchString)" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), Params = { { "@SearchString", "searchString" } } };
		}

		public GroupInfo GetAlbum()
		{
			return new GroupInfo() { Filter = "Name like (@SearchString)" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), Params = { { "@SearchString", "searchString" } }  };
		}

		public GroupInfo GetSongs()
		{
			return new GroupInfo() { Filter ="Title like (@SearchString) or Artist  like (@SearchString)" + ((Settings.ShowOfflineOnly) ? " and IsLocal = 1" : ""), Params = { { "@SearchString", "searchString" } }  };
		}
	}
}
