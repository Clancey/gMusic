using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;
using SDWebImage;
using MusicPlayer.iOS;

namespace MusicPlayer.Cells
{
	internal class AlbumCell : BaseCell
	{
		#region implemented abstract members of BaseCell

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(AlbumTableViewCell.Key) as AlbumTableViewCell ?? new AlbumTableViewCell();
			cell.BindingContext = BindingContext as Album;
			cell.ApplyStyle(tv);
			return cell;
		}

		#endregion

		class AlbumTableViewCell : MediaItemCell
		{
			public const string Key = "AlbumCell";

			public AlbumTableViewCell() : base(Key)
			{
			}

			Album bindingContext;

			public Album BindingContext
			{
				get { return bindingContext; }
				set
				{
					if (BindingContext != null)
						ClearEvents();
					if (value == null)
					{
						bindingContext = null;
						return;
					}
					bindingContext = value;
					SetValues(value);
				}
			}

			public override void TappedAccessory(SimpleButton button)
			{
				PopupManager.Shared.Show(BindingContext, button);
			}

			void ClearEvents()
			{
				var song = BindingContext;
				if (song == null)
					return;
			}

			async void SetValues(Album album)
			{
				if (album == null)
					return;
				SetText(album);
				ShowOffline = album.OfflineCount > 0;
				ImageView.Image = Images.GetDefaultAlbumArt(ImageWidth);
				var locaImage = await album.GetLocalImage(ImageWidth);
				if (locaImage != null)
				{
					ImageView.Image = locaImage;
				}
				else
				{
					var artUrl = await ArtworkManager.Shared.GetArtwork(album);
					if (!string.IsNullOrWhiteSpace(artUrl))
						ImageView.SetImage(NSUrl.FromString(artUrl), Images.GetDefaultAlbumArt(ImageWidth));
				}
			}
		}
	}
}