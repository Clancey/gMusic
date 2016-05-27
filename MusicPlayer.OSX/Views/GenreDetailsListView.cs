using System;
using MusicPlayer.Models;
using AppKit;

namespace MusicPlayer
{
	public class GenreDetailsListView : BaseTableView<GenreAlbumsViewModel,Album>
	{
		public GenreDetailsListView (Genre genre)
		{
			TableView.AddColumn (new NSTableColumn ("Artist"){ Title = "Artist"});
			TableView.HeaderView = null;
			TableView.UsesAlternatingRowBackgroundColors = false;
			TableView.RowHeight = 300;
			TableView.SizeLastColumnToFit ();
			TableView.DoubleClick += (object sender, EventArgs e) => {
				var item = Model.GetItem(TableView.SelectedRow);
				Model.PlayItem(item);
			};
			Model = new GenreAlbumsViewModel{
				Genre = genre,
			};
			Model.CellFor += (item) => new AlbumDetailsCell {
				BindingContext = item,
			};
		}
		public Genre Genre
		{
			get { return Model.Genre; }
			set { 
				Model.Genre = value;
				TableView.ReloadData ();
			}
		}

	}
}

