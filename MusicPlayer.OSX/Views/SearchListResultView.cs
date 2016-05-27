using System;
using MusicPlayer.Models;
using AppKit;
using MusicPlayer.Managers;
using SimpleTables;

namespace MusicPlayer
{
	public class SearchListResultView : BaseTableView<SearchListViewModel,MediaItemBase>
	{
		public string Title {
			get {
				return Model?.Title ?? "";
			}
		}

		WeakReference _parent;
		public SearchView Parent {
			get {
				return _parent?.Target as SearchView;;
			}
			set {
				_parent = new WeakReference(value);
			}
		}

		public SearchListResultView ()
		{

			TableView.AddColumn (new NSTableColumn ("Title"){ Title = "Title"});
			TableView.HeaderView = null;
			TableView.UsesAlternatingRowBackgroundColors = false;
			TableView.RowHeight = 55;
			TableView.SizeLastColumnToFit ();
			TableView.DoubleClick += TableView_DoubleClick;
		}

		async void TableView_DoubleClick (object sender, EventArgs e)
		{
			var item = Model.GetItem (TableView.SelectedRow);

			var onlineSong = item as OnlineSong;
			if (onlineSong != null)
			{
				if (!await MusicManager.Shared.AddTemp(onlineSong))
				{
					App.ShowAlert("Sorry", "There was an error playing this track");
					return;
				}
				await PlaybackManager.Shared.PlayNow(onlineSong, onlineSong.TrackData.MediaType == MediaType.Video);
				return;
			}
			var song = item as Song;
			if (song != null)
			{
				await PlaybackManager.Shared.PlayNow(song);
				return;
			}
			var radio = item as RadioStation;
			if(radio != null)
			{
				await PlaybackManager.Shared.Play(radio);
				return;
			}

		}
		protected override void ModelChanged ()
		{
			base.ModelChanged ();
			Model.ShowHeaders = true;
			Model.CellFor += (item) => {
				if(item is Song)
					return new SongSearchCell{BindingContext = item};
				return null;
			};
			Model.CellForHeader += (header) => new HeaderCell {
				Title = header
			};
			Model.ItemSelected += ModelOnItemSelected;
		}


		async void ModelOnItemSelected(object sender, EventArgs<MediaItemBase> eventArgs)
		{

			var song = eventArgs.Data as Song;
			if (song != null)
			{
				return;
			}

			TableView.DeselectAll (this);
			if (eventArgs.Data == null)
				return;

			var album = eventArgs.Data as Album;
			if (album != null)
			{
				var vc = new AlbumDetailViewController ().View;
				vc.Album = album;
				Parent.NavigationController.Push(vc);
				return;
			}

			var onlineArtist = eventArgs.Data as OnlineArtist;
			if (onlineArtist != null)
			{
//				var vc = new OnlineArtistDetailsViewController
//				{
//					Artist = onlineArtist,
//				};
//				this.NavigationController.PushViewController(vc, true);
				return;
			}

			var artist = eventArgs.Data as Artist;
			if (artist != null)
			{
//				var vc = new ArtistDetailViewController
//				{
//					Artist = artist,
//				};
//				this.NavigationController.PushViewController(vc,true);
				return;
			}

			var onlineRadio = eventArgs.Data as OnlineRadioStation;
			if (onlineRadio != null)
			{
				using (new Spinner("Creating Station"))
				{
					var statsion = await MusicManager.Shared.CreateRadioStation(onlineRadio);
					await PlaybackManager.Shared.Play(statsion);
				}
				return;
			}
			var radio = eventArgs.Data as RadioStation;
			if(radio != null)
			{
				return;
			}

			var onlinePlaylist = eventArgs.Data as OnlinePlaylist;
			if (onlinePlaylist != null)
			{
//				var vc = new OnlinePlaylistViewController()
//				{
//					Playlist = onlinePlaylist,
//				};
//				this.NavigationController.PushViewController(vc, true);
				return;
			}
			App.ShowNotImplmented();
		}
	}
}

