using System;
using System.IO;
using NGraphics;

namespace UIKit
{
	public static class NGraphicsExtensions
	{
		static readonly IPlatform Platform = new ApplePlatform();
		public static readonly double Scale = (double) UIScreen.MainScreen.Scale;

		public static void LoadSvg(this UIImageView imageView, string svg,UIImageRenderingMode renderingMode = UIImageRenderingMode.Automatic)
		{
			var s = imageView.Bounds.Size;
			LoadSvg(imageView, svg, new Size(s.Width, s.Height),renderingMode);
		}

		public static void LoadSvg(this UIImageView imageView, string svg, Size size,
			UIImageRenderingMode renderingMode = UIImageRenderingMode.Automatic)
		{
			var image = svg.LoadImageFromSvg(size, renderingMode);
			imageView.Image = image;
		}

		public static UIImage LoadImageFromSvg(this string svg, Size size,
			UIImageRenderingMode renderingMode = UIImageRenderingMode.Automatic)
		{
			try
			{
				var fileName = System.IO.Path.GetFileNameWithoutExtension(svg);
				using (var file = File.OpenText(svg))
				{
					var graphic = Graphic.LoadSvg(file);
					//Shame on Size not being Equatable ;)
					if (size.Width <= 0 || size.Height <= 0)
						size = graphic.Size;
					var gSize = graphic.Size;
					if (gSize.Width > size.Width || size.Height > gSize.Height)
					{
						var ratioX = size.Width/gSize.Width;
						var ratioY = size.Height/gSize.Height;
						var ratio = Math.Min(ratioY, ratioX);
						graphic.Size = size = new Size(gSize.Width*ratio, gSize.Height*ratio);
					}
					var c = Platform.CreateImageCanvas(size, Scale);
					graphic.Draw(c);
					var image = c.GetImage().GetUIImage();
					if (renderingMode != UIImageRenderingMode.Automatic)
						image = image.ImageWithRenderingMode(renderingMode);
					image.AccessibilityIdentifier = fileName;
					return image;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Console.WriteLine("Failed parsing: {0}", svg);
				throw;
			}
		}
	}
}