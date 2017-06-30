using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using SDWebImage;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;
using MusicPlayer.iOS;

namespace MusicPlayer.Cells
{
	internal class PlaylistCell : BaseCell
	{
		#region implemented abstract members of BaseCell

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(PlaylistTableViewCell.Key) as PlaylistTableViewCell ?? new PlaylistTableViewCell();
			cell.BindingContext = BindingContext as Playlist;
			cell.ApplyStyle(tv);
			return cell;
		}

		#endregion

		class PlaylistTableViewCell : MultiImageMediaItemCell
		{
			public const string Key = "PlaylistCell";

			public PlaylistTableViewCell() : base(Key)
			{
			}

			WeakReference bindingContext;

			public Playlist BindingContext
			{
				get { return bindingContext?.Target as Playlist ?? null; }
				set
				{
					if (BindingContext != null)
						ClearEvents();
					if (value == null)
					{
						bindingContext = null;
						return;
					}
					bindingContext = new WeakReference(value);
					SetValues(value);
				}
			}

			public override void TappedAccessory(SimpleButton button)
			{
				PopupManager.Shared.Show(BindingContext, button);
			}

			void ClearEvents()
			{
				var playlist = BindingContext;
				if (playlist == null)
					return;
			}

			async void SetValues(Playlist playlist)
			{
				if (playlist == null)
					return;
				ShowOffline = playlist.OfflineCount > 0;
				SetText(playlist);

				DefaultImage = Images.GetDefaultAlbumArt(ImageWidth);
				var artUrls = await ArtworkManager.Shared.GetArtwork(playlist);
				ImageUrls = artUrls;
			}
		}
	}
}