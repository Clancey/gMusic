using System;
using MusicPlayer.Managers;
using System.Threading.Tasks;

namespace MusicPlayer
{
	public class SearchManager : ManagerBase<SearchManager>
	{
		public SearchManager()
		{
			
		}

		public async Task<SearchResults> GetLocalResults(string query)
		{
			return null;
		}

		public async Task<SearchResults> GetSearchResults(string query, string providerId)
		{
			return null;
		}


	}
}

