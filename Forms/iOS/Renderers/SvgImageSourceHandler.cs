using System;
using System.Threading;
using System.Threading.Tasks;
using MusicPlayer.Forms;
using MusicPlayer.Forms.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportImageSourceHandler(typeof(SvgImageSource), typeof(SvgImageSourceHandler))]
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
}
