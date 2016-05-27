using System;
using AppKit;
using System.Threading.Tasks;
using MusicPlayer.Models;
using MusicPlayer.Managers;
using Akavache;
using Splat;
using System.Reactive.Linq;
using Foundation;

namespace MusicPlayer
{
	public static class ImageViewExtensions
	{
		public static async Task LoadFromUrl(this NSImageView imageView, string url, NSImage defaultImage = null)
		{
			try{
				if(imageView.Identifier != url)
					imageView.Image = defaultImage;
				var width = (float)imageView.Bounds.Width;
				if (!string.IsNullOrWhiteSpace (url)) {
					imageView.Identifier = url;
					imageView.Image = (await BlobCache.LocalMachine.LoadImageFromUrl (url, desiredWidth: width)).ToNative ();
				}
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}
		}
		public static Task LoadFromItem(this NSImageView imageView, MediaItemBase item, NSImage defaultImage = null)
		{
			var width = (float)imageView.Bounds.Width;
			return imageView.LoadFromItem (item, width, defaultImage);
		}
		public static async Task LoadFromItem(this NSImageView imageView, MediaItemBase item,float imageWidth,  NSImage defaultImage = null)
		{
			try{
				if (defaultImage == null)
					defaultImage = Images.GetDefaultAlbumArt (imageWidth);
				if(imageView.Identifier != item?.Id)
					imageView.Image = defaultImage;
				if (item == null)
					return;
				imageView.Identifier = item.Id;
				var image = await item.GetLocalImage (imageWidth);
				if (imageView.Identifier != item.Id)
					return;
				if (image != null) {
					imageView.Image = image;
				} else {
					var artUrl = await ArtworkManager.Shared.GetArtwork (item);
					if (imageView.Identifier != item.Id)
						return;
					
					if (string.IsNullOrWhiteSpace (artUrl))
						return;
					var bitmap = (await BlobCache.LocalMachine.LoadImageFromUrl (artUrl, desiredWidth: imageWidth));
					if (imageView.Identifier != item.Id)
						return;
					imageView.Image = bitmap.ToNative ();
				}
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}

		}
	}
}

