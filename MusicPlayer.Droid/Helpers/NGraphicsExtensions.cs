using System;
using System.IO;
using NGraphics;
using Android.Widget;
using Android.Graphics;

namespace Android
{
	public static class NGraphicsExtensions
	{
		static readonly IPlatform Platform = new AndroidPlatform();
		static readonly double Scale = (double)MusicPlayer.App.Context.Resources.DisplayMetrics.Density;

		//public static void LoadSvg(this ImageView imageView, string svg)
		//{
		//	var s = imageView..Size;
		//	LoadSvg(imageView, svg, new Size(s.Width, s.Height),renderingMode);
		//}

		public static void LoadSvg(this ImageView imageView, string svg, Size size)
		{
			var image = svg.LoadImageFromSvg(size);
			imageView.SetImageBitmap(image);
		}

		public static Bitmap LoadImageFromSvg(this string svg, Size size)
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
					var image = c.GetImage() as BitmapImage;
					return image.Bitmap;
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