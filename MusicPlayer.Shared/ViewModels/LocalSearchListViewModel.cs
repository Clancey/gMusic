using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Data;
using MusicPlayer.Models;
using SimpleDatabase;
using MusicPlayer.Managers;

namespace MusicPlayer.ViewModels
{
	class LocalSearchListViewModel : SearchListViewModel
	{
		string searchString;
		public async void Search(string query)
		{
			searchString = query;
			Results = null;
			var result = new SearchResults();
			if (string.IsNullOrWhiteSpace(query))
			{
				Results = result;
				return;
			}
			try
			{
				result.Songs =
					await
						Database.Main.QueryAsync<Song>("select * from song where " +
														string.Format("Name like ('%{0}%') or Artist  like ('%{0}%')" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), searchString));
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			try
			{
				result.Artist =
						await Database.Main.QueryAsync<Artist>("select * from Artist where " +
														string.Format("Name like ('%{0}%')" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), searchString));
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}

			try
			{
				result.Albums =
						await Database.Main.QueryAsync<Album>("select * from Album where " +
														string.Format("Name like ('%{0}%')" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), searchString));
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}

			try
			{
				result.Playlists =
					await Database.Main.QueryAsync<Playlist>("select * from Playlist where " +
													string.Format("Name like ('%{0}%')" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), searchString));
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			Results = result;
		}

		public GroupInfo GetArtist()
		{
			return new GroupInfo() { Filter = string.Format("Name like ('%{0}%')" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), searchString) };
		}

		public GroupInfo GetAlbum()
		{
			return new GroupInfo() { Filter = string.Format("Name like ('%{0}%')" + ((Settings.ShowOfflineOnly) ? " and OfflineCount > 0" : ""), searchString) };
		}

		public GroupInfo GetSongs()
		{
			return new GroupInfo() { Filter = string.Format("Title like ('%{0}%') or Artist  like ('%{0}%')" + ((Settings.ShowOfflineOnly) ? " and IsLocal = 1" : ""), searchString) };
		}
	}
}
