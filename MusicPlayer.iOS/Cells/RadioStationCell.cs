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
	internal class RadioStationCell : BaseCell
	{
		#region implemented abstract members of BaseCell

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(RadioStationTableViewCell.Key) as RadioStationTableViewCell ??
						new RadioStationTableViewCell();
			cell.BindingContext = BindingContext as RadioStation;
			cell.ApplyStyle(tv);
			return cell;
		}

		#endregion

		class RadioStationTableViewCell : MediaItemCell
		{
			public const string Key = "RadioStationCell";

			public RadioStationTableViewCell() : base(Key)
			{
			}

			RadioStation bindingContext;

			public RadioStation BindingContext
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

			async void SetValues(RadioStation song)
			{
				if (song == null)
					return;
				SetText(song);
				ImageView.Image = Images.GetDefaultSongImage(ImageWidth);


				ImageView.Image = Images.GetDefaultAlbumArt(ImageWidth);
				var artUrl = await ArtworkManager.Shared.GetArtwork(song);
				if (!string.IsNullOrWhiteSpace(artUrl))
					ImageView.SetImage(NSUrl.FromString(artUrl), Images.GetDefaultAlbumArt(ImageWidth));
			}
		}
	}
}