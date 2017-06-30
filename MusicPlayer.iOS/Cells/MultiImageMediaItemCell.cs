using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using System.Linq;
using CoreGraphics;
using Foundation;
using SDWebImage;
using Xamarin;
using MusicPlayer.Managers;

namespace MusicPlayer.Cells
{
	class MultiImageMediaItemCell : MediaItemCell
	{
		string[] images;
		UIImageView[] ImageViews = new UIImageView[4];
		UIImage _defaultImage;

		public MultiImageMediaItemCell(string key) : base(key)
		{
			Enumerable.Range(0, 4).ForEach(x =>
			{
				var imageView = new UIImageView {Alpha = 0,Frame = new CGRect(0,0,ImageWidth/2,ImageWidth/2)};
				ImageViews[x] = imageView;
				ImageView.Add(imageView);
			});
		}

		public UIImage DefaultImage
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
					ImageViews.ForEach(x=> x.Alpha = 0);
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
						imageView.Alpha = 1;
						imageView.SetImage(NSUrl.FromString(url), DefaultImage);
					});
				}
				catch(Exception ex)
				{
					ex.Data["imageCount"] = images?.Length;
					LogManager.Shared.Report(ex);
				}
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
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
