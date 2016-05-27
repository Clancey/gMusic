using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Cells;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using SimpleTables;

namespace MusicPlayer.ViewModels
{
    class OnlinePlaylistViewModel : TableViewModel<OnlinePlaylistEntry>
	{
		public OnlinePlaylist Playlist { get; set; }
		Task<List<OnlinePlaylistEntry>> loadingTask;
	    bool isLoading;
		async Task Load()
		{
			if (loadingTask?.IsCompleted == false)
			{
				await loadingTask;
			}
			isLoading = true;
			loadingTask = MusicManager.Shared.GetOnlineTracks(Playlist);
			Playlist.Entries = await loadingTask;
			isLoading = false;
			ReloadData();
			return;
		}

	    public async Task Refresh()
	    {
		    if (Playlist.Entries == null)
			    await Load();
	    }
		public override int NumberOfSections()
		{
			return 1;
		}
		public override int RowsInSection(int section)
		{
			if (isLoading || Playlist.Entries == null)
				return 1;
			return Playlist.Entries.Count;
		}

		public override OnlinePlaylistEntry ItemFor(int section, int row)
		{
			return Playlist.Entries.Count <= row ? null : Playlist.Entries[row];
		}

		public override ICell GetICell(int section, int row)
		{
			if (isLoading)
				return new SpinnerCell();

			return base.GetICell (section, row);
		}

		public override string HeaderForSection(int section)
	    {
		    return "";
	    }

		#if __IOS

	    public override nfloat GetHeightForHeader(UIKit.UITableView tableView, nint section)
	    {
		    return 0;
	    }
		#endif

	    public override void RowSelected(OnlinePlaylistEntry item)
	    {
		    PlaybackManager.Shared.Play(item, Playlist);
	    }
	}
}