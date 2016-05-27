using System;
using CoreGraphics;
using MusicPlayer.Data;
using MusicPlayer.Cells;
using MusicPlayer.iOS.UI;
using MusicPlayer.iOS.ViewControllers;
using MusicPlayer.ViewModels;
using MusicPlayer.Models;
using UIKit;

namespace MusicPlayer.iOS
{
	public class AlbumDetailsViewController : BaseTableViewController
	{
		AlbumDetailsViewModel model;

		public AlbumDetailsViewController()
		{
			model = new AlbumDetailsViewModel();
			this.EdgesForExtendedLayout = UIRectEdge.All;
		}

		public virtual Album Album
		{
			set
			{
				var onlineAlbum = value as OnlineAlbum;
				if (onlineAlbum != null)
				{
					model = new OnlineAlbumDetailsViewModel();
				}
				model.Album = value;
				Title = value.Name;
			}
		}

		public virtual Artist Artist
		{
			set
			{
				var albumId = Database.Main.TablesAsync<Song>().Where(x => x.ArtistId == value.Id).FirstAsync().Result.AlbumId;
				Album = Database.Main.GetObject<Album>(albumId);
			}
		}
		AlbumHeaderView header;
		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = model;
			TableView.SectionIndexBackgroundColor = UIColor.Clear;
			TableView.TableHeaderView  = header = new AlbumHeaderView(model.Album)
			{
				Frame = new CGRect(0, 0, 320, 320),
			};
			this.StyleViewController();
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			TableView.ContentOffset = new CGPoint(0, 280);
			header.MoreTapped = (b) => PopupManager.Shared.Show (model.Album, b);
		}

		public override void SetupEvents()
		{
			base.SetupEvents();
			model.CellFor += item => new AlbumSongCell(item);
		}

		public override void TeardownEvents()
		{
			base.TeardownEvents();
			model?.ClearEvents();
			header.MoreTapped = null;
		}
	}
}