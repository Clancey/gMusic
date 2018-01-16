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
	internal class GenreCell : BaseCell
	{
		#region implemented abstract members of BaseCell

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(GenreTableViewCell.Key) as GenreTableViewCell ?? new GenreTableViewCell();
			cell.BindingContext = BindingContext as Genre;
			cell.ApplyStyle(tv);
			return cell;
		}

		#endregion

		class GenreTableViewCell : MultiImageMediaItemCell
		{
			public const string Key = "GenreCell";

			public GenreTableViewCell() : base(Key)
			{
			}

			Genre bindingContext;
			public Genre BindingContext
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
				var genre = BindingContext;
				if (genre == null)
					return;
			}

			async void SetValues(Genre genre)
			{
				if (genre == null)
					return;
				ShowOffline = genre.OfflineCount > 0;
				SetText(genre);

				DefaultImage = Images.GetDefaultSongImage(ImageWidth);
				var artUrls = await ArtworkManager.Shared.GetArtwork(genre);
				ImageUrls = artUrls;
			}
		}
	}
}