using System;
using CoreGraphics;
using UIKit;
using MusicPlayer.Models;
using Foundation;
using Haneke;
using MusicPlayer.Cells;
using MusicPlayer.Managers;

namespace MusicPlayer.iOS
{
	public class SongCell : BaseCell
	{
		#region implemented abstract members of BaseCell

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(SongTableViewCell.Key) as SongTableViewCell ?? new SongTableViewCell();
			cell.BindingContext = BindingContext as Song;
			cell.ApplyStyle(tv);
			return cell;
		}

		#endregion

		public class SongTableViewCell : MediaItemCell
		{
			public const string Key = "SongCell";

			public SongTableViewCell() : base(Key)
			{
			}

			WeakReference bindingContext;

			public Song BindingContext
			{
				get { return bindingContext?.Target as Song; }
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
				var song = BindingContext;
				if (song == null)
					return;
			}

			async void SetValues(Song song)
			{
				if (song == null)
					return;
				ShowOffline = song.OfflineCount > 0;
				MediaTypeImage.Hidden = !song.HasVideo;
				SetText(song);
				var locaImage = await song.GetLocalImage(ImageWidth);
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