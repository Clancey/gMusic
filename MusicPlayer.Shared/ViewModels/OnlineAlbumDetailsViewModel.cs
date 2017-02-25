#if !FORMS
using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Cells;
using MusicPlayer.Models;
using SimpleTables;

namespace MusicPlayer.ViewModels
{
    class OnlineAlbumDetailsViewModel : AlbumDetailsViewModel
    {
	    public override int NumberOfSections()
	    {
		    return 1;
	    }
		public override int RowsInSection(int section)
		{
			if (isLoading)
				return 1;
			return Songs.Count;
		}

		public override Song ItemFor(int section, int row)
		{
			return Songs.Count <= row ? null : Songs[row];
		}

		public override ICell GetICell(int section, int row)
		{
			if (isLoading)
				return new SpinnerCell();
			return base.GetICell(section, row);
		}
	}
}
#endif