using System;
using MusicPlayer.Models;
using AppKit;

namespace MusicPlayer
{
	public class ArtistAlbumsListView : BaseTableView<ArtistAlbumsViewModel,Album>
	{
		public ArtistAlbumsListView (Artist artist)
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
			Model = new ArtistAlbumsViewModel{
				Artist = artist,
			};
			Model.CellFor += (item) => new AlbumDetailsCell {
				BindingContext = item,
			};
		}
		public Artist Artist
		{
			get { return Model.Artist; }
			set { 
				Model.Artist = value;
				TableView.ReloadData ();
			}
		}
	}
}

