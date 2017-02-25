#if !FORMS
using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using SimpleTables;

namespace MusicPlayer.ViewModels
{
    class DownloadViewModel : TableViewModel<Song>
	{

	    public override int RowsInSection(int section)
	    {
		   return  BackgroundDownloadManager.Shared.Count;
	    }

	    public override string HeaderForSection(int section)
	    {
		    return "";
	    }

	    public override int NumberOfSections()
	    {
		    return 1;
	    }
		
	    public override Song ItemFor(int section, int row)
	    {
		    var item = BackgroundDownloadManager.Shared.PendingItemForRow(row);
		    return item;
	    }
		
	}
}
#endif
