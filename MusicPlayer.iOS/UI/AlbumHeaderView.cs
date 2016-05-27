using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using Foundation;
using Haneke;
using Localizations;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;

namespace MusicPlayer.iOS.UI
{
	internal class AlbumHeaderView : UIView
	{
		public const string Key = "artistHeaderCell";
		readonly BluredView Overlay;
		readonly UIImageView songsImage;

		UILabel SongsLabel { get; set; }
		UIImageView AlbumArtImage { get; set; }

		SimpleButton moreButton { get;set; }

		string imageUrl;
		bool hasSetImage;

		public Action<UIButton> MoreTapped { get; set; }

		public AlbumHeaderView(Album album)
		{
			AlbumArtImage = new UIImageView
			{
				ContentMode = UIViewContentMode.ScaleAspectFill,
			};

			Add(AlbumArtImage);


			Overlay = new BluredView();
			Add(Overlay);

			songsImage =
				new UIImageView(Images.GetMusicNotes(29).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate))
				{
					TintColor = UIColor.White,
				};
			Add(songsImage);

			SongsLabel = new UILabel().StyleAsSubText();

			Add(SongsLabel);

			Add(moreButton = new SimpleButton()
			{
				Image = Images.DisclosureImage.Value.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
				Tapped = (b) =>
				{
					var parent = b.Superview as AlbumHeaderView;
					parent?.MoreTapped?.Invoke(b);
				},
				Frame = new CGRect(0,0,44,44),

			});
			AccessibilityLabel = album.Name ?? "";
			//MainLabel.SizeToFit();
			
			if(album.TrackCount > 0)
				SongsLabel.Text = $"{album.TrackCount} {Strings.Songs}";
			SongsLabel.SizeToFit();

#if iPod

			AlbumArtImage.Image = album.MpItem.Artwork == null ? Images.AlbumArtDefault.Value : album.MpItem.Artwork.ImageWithSize(new CGSize(320,320))?? Images.AlbumArtDefault.Value;
#else
			SetImage(album);
#endif
		}
		async void SetImage(Album album)
		{

			AlbumArtImage.Image = Images.GetDefaultAlbumArt(Images.MaxScreenSize);
			var locaImage = await album.GetLocalImage(Images.MaxScreenSize);
			if (locaImage != null)
			{
				AlbumArtImage.Image = locaImage;
			}
			else
			{
				imageUrl = await ArtworkManager.Shared.GetArtwork(album);
				hasSetImage = false;
				SetNeedsLayout();
			}
		}

		public string SongText
		{
			set { SongsLabel.Text = $"{value} {Strings.Songs}"; }
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			const float HeaderSidePadding = 15f;
			const float SongIconWidth = 20f;
			const float BottomOffset = 20f;
			const float Padding = 5f;
			var bounds = Bounds;
			var frame = bounds;
			var midY = bounds.Height/2;
			frame.Width = frame.Height = NMath.Max(frame.Width, frame.Height);
			frame.Y = bounds.Height - frame.Height;
			AlbumArtImage.Frame = frame;
			if (!hasSetImage)
			{
				if (!string.IsNullOrWhiteSpace(imageUrl)) {
					AlbumArtImage.SetImage(NSUrl.FromString(imageUrl), Images.GetDefaultAlbumArt(320));
					hasSetImage = true;
				}
			}

			frame.X = bounds.X + HeaderSidePadding;
			frame.Width = frame.Height = SongIconWidth;
			frame.Y = bounds.Bottom - BottomOffset - frame.Height/2;
			songsImage.Frame = frame;

			frame.X = frame.Right + Padding;
			frame.Width = bounds.Width - (frame.X/2);
			frame.Height = SongsLabel.Frame.Height;
			frame.Y = bounds.Bottom - BottomOffset - (frame.Height/2);
			SongsLabel.Frame = frame;
			
			const float overlayHeight = 35f;
			Overlay.Frame = new CGRect(0, bounds.Height - overlayHeight, bounds.Width, overlayHeight);
			frame = Overlay.Frame;
			frame.Width = moreButton.Frame.Width ;
			frame.X = bounds.Width - frame.Width;
			moreButton.Center = frame.GetCenter();

		}
	}
}