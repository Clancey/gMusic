using System;
using UIKit;
using CoreImage;
using CoreGraphics;
using System.Threading.Tasks;
using MusicPlayer.Managers;

namespace MusicPlayer.iOS
{
	public static class ImageExtensions
	{
		public static Task<UIImage> BlurAsync(this UIImage image, float radius)
		{
			return Task.Factory.StartNew(() => image.Blur(radius));
		}

		static CIContext context;

		public static UIImage Blur(this UIImage image, float radius)
		{
			if (image == null)
				return null;
			try
			{
				var imageToBlur = CIImage.FromCGImage(image.CGImage);

				if (imageToBlur == null)
					return image;
				var transform = new CIAffineClamp
				{
					Transform = CGAffineTransform.MakeIdentity(),
					Image = imageToBlur
				};


				var gaussianBlurFilter = new CIGaussianBlur
				{
					Image = transform.OutputImage,
					Radius = radius
				};

				if (context == null)
					context = CIContext.FromOptions(null);

				var resultImage = gaussianBlurFilter.OutputImage;

				var finalImage = UIImage.FromImage(context.CreateCGImage(resultImage, new CGRect(CGPoint.Empty, image.Size)), 1,
					UIImageOrientation.Up);
				return finalImage;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				return image;
			}
		}

		public static UIImage Rotate(this UIImage image)
		{
			var h = image.Size.Height;
			var w = image.Size.Width;
			UIGraphics.BeginImageContextWithOptions(new CGSize(h, w), true, 1);
			image.Draw(new CGRect(0, 0, h, w));
			var result = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			return result;
		}
	}
}