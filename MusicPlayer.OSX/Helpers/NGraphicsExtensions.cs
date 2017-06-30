using System;
using System.IO;
using NGraphics;
using System.Linq;
using CoreGraphics;

namespace AppKit
{
	public static class NGraphicsExtensions
	{
		static readonly IPlatform Platform = new ApplePlatform();

		public static void LoadSvg(this NSImageView imageView, string svg)
		{
			if (string.IsNullOrWhiteSpace (svg))
				return;
			var s = imageView.Bounds.Size;
			LoadSvg(imageView, svg, new Size(s.Width, s.Height));
		}

		public static void LoadSvg(this NSImageView imageView, string svg, Size size , NSColor color = null)
		{
			var image = svg.LoadImageFromSvg(size,color);
			imageView.Image = image;
		}

		public static NSImage LoadImageFromSvg (this string svg, Size size, NSColor color = null)
		{
			var normal = svg.LoadImageFromSvg (size, 1, color).ToImageRep();
			var retina = svg.LoadImageFromSvg (size, 2, color).ToImageRep();
			var combinedImage = new NSImage ();
			combinedImage.MatchesOnMultipleResolution = true;
			combinedImage.AddRepresentations (new NSImageRep [] { normal, retina });
			return combinedImage;
		}

		static NSImageRep ToImageRep (this NSImage image)
		{
			var imageData = image.AsTiff ();
			var imageRep = NSBitmapImageRep.ImageRepFromData (imageData);
			return imageRep;
		}

		public static NSImage LoadImageFromSvg(this string svg, Size size, double scale,NSColor color = null)
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
					var c = Platform.CreateImageCanvas(size, scale);
					graphic.Draw(c);
					var image = c.GetImage().GetNSImage();
					if(color != null)
					{
						image.LockFocus();
						color.Set();
						var rect = new CGRect(CGPoint.Empty, image.Size);
						NSGraphics.RectFill(rect, NSCompositingOperation.SourceAtop);
						image.UnlockFocus();
					}
					image.AccessibilityDescription = fileName;
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