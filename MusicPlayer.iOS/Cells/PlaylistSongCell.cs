using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using Haneke;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;
using MusicPlayer.iOS;

namespace MusicPlayer.Cells
{
	internal class PlaylistSongCell : BaseCell
	{
		#region implemented abstract members of BaseCell

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(PlaylistTableViewCell.Key) as PlaylistTableViewCell ?? new PlaylistTableViewCell();
			cell.BindingContext = BindingContext as PlaylistSong;
			cell.ApplyStyle(tv);
			return cell;
		}

		#endregion

		class PlaylistTableViewCell : MediaItemCell
		{
			public const string Key = "PlaylistSongCell";

			public PlaylistTableViewCell() : base(Key)
			{
			}

			WeakReference bindingContext;

			public PlaylistSong BindingContext
			{
				get { return bindingContext?.Target as PlaylistSong ?? null; }
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
				PopupManager.Shared.Show(BindingContext.Song, button);
			}

			void ClearEvents()
			{
				var playlistSong = BindingContext;
				if (playlistSong == null)
					return;
			}

			async void SetValues(PlaylistSong playlistsong)
			{
				if (playlistsong == null)
					return;
				var song = playlistsong.Song;
				ShowOffline = song?.OfflineCount > 0;
				SetText(playlistsong.Song);
				var locaImage = song == null ? null : await song.GetLocalImage(ImageWidth);
				if (locaImage != null)
				{
					ImageView.Image = locaImage;
				}
				else
				{
					var artUrl = await ArtworkManager.Shared.GetArtwork(song);
					if (string.IsNullOrWhiteSpace(artUrl))
						ImageView.Image = Images.GetDefaultSongImage(ImageWidth);
					else
						ImageView.SetImage(NSUrl.FromString(artUrl), Images.GetDefaultSongImage(ImageWidth));
				}
			}
		}
	}
}