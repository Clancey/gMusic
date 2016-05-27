using System;
using AppKit;
using System.Linq;
using CoreGraphics;
using MusicPlayer.Managers;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class MutliImageMediaCellView : MediaCellView
	{
		public const string Key = "MutliImageMediaCellView";

		NSImageView[] ImageViews = new NSImageView[4];
		NSImage _defaultImage;

		string[] images;
		public MutliImageMediaCellView ()
		{
			Identifier = Key;
			Enumerable.Range(0, 4).ForEach(x =>
			{
				var imageView = new NSImageView {AlphaValue = 0,Frame = new CGRect(0,0,ImageWidth/2,ImageWidth/2)};
				ImageViews[x] = imageView;
				ImageView.AddSubview(imageView);
			});
		}

		public NSImage DefaultImage
		{
			get { return _defaultImage; }
			set
			{
				_defaultImage = value;
				ImageView.Image = DefaultImage;
			}
		}

		public string[] ImageUrls
		{
			get { return images; }
			set
			{
				try
				{
					images = value;
					ImageViews.ForEach(x=> x.AlphaValue = 0);
					if (images.Length == 0)
					{
						ImageView.Image = DefaultImage;
						return;
					}
					ImageView.Image = DefaultImage;
					Enumerable.Range(0, ImageViews.Length).ForEach(x =>
						{
							if (x >= images.Length - 1)
								return;
							var url = images[x];
							var imageView = ImageViews[x];
							imageView.AlphaValue = 1;
							imageView.LoadFromUrl(url, DefaultImage);
						});
				}
				catch(Exception ex)
				{
					ex.Data["imageCount"] = images?.Length;
					LogManager.Shared.Report(ex);
				}
			}
		}

		public override async void UpdateValues (MusicPlayer.Models.MediaItemBase item)
		{
			base.UpdateValues (item);

//			ShowOffline = playlist.OfflineCount > 0;
//			SetText(playlist);

			DefaultImage = Images.GetDefaultAlbumArt(ImageWidth);

			var playlist = item as Playlist;
			if(playlist != null)
				ImageUrls = await ArtworkManager.Shared.GetArtwork(playlist);
			var genre = item as Genre;
			if(genre != null)
				ImageUrls = await ArtworkManager.Shared.GetArtwork(genre);

		}


		public override void ResizeSubviewsWithOldSize (CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			var frame = ImageView.Bounds;
			frame.Width /= 2;
			frame.Height /= 2;

			ImageViews[0].Frame = frame;
			frame.X = frame.Width;
			ImageViews[1].Frame = frame;
			frame.Y = frame.Height;
			ImageViews[2].Frame = frame;
			frame.X = 0;
			ImageViews[3].Frame = frame;
		}
	}
}

