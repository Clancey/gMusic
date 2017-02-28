using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MusicPlayer.Forms;
using MusicPlayer.Forms.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportImageSourceHandler(typeof(SvgImageSource), typeof(SvgImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(FileImageSource), typeof(SvgFileImageSourceHandler))]
namespace MusicPlayer.Forms.iOS
{
	public class SvgImageSourceHandler : IImageSourceHandler
	{
		public Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1)
		{
			var svgSource = imagesource as SvgImageSource;
			return Task.FromResult(svgSource.SvgName.LoadImageFromSvg(new NGraphics.Size(svgSource.Size.Width, svgSource.Size.Height)));

		}
	}
	public class SvgFileImageSourceHandler : IImageSourceHandler
	{
		public Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1)
		{
			UIImage image = null;
			var filesource = imagesource as FileImageSource;
			if (filesource == null)
				return Task.FromResult(image);
			var file = filesource.File;
			if (file?.EndsWith(".svg", StringComparison.CurrentCultureIgnoreCase) ?? false)
			{
				image = Images.GetFileImagByName(file);
			} else if (!string.IsNullOrEmpty(file))
				image = File.Exists(file) ? new UIImage(file) : UIImage.FromBundle(file);
			return Task.FromResult(image);
		}
	}
}
