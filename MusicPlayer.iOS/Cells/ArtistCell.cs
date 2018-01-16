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
	internal class ArtistCell : BaseCell
	{
		#region implemented abstract members of BaseCell

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(ArtistTableViewCell.Key) as ArtistTableViewCell ?? new ArtistTableViewCell();
			cell.BindingContext = BindingContext as Artist;
			cell.ApplyStyle(tv);
			return cell;
		}

		#endregion

		class ArtistTableViewCell : MediaItemCell
		{
			public const string Key = "ArtistCell";

			public ArtistTableViewCell() : base(Key)
			{
			}

			Artist bindingContext;

			public Artist BindingContext
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
				var artist = BindingContext;
				if (artist == null)
					return;
			}

			async void SetValues(Artist artist)
			{
				if (artist == null)
					return;
				SetText(artist);
				ShowOffline = artist.OfflineCount > 0;
				ImageView.Image = Images.GetDefaultSongImage(ImageWidth);
				var artUrl = await ArtworkManager.Shared.GetArtwork(artist);
				if (!string.IsNullOrWhiteSpace(artUrl))
					ImageView.SetImage(NSUrl.FromString(artUrl), Images.GetDefaultSongImage(ImageWidth));
			}
		}
	}
}